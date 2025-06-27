using LibUtil.UtilDisplay;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.Connector
{
    internal static partial class SConnector
    {
        // This adds the provided labels to the option set with the provided logical name
        internal static void AddOptionSetData(string logicalname, IEnumerable<string> labels)
        {
            SDisplay.Print($"Adding {labels.Count()} labels to the option set: {logicalname}.");

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

        internal static List<COptionSetEntry> GetOptionSetData(string logicalname)
            => GetOptionSetMeta(logicalname)
                .Options.Select(
                    o => new COptionSetEntry(
                    o.Label.UserLocalizedLabel.Label,
                    o.Value ?? 0))
                .ToList();
    }
    
}
