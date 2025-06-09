using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;

namespace SSDConsole.Dataverse
{
    internal static class DVRequest
    {
        private const int QUERY_LIMIT = 400;

        internal enum RequestType
        {
            Push,
            Update,
            Retrieve,
            Delete
        }

        #region pushrequest
        // The wrapper header for processing push requests
        internal static List<Guid> ProcessRequests_Push(IOrganizationService service, List<Entity> entities)
            => ProcessRequests<Entity, Guid>(service, entities, RequestType.Push); // redirect to generic process function
        private static List<Guid> ProcessRequest_Push(IOrganizationService service, List<Entity> entities) // the actual logic for sending a single push request
        {
            List<Guid> createdGuids = new List<Guid>();

            if (entities.Count == 0) return createdGuids;

            var logicalname = entities.ElementAt(0).LogicalName;

            var collection = new EntityCollection(entities)
            {
                EntityName = logicalname,
            };

            var request = new CreateMultipleRequest()
            {
                Targets = collection,
            };

            var response = (CreateMultipleResponse)service.Execute(request);

            createdGuids.AddRange(response.Ids);

            return createdGuids;
        }
        #endregion pushrequest

        #region updaterequest
        // The wrapper header for processing update requests
        internal static List<Entity> ProcessRequests_Update(IOrganizationService service, List<Entity> entities)
            => ProcessRequests<Entity, Entity>(service, entities, RequestType.Update); // redirect to generic process function
        private static List<Entity> ProcessRequest_Update(IOrganizationService service, List<Entity> entities) // the actual logic for sending a single update request
        {
            if (entities.Count == 0) return entities;

            // Retrieve the logical name
            var logicalname = entities.ElementAt(0).LogicalName;

            // Build a collection of the entities for updating
            var collection = new EntityCollection(entities.ToList())
            {
                EntityName = logicalname
            };

            // Create the request to update the entities
            var request = new UpdateMultipleRequest
            {
                Targets = collection
            };

            // Process the request
            service.Execute(request);

            return entities;
        }
        #endregion updaterequest

        #region retrieverequest
        // The wrapper header for processing retrieve requests
        internal static List<Entity> ProcessRequests_Retrieve(IOrganizationService service, List<Associable> associables)
            => ProcessRequests<Associable, Entity>(service, associables, RequestType.Retrieve); // redirect to generic process function
        private static List<Entity> ProcessRequest_Retrieve(IOrganizationService service, List<Associable> associables) // the actual logic for sending a single retrieve request
        {
            var results = new List<Entity>();

            // All associables should have the same logicalname and column set
            // WARNING: If the associables are of different types, then problems will arise
            var logicalname = associables.ElementAt(0).LogicalName();
            var columnset = associables.ElementAt(0).ColumnSet();

            int condition_limit = QUERY_LIMIT;

            int associable_current = 0;
            int associable_total = associables.Count();

            do
            {

                // Create a filter expression that will match on all associables of this type
                var criteria = new FilterExpression(LogicalOperator.Or);
                int conditions_current = 0;

                // Build the query from accessing the filters within each associable
                while (associable_current < associable_total)
                {
                    var associable = associables.ElementAt(associable_current);

                    var filter = associable.EqualExpression();

                    var conditions_new = filter.CountConditions();
                    if (conditions_new + conditions_current > condition_limit)
                    {
                        // Would adding the new conditions bring this query over the query limit?
                        // If so, stop adding conditions and send the current query before building a new one
                        break;
                    }

                    associable_current++;

                    // Add the equal expression for this associable to the criteria
                    // This ensures that the following retrieve request will return all matching entities
                    criteria.AddFilter(filter);
                    conditions_current += conditions_new;
                }

                var request = new RetrieveMultipleRequest()
                {
                    Query = new QueryExpression
                    {
                        EntityName = logicalname,
                        ColumnSet = columnset,
                        Criteria = criteria
                        
                    }
                };

                var response = (RetrieveMultipleResponse)service.Execute(request);
                var collection = response?.EntityCollection;

                if (collection is not null && collection.Entities.Count > 0)
                {
                    // Add these entities to the results to be returned
                    results.AddRange(collection.Entities);
                }

            } while (associable_current < associable_total); // keep sending queries until all associables have been queried

            return results.Distinct().ToList();
        }
        #endregion retrieverequest

