using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using SSDConsole.SSDDisplay;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{
    internal static partial class DVConnector
    {

        #region push_create
        internal static Dictionary<EntityType, EntityCollection> PushCreates(Dictionary<EntityType, List<AEPair>> data)
            => data.ToDictionary(
                pair => pair.Key,
                pair => PushCreates(pair.Value));

        private static EntityCollection PushCreates(List<AEPair> pairs)
            => PushCreates(pairs.EntityCollection());

        // This pushes creation of new entities to dataverse using pagination to avoid exceeding the maximum request size
        internal static EntityCollection PushCreates(EntityCollection collection)
        {
            var total_entities = collection.Entities.Count();
            
            if (total_entities == 0)
            {
                Display.Print($"No entities to push of type {collection.EntityName}. Skipping push.");
                return collection;
            }
            
            var entities_to_push = Util.SplitList(collection.Entities.ToList(), PAGE_SIZE);
            var entities_pushed = new List<Entity>();

            Display.Interrupt($"Pushing {total_entities} newly created entities of type {collection.EntityName} to Dataverse.");
            Display.StartProgressBar("Entities pushed", total_entities);

            foreach (var entity_list in entities_to_push)
            {
                var pushed_col = PushCreates(entity_list);
                entities_pushed.AddRange(pushed_col.Entities);
                Display.UpdateProgressBar(pushed_col.Entities.Count());
            }

            Display.StopProgressBar();
            Display.Interrupt($"Finished pushing {total_entities} entities of type {collection.EntityName}.",
                                Display.MessageSeverity.Success);
            return DVEntity.EntityCollection(entities_pushed);
        }
        private static EntityCollection PushCreates(List<Entity> entities)
        {
            if (entities.Count == 0) return new EntityCollection();
            var request = new CreateMultipleRequest()
            {
                Targets = DVEntity.EntityCollection(entities),
            };
            var response = (CreateMultipleResponse)Service().Execute(request);

            var entityType = entities.ElementAt(0).EntityType();

            // Build a query that retrieves all entities with IDs matching the newly created entities
            // We do this so that we can retrieve all relevant data regarding these entities, not just the IDs
            var query = entityType.QueryExpression();
            query.Criteria = DVFilter.FilterAny(entityType.IdAttribute(), response.Ids.ToList());

            return FetchEntities(query, entityType, true);
        }

        

        #endregion push_create
        #region push_update
        // This pushes updates to existing entities to dataverse
        internal static void PushUpdates(EntityCollection collection)
        {
            var total_entities = collection.Entities.Count();
            var entities_to_push = Util.SplitList(collection.Entities.ToList(), PAGE_SIZE);

            if (entities_to_push.Count() == 0 || entities_to_push.ElementAt(0).Count() == 0)
            {
                return; // no entities of this type to update
            }
            
            Display.Interrupt($"Updating {total_entities} entities of type {collection.EntityName}.");
            Display.StartProgressBar("Entities updated", total_entities);
            foreach (var entity_list in entities_to_push)
            {
                PushUpdates(entity_list);
                Display.UpdateProgressBar(entity_list.Count());
            }
            Display.StopProgressBar();
            Display.Interrupt($"Finished updating {total_entities} entities of type {collection.EntityName}.",
                                Display.MessageSeverity.Success);

        }
        private static void PushUpdates(List<Entity> entities)
        {
            if (entities.Count == 0) return;
            var collection = new EntityCollection(entities)
            {
                EntityName = entities.ElementAt(0).LogicalName,
            };
            var request = new UpdateMultipleRequest()
            {
                Targets = collection,
            };
            Service().Execute(request);
        }
        internal static void PushUpdates(Dictionary<EntityType, List<AEPair>> sorted_pairs)
            => sorted_pairs.Values.ToList()
                .ForEach(l => PushUpdates(l));

        private static void PushUpdates(List<AEPair> pairs)
            => PushUpdates(DVEntity.EntityCollection(
                pairs.Select(p => p.Entity())
                .ToList()));

        #endregion push_update
    }
}
