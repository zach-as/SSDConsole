using LibUtil.UtilDisplay;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using LibDV.DVEntity;
using System.Drawing.Printing;
using Microsoft.Xrm.Sdk.Messages;
using LibDV.EntityType;

namespace LibDV.Connector
{
    public static partial class SConnectorDV
    {

        #region fetch
        // public func to fetch existing entities from DV
        public static CEntitySuperSet FetchAllEntities()
        {
            var set = new CEntitySuperSet();
            var entityTypes = SEntityType.EntityTypes();
            foreach (var entityType in entityTypes)
            {
                var setOfType = FetchEntities(entityType);
                set.AddSet(setOfType);
            }
            return set;
        }
        public static CEntitySet FetchEntities(EEntityType entityType)
            => FetchEntities(entityType.QueryExpression());

        // internal func to fetch existing entities from DV
        internal static CEntitySet FetchEntities(QueryExpression query, bool skipDisplay = false)
        {
            var entities = new List<Entity>();
            var logicalName = query.EntityName;

            query.PageInfo.PageNumber = 1;
            query.PageInfo.Count = PAGE_SIZE;

            if (!skipDisplay)
            {
                SDisplay.Print($"Fetching entities of type {logicalName}.");
                SDisplay.StartProgressBar("Entities fetched");
            }

            while (true)
            {
                // Fetch the records
                EntityCollection results = Service().RetrieveMultiple(query);

                entities.AddRange(results.Entities);

                if (!skipDisplay) SDisplay.UpdateProgressBar(results.Entities.Count);

                // Break out of the loop if there are no more records 
                if (!results.MoreRecords)
                {
                    break;
                }

                // Update the cookie and page number for the next request
                query.PageInfo.PagingCookie = results.PagingCookie;
                query.PageInfo.PageNumber++;
            }


            if (!skipDisplay)
            {
                SDisplay.StopProgressBar();

                if (entities.Count == 0)
                {
                    SDisplay.Print($"No entities found of type {logicalName}.",
                                            SDisplay.MessageSeverity.Warning);
                }
                else
                {
                    SDisplay.Print($"Fetched {entities.Count} entities of type {logicalName}.");
                }
            }

            // return entities as CEntity wrapper
            return new CEntitySet(entities);
        }
        #endregion fetch

        #region push_update

        // Push entity updates to DV (entities must already exist in DV)
        public static void PushEntityUpdate(CEntitySuperSet set)
            => set.Sets().Values.ToList().ForEach(s => PushEntityUpdate(s));
        public static void PushEntityUpdate(CEntitySet set, int batchSize = PAGE_SIZE, int index = 0)
        {
            var entities = set.Entities();
            var entityCount = set.Count();
            var logicalName = set.LogicalName();

            if (entityCount == 0)
            {
                SDisplay.Print("No entities to update. Skipping.");
                return;
            }
            if (!set.AllExist())
                throw new ArgumentException("All entities must have a valid ID before pushing entity updates.");
            if (entityCount > batchSize) // set batch size to upper bounds of entity count if necessary
                batchSize = entities.Count;

            SDisplay.Print($"Pushing [ {index + batchSize} / {entityCount} ] entities of type {logicalName} to DV. {entityCount - index} remaining.");

            var batch = set.Subset(index, batchSize);
            PushEntityUpdate_Request(batch);

            if (index + batchSize >= entities.Count)
            {
                SDisplay.Success($"All {entityCount} entities of type {logicalName} successfully updated.");
                return;
            }

            // Recursively push the next batch
            PushEntityUpdate(set, batchSize, index + batchSize);
        }
        private static void PushEntityUpdate_Request(CEntitySet set)
        {
            var request = new UpdateMultipleRequest()
            {
                Targets = set.Collection(),
            };
            Service().Execute(request);
        }
        #endregion push_update

        #region push_create

        public static CEntitySuperSet PushEntityCreate(CEntitySuperSet sets)
        {
            var created = new CEntitySuperSet();
            foreach (var set in sets.Sets())
            {
                var createdSet = PushEntityCreate(set.Value);
                created.AddSet(createdSet);
            }
            return created;
        }

        // Push newly created entities to DV
        public static CEntitySet PushEntityCreate(CEntitySet set, int batchSize = PAGE_SIZE, int index = 0)
        {
            var entities = set.Entities();
            var entityCount = set.Count();
            var logicalName = set.LogicalName();

            if (entityCount == 0)
            {
                SDisplay.Print("No entities to create. Skipping.");
                return set;
            }
            if (set.AnyExists())
            {
                throw new ArgumentException("All entities must have not already exist before pushing entity creation.");
            }

            var batch = set.Subset(index, batchSize);
            var created = PushEntityCreate_Request(batch);

            if (index + batchSize >= entities.Count)
            {
                SDisplay.Success($"All {entityCount} entities of type {logicalName} successfully created.");
                return created;
            }

            // Recursively push the next batch
            var nextCreated = PushEntityCreate(set, batchSize, index + batchSize);
            created.AddSet(nextCreated);

            // Return the combined created entities (IDs included)
            return created;
        }
        private static CEntitySet PushEntityCreate_Request(CEntitySet set)
        {
            var logicalName = set.LogicalName();

            // Build and send the create multiple request
            var request = new CreateMultipleRequest
            {
                Targets = set.Collection(),
            };
            var response = (CreateMultipleResponse)Service().Execute(request);

            // Retrieve the created entities using the IDs returned in the response
            // We do this so that we can get the IDs of the newly created entities
            var query = set.QueryExpression();
            query.Criteria = new FilterExpression(LogicalOperator.Or);
            foreach (var id in response.Ids)
            {
                query.Criteria.AddCondition(logicalName + "id", ConditionOperator.Equal, id);
            }
            return FetchEntities(query, true);

        }

        #endregion push_create
    }
}
