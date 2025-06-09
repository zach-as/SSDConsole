using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Data.Associable;
using Microsoft.Crm.Sdk.Messages;

namespace SSDConsole.Dataverse.DVConnector
{
    internal static partial class DVConnector
    {
        #region entity
        
        // This creates the basic entity objects for later use
        internal static Dictionary<EntityType, List<AEPair>> CreateAndUpdateEntities(List<Associable> associables)
        {
            Console.WriteLine("CreateAndUpdateEntities() called.");

            // Retrieve all existing entities
            var existingEntities = FetchEntities();
            // Pair entities with provided asociables
            var pairedEntities = PairedList(associables, existingEntities);
            // Update entities that have matching associables
            ApplyAttributes(pairedEntities);
            // Push updated entities to dataverse
            PushUpdates(pairedEntities);
            // Create new entities for each associable that does not have a matching entity in Dataverse
            var newEntities = CreateEntities(associables, pairedEntities);
            // Push the newly created entities to dataverse
            var pushedEntities = PushCreates(newEntities);
            // Pair the newly created entities with the associables
            var pairedNewEntities = PairedList(associables, pushedEntities);
            // Merge the newly created entities with the existing paired entities
            var allEntities = pairedEntities.ToDictionary(
                                    p => p.Key, // key is the entity type 
                                    p => p.Value.Concat(pairedNewEntities[p.Key]).ToList()); // value is the concatenation of existing and new entities

            // Return the complete dictionary of entities
            return allEntities;
        }

        #endregion entity
    }
}