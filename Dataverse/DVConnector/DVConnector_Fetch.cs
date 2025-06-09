using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.SSDDisplay;

namespace SSDConsole.Dataverse.DVConnector
{
    internal static partial class DVConnector
    {

        // Fetches all entities of all types from dataverse
        internal static Dictionary<EntityType, EntityCollection> FetchEntities()
            => FetchEntities(EntityType.Clinic, EntityType.Clinician, EntityType.MedicalGroup);

        // Fetches all entities of the specified types from dataverse
        internal static Dictionary<EntityType, EntityCollection> FetchEntities(params EntityType[] entityTypes)
            =>  entityTypes.ToDictionary(
                    t => t,
                    t => FetchEntities(t));

        // Fetches all entities of the specified type from dataverse
        internal static EntityCollection FetchEntities(EntityType entityType)
            => FetchEntities(entityType.QueryExpression(), entityType);

        // Fetches entities that match the specified query from dataverse
        internal static EntityCollection FetchEntities(QueryExpression query, EntityType? entityType = null, bool skipDisplay = true)
        {
            List<Entity> entities = new List<Entity>();

            query.PageInfo.PageNumber = 1;
            query.PageInfo.Count = PAGE_SIZE;

            var type_str = entityType is not null ? $" of type {entityType?.LogicalName()}" : "";
            
            if (!skipDisplay)
            {
                Display.Interrupt($"Fetching entities{type_str}.");
                Display.StartProgressBar("Entities fetched");
            }

            while (true)
            {
                // Fetch the records
                EntityCollection results = Service().RetrieveMultiple(query);

                entities.AddRange(results.Entities);

                if (!skipDisplay) Display.UpdateProgressBar(results.Entities.Count);

                // Break out of the loop if there are no more records 
                if (!results.MoreRecords)
                {
                    break;
                }

                // Update the cookie and page number for the next request
                query.PageInfo.PagingCookie = results.PagingCookie;
                query.PageInfo.PageNumber++;
            }

            Display.StopProgressBar();

            if (!skipDisplay)
            {
                if (entities.Count == 0)
                {
                    Display.Interrupt($"No entities found{type_str}.",
                                            Display.MessageSeverity.Warning);
                }
                else
                {
                    Display.Interrupt($"Fetched {entities.Count} entities{type_str}.");
                }
            }
            
            return new EntityCollection(entities);
        }
    }
}
