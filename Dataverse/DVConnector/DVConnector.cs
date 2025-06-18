using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Bson;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.SSDDisplay;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{

    internal static partial class DVConnector
    {
        public const string SOLUTION = "Sandbox";
        public const string ENV = "sturtzsolutionsdev";
        public const string RESOURCE = $"https://{ENV}.api.crm.dynamics.com";
        public const string URI_API = "/api/associables/v9.2/";
        public const string URI_REDIRECT = "http://localhost";
        public const string AUTH_TYPE = "OAuth";
        public const string ID_CLIENT = "f92d3a39-484c-44c2-a40b-1ea0ba740e32";
        public const string ID_TENANT = "d4855b11-8b9b-42e2-bc58-dccca813fc2c";
        public const string SCOPE = $"{RESOURCE}/user_impersonation";

        public const int PAGE_SIZE = 5000; // the limit for page sizes when querying records
        public const int CONDITION_LIMIT = 500; // the limit for the number of conditions in a single query

        private const string CONNECTION = $@"
        AuthType = {AUTH_TYPE};
        Url = {RESOURCE};
        AppId = {ID_CLIENT};
        RedirectUri = {URI_REDIRECT};
        LoginPrompt = Auto;
        RequireNewInstance = True;";

        private static IOrganizationService? service;
        private static IOrganizationService Service()
        {
            if (service is null)
            {
                service = new ServiceClient(CONNECTION);
            }
            return service;
        }

        #region entity
        
        internal static List<AEPair> CreateAndUpdateEntities(List<Associable> associables)
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

        #endregion entity

        #region optionset
        // The "key" will be the label of the option set whereas the "value" will be the corresponding code
        internal static Dictionary<string, int> GetOptionSetData(string logicalname)
        {
            var meta = GetOptionSetMeta(logicalname);
            var options = meta.Options;
            return options.ToDictionary(
                o => o.Label.UserLocalizedLabel.Label,
                o => o.Value ?? 0);
        }

        // This adds the provided labels to the option set with the provided logical name
        internal static void AddOptionSetData(string logicalname, IEnumerable<string> labels)
        {
            Display.Print($"Adding {labels.Count()} labels to the option set: {logicalname}.");

            var meta = GetOptionSetMeta(logicalname);
            var existingLabels = meta.Options.Select(o => o.Label.UserLocalizedLabel.Label);
            var labelsToAdd = labels.Except(existingLabels); // only add labels that do not already exist

            int added = 0;
            int total = labelsToAdd.Count();

            // Relate the new labels
            foreach (string labelToAdd in labelsToAdd)
            {
                Console.CursorLeft = 0;
                Console.Write($"Labels added: {added} / {total}.");
                
                added++;

                InsertOptionSetLabel(logicalname, labelToAdd);
            }

            Console.WriteLine();

            Console.WriteLine("Label addition complete.");
        }

        private static InsertOptionValueResponse? InsertOptionSetLabel(string logicalname, string label)
        {
            // Create a request.
            InsertOptionValueRequest request = new()
            {
                OptionSetName = logicalname,
                Label = new Label(label, 1033),
                SolutionUniqueName = SOLUTION,
            };

            // Execute the request
            return (InsertOptionValueResponse)Service().Execute(request);
        }

        private static OptionSetMetadata GetOptionSetMeta(string logicalname)
        {
            var request = new RetrieveOptionSetRequest
            {
                Name = logicalname
            };
            var response = (RetrieveOptionSetResponse)Service().Execute(request);

            if (response is null)
            {
                throw new Exception($"Failed to retrieve optionset: {logicalname}");
            }

            return (OptionSetMetadata)response.OptionSetMetadata;
        }

        #endregion optionset
    }
}