        #region deleterequest
        internal static void ProcessRequests_Delete(IOrganizationService service, List<Entity> entities)
            => ProcessRequests<Entity, Entity>(service, entities, RequestType.Delete); // redirect to generic process function
        private static void ProcessRequest_Delete(IOrganizationService service, List<Entity> entities) // the actual logic for sending a single delete request
        {
            var entityReferences = entities.Select(e => e.ToEntityReference()).ToList();
            var request = new DeleteMultipleRequest
            {
                Targets = new EntityReferenceCollection(entityReferences)
            };
            service.Execute(request);
        }
        #endregion deleterequest

        #region generalrequests
            // The function to process generic requests
        private static List<R> ProcessRequests<T, R>(IOrganizationService service, List<T> items, RequestType requestType)
        {
            var results = new List<R>();

            if (items.Count == 0) return results;

            var partitions = PartitionList<T>(items);
            var current = 0;
            var total = items.Count;

            Console.WriteLine($"Processing {requestType.ToString().ToLower()} request. Handling {total} items.");

            foreach (var partition in partitions)
            {
                results.AddRange(ProcessRequest<T, R>(service, partition, requestType));
                current += partition.Count();
                Console.CursorLeft = 0;
                Console.Write($"Processed items: {current} / {total}.");
            }

            Console.WriteLine();
            Console.WriteLine($"{requestType.ToString()} Request processed.");

            return results;
        }

        // The function to process a single generic request (a partition)
        private static List<R> ProcessRequest<T, R>(IOrganizationService service, List<T> items, RequestType requestType)
        {
            var results = new List<R>();
            if (items.Count == 0) return results;

            var in_exc = new Exception($"ProcessRequest(): {requestType} request processed but items are invalid type: {items.GetType()}");
            var out_exc = new Exception($"ProcessRequest(): {requestType} request processed but output is invalid!");

            List<R>? output = null;

            switch (requestType)
            {
                case RequestType.Push:
                case RequestType.Update:
                    var entities = items as List<Entity>;
                    if (entities is null) throw in_exc;

                    if (requestType == RequestType.Push) output = ProcessRequest_Push(service, entities) as List<R>;
                    if (requestType == RequestType.Update) output = ProcessRequest_Update(service, entities) as List<R>;

                    if (output is null) throw out_exc;
                    results.AddRange(output);
                    break;
                case RequestType.Retrieve:
                    var associables = items as List<Associable>;
                    if (associables is null) throw in_exc;

                    output = ProcessRequest_Retrieve(service, associables) as List<R>;
                    
                    if (output is null) throw out_exc;
                    results.AddRange(output);
                    break;
                case RequestType.Delete:
                    var entitiesToDelete = items as List<Entity>;
                    if (entitiesToDelete is null) throw in_exc;
                    ProcessRequest_Delete(service, entitiesToDelete);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected RequestType in ProcessRequests(): {requestType}");
            }
            return results;
        }
        #endregion generalrequests

        #region util

        // PartitionList() divides the provided list into a number of sublists depending on the partitionSize
        private static List<List<Entity>> PartitionList(List<Entity> list, int partitionSize = QUERY_LIMIT)
            => PartitionList<Entity>(list, partitionSize);
        private static List<List<T>> PartitionList<T>(List<T> list, int partitionSize = QUERY_LIMIT)
        {
            List<List<T>> results = new List<List<T>>();

            int numFullPartitions = list.Count / partitionSize;
            int remainingElements = list.Count % partitionSize;
            int partitionCount = 0;

            while (partitionCount < numFullPartitions)
            {
                int index = partitionCount * partitionSize;
                results.Add(list.GetRange(index, partitionSize));
                partitionCount++;
            }

            if (remainingElements > 0)
            {
                int endOfFullPartitions = numFullPartitions * partitionSize;
                results.Add(list.GetRange(endOfFullPartitions, remainingElements));
            }

            return results;
        }

        #endregion util
    }
}
