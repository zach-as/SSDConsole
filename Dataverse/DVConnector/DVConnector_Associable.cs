using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.SSDDisplay;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{

    internal static partial class DVConnector
    {

        internal static Dictionary<EntityType, List<AEPair>> PairedList(List<Associable> associables, Dictionary<EntityType, EntityCollection> entities)
            => PairedList(PairEntitiesWithAssociables(associables, entities));
        internal static Dictionary<EntityType, List<AEPair>> PairedList(Dictionary<EntityType, Dictionary<Associable, Entity>> info)
            => info.ToDictionary(
                i => i.Key,
                i => i.Value
                     .Select(d => new AEPair(d.Key, d.Value))
                     .ToList());

        internal static Dictionary<EntityType,Dictionary<Associable, Entity>> PairEntitiesWithAssociables(List<Associable> associables, Dictionary<EntityType, EntityCollection> entities)
        {
            Display.Print("Pairing entities with associables.");

            var pairs = new Dictionary<EntityType, Dictionary<Associable, Entity>>();
            var entitiesWithNoMatch = new List<Entity>();

            // Create a list to store duplicate pairs for debugging
            var duplicatePairings = new List<AEDuplicate>();

            var infoid_nomatch = "nomatch"; // the id for the progress bar additional info: nomatch count
            var infoid_duplicate = "duplicate"; // the id for the progress bar additional info: duplicate count

            // Attempt to match all associables with entities into pairs
            foreach (var kvp in entities)
            {
                var entitiesOfTypeWithNoMatch = 0;
                var entitiesOfTypeWithDuplicate = 0;

                // Start the progress bar
                Display.StartProgressBar("Entities paired", 
                            new Display.ProgressBarInfo(infoid_nomatch, "Entities with no matches found"),
                            new Display.ProgressBarInfo(infoid_duplicate, "Duplicate matches found"));

                EntityType entityType = kvp.Key;
                EntityCollection collection = kvp.Value;

                pairs[entityType] = new Dictionary<Associable, Entity>();

                foreach (Entity entity in collection.Entities)
                {
                    var associable = FindMatchingAssociable(associables, entity);
                    
                    // No matching associable found
                    if (associable is null)
                    {
                        Display.UpdateProgressBar(infoid_nomatch);
                        entitiesWithNoMatch.Add(entity);
                        entitiesOfTypeWithNoMatch++;
                        continue;
                    }

                    if (pairs[entityType].ContainsKey(associable))
                    {
                        Display.UpdateProgressBar(infoid_duplicate);
                        var pair1 = new AEPair(associable, pairs[entityType][associable]);
                        var pair2 = new AEPair(associable, entity);
                        duplicatePairings.Add(new AEDuplicate(pair1, pair2));
                        continue;
                    }

                    // A matching associable is found, register the match
                    pairs[entityType][associable] = entity;
                    Display.UpdateProgressBar();
                }

                // Stop the progress bar
                Display.StopProgressBar();

                // Were any pairs made?
                if (pairs[entityType].Count() == 0)
                {
                    Display.Interrupt($"Failed to pair any entities of type {entityType.LogicalName()}",
                                        Display.MessageSeverity.Warning);
                } else
                {
                    Display.Interrupt($"Successfully paired {pairs.Count()} entities of type {entityType.LogicalName()}",
                                        Display.MessageSeverity.Success);
                }

                // Were any entities unable to find matching associables?
                if (entitiesOfTypeWithNoMatch > 0)
                {
                    Display.Interrupt($"Entities of type {entityType.LogicalName()} with no matches: {entitiesOfTypeWithNoMatch}.",
                                        Display.MessageSeverity.Warning);
                } else
                {
                    Display.Interrupt($"All entities of type {entityType.LogicalName()} have matches.",
                                        Display.MessageSeverity.Success);
                }

                // Were there any collisions, wherein multiple entities attempted to associate with the same associables?
                if (entitiesOfTypeWithDuplicate > 0)
                {
                    Display.Interrupt($"Entities of type {entityType.LogicalName()} with duplicates: {entitiesOfTypeWithDuplicate}",
                                        Display.MessageSeverity.Warning);
                } else
                {
                    Display.Interrupt($"Entities of type {entityType.LogicalName()} found no duplicates.",
                                        Display.MessageSeverity.Success);
                }
            }

            var pairedAssociables = pairs.Values.SelectMany(dict => dict.Keys);
            var associablesWithNoMatch = associables.Except(pairedAssociables);
            if (associablesWithNoMatch.Count() > 0)
            {
                Display.Interrupt($"Associables with no matches: {associablesWithNoMatch.Count()}.",
                                    Display.MessageSeverity.Warning);
            }

            return pairs;
        }

        private static Associable? FindMatchingAssociable(List<Associable> associables, Entity entity)
            => associables.FirstOrDefault(a => a.Matches(entity));

        
    }

}
