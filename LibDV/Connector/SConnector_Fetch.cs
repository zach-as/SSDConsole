using LibUtil.UtilDisplay;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using LibDV.Entity;

namespace LibDV.Connector
{
    internal static partial class SConnector
    {
        internal static List<CEntity> FetchEntities(QueryExpression query, bool skipDisplay = false)
        {
            var entities = new List<Microsoft.Xrm.Sdk.Entity>();
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
            return entities.ConvertEntities();
        }
    }
}
