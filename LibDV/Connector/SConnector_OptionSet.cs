using LibDV.OptionSet;
using LibUtil.UtilDisplay;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.Connector
{
    internal static partial class SConnector
    {
        internal static COptionSet GetOptionSetData(string logicalName)
            => new COptionSet(GetOptionSetMeta(logicalName));

        // This adds the provided labels to the option set with the provided logical name
        internal static void AddOptionSetData(string logicalName, List<string> labels)
        {
            var existingOptionSet = GetOptionSetData(logicalName);
            var labelsToAdd = existingOptionSet.MissingLabels(labels);

            if (labelsToAdd.Count() == 0)
            {
                SDisplay.Print($"No labels to add for option set: {logicalName}. Skipping.");
                return;
            }

            SDisplay.Print($"Updating the option set: {logicalName}.");
            SDisplay.Print($"Adding {labelsToAdd.Count()} labels.");

            SDisplay.StartProgressBar("Labels added", labelsToAdd.Count());

            // Relate the new labels
            foreach (string labelToAdd in labelsToAdd)
            {
                SDisplay.UpdateProgressBar();
                InsertOptionSetLabel(logicalName, labelToAdd);
            }

            SDisplay.StopProgressBar();
            SDisplay.Success("Successfully added {labelsToAdd.Count()} labels.");
        }

        private static void InsertOptionSetLabel(string logicalName, string label)
            => Service().Execute(
                new InsertOptionValueRequest()
                {
                    OptionSetName = logicalName,
                    Label = new Label(label, 1033),
                    SolutionUniqueName = SOLUTION,
                });

        private static OptionSetMetadata GetOptionSetMeta(string logicalName)
        {
            var request = new RetrieveOptionSetRequest
            {
                Name = logicalName
            };
            var response = (RetrieveOptionSetResponse)Service().Execute(request);

            if (response is null)
            {
                throw new Exception($"Failed to retrieve optionset: {logicalName}");
            }

            return (OptionSetMetadata)response.OptionSetMetadata;
        }
    }
}
