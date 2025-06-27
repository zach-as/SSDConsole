using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Rest;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Bson;

namespace LibDV.Connector
{

    internal static partial class SConnector
    {
        internal const string SOLUTION = "Sandbox";
        internal const string ENV = "sturtzsolutionsdev";
        internal const string RESOURCE = $"https://{ENV}.api.crm.dynamics.com";
        internal const string URI_API = "/api/associables/v9.2/";
        internal const string URI_REDIRECT = "http://localhost";
        internal const string AUTH_TYPE = "OAuth";
        internal const string ID_CLIENT = "f92d3a39-484c-44c2-a40b-1ea0ba740e32";
        internal const string ID_TENANT = "d4855b11-8b9b-42e2-bc58-dccca813fc2c";
        internal const string SCOPE = $"{RESOURCE}/user_impersonation";

        internal const int PAGE_SIZE = 5000; // the limit for page sizes when querying records
        internal const int CONDITION_LIMIT = 500; // the limit for the number of conditions in a single query

        private const string CONNECTION = $@"
        AuthTsype = {AUTH_TYPE};
        Url = {RESOURCE};
        AppId = {ID_CLIENT};
        RedirectUri = {URI_REDIRECT};
        LoginPrompt = Auto;
        RequireNewInstance = True;";

        internal static IOrganizationService? service;
        internal static IOrganizationService Service()
        {
            if (service is null)
            {
                service = new ServiceClient(CONNECTION);
            }
            return service;
        }

        /*#region entity
        
         static List<AEPair> CreateAndUpdateEntities(List<Associable> associables)
        {
            Display.Print("CreateAndUpdateEntities() called.");

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
            var allEntities = pairedEntities.Values.SelectMany(p => p)
                                .Concat(pairedNewEntities.Values.SelectMany(p => p))
                                .ToList();

            // Return the complete dictionary of entities
            return allEntities;
        }

        #endregion entity*/

        
    }
        
}