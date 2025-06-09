using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.SSDDisplay;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{
    internal static partial class DVConnector
    {

        internal static Dictionary<EntityType,List<AEPair>> CreateEntities(
            List<Associable> associables, Dictionary<EntityType, List<AEPair>> sorted_pairs)
        {
            var results = new Dictionary<EntityType, List<AEPair>>();

            var sorted_associables = DVAssociable.SortByType(associables);
            var entity_types = Enum.GetValues<EntityType>();

            foreach ( var entity_type in entity_types )
            {
                var logicalname = entity_type.LogicalName();
                Display.Print($"Checking to see if entities of type {logicalname} need to be created.");

                var a_list = sorted_associables[entity_type];
                var e_list = sorted_pairs[entity_type];
                results[entity_type] = new List<AEPair>();

                if (a_list == null)
                {
                    Display.Print($"No associables of type {logicalname}. Skipping.");
                    continue;
                }

                var associablesToCreate = new List<Associable>();

                if (e_list == null) // are there any paired entities?
                {
                    associablesToCreate = a_list;
                } else
                {
                    var e_associables = e_list.Select(p => p.Associable());
                    associablesToCreate = a_list.Except(e_associables).ToList();
                }

                // Does every associable have a matching entity?
                if (associablesToCreate.Count == 0)
                {
                    Display.Print($"All associables of type {logicalname} have matching entities. Skipping.",
                                    Display.MessageSeverity.Success);
                    continue;
                }

                // Create the basic entity objects
                results[entity_type] = CreateEntities(associablesToCreate);

                // Populate the entity objects with information from the related associables
                ApplyAttributes(results[entity_type]);
            }

            return results;
        }

        private static List<AEPair> CreateEntities(List<Associable> associables)
        {
            var pairs = new List<AEPair>();
            var total = associables.Count();
            if (total == 0) return pairs;
            var logicalname = associables[0].LogicalName();

            Display.Interrupt($"Creating {total} new entities of type {logicalname}.");
            Display.StartProgressBar("Entities created", total);

            foreach (var associable in associables)
            {
                var entity = new Entity(associable.LogicalName());
                pairs.Add(new AEPair(associable, entity));
                Display.UpdateProgressBar();
            }

            Display.StopProgressBar();
            Display.Interrupt($"Successfully created {total} new entities of type {logicalname}.",
                                    Display.MessageSeverity.Success);

            return pairs;
        }
    }
}
