using System.Net.Http.Headers;
using LibUtil.UtilDisplay;
using LibCMS.Params;
using LibCMS.Record;
using LibCMS.Http;
using LibCMS.Data.Associable;

namespace LibCMS.Connector
{

    public static class SConnectorCMS
    {

        // The threshold for how much time must pass before records are forcefully updated
        private static TimeSpan MIN_UPDATE_TIME = TimeSpan.FromDays(1);

        #region Client
        // baseAddress should only be provided when initializing the client
        private static HttpClient? client;
        private static HttpClient Client()
        {
            if (client is null)
            {
                // Initialize a new HttpClient
                client = new HttpClient()
                {
                    Timeout = new TimeSpan(0, 2, 0)
                };
                // Relate the default headers to the new client
                AddDefaultHeaders();
            }
            return client;
        }
        #endregion Client

        #region Records
        private static CRecordOutput? records;
        private static async Task<CRecordOutput> Records()
        {
            await UpdateRecords();
            if (records is not null) return records;
            throw new Exception("Update records failed.");
        }
        // This performs a lengthy operation to update all records by querying CMS. This will easily take several minutes to complete.
        private static async Task UpdateRecords()
        {
            if (!ShouldUpdateRecords()) return;

            SDisplay.Print("Pulling records from CMS. This may take a few minutes.");

            CParameters parameters = new CParameters();
            parameters.Limit = 30;
            int? recordTotal = 30;
            int recordsRecorded = 0;

            do
            {
                var message = await Send(new CHttpRequest(parameters)); // query CMS

                var response = await CRecordResponse.BuildFromHttpResponse(message); // convert text to usable data

                if (response is null) throw new Exception("Failed to build record response from text.");
                if (recordTotal is null || recordTotal == 0) recordTotal = response.RecordCountDB; // note the total # of records

                if (!SDisplay.InProgress()) SDisplay.StartProgressBar("CMS records pulled and formatted", recordTotal);

                if(records is null) records = new CRecordOutput();
                records.AddRecordInput(response); // Convert the response to most usable form (clinicians, clinics, orgs, etc)

                int recordsPulled = response.Records().Count();
                
                SDisplay.UpdateProgressBar(recordsPulled);
                
                // Increment the offset so that the query will return the next set of records (default 2000 at a time)
                parameters.Offset += recordsPulled;

                // update the number of records recorded so we know when to stop looping
                recordsRecorded += recordsPulled;

                // keep looping until we have all the records
            } while (recordTotal is not null
                    && recordsRecorded < recordTotal);

            SDisplay.StopProgressBar();
            SDisplay.Print("Records from CMS pulled and updated.");

        }
        private static bool ShouldUpdateRecords()
        {
            if (records is null) return true;
            TimeSpan timeSinceUpdate = DateTime.UtcNow - records.TimeLastUpdated();
            if (timeSinceUpdate > MIN_UPDATE_TIME) return true;
            return false;
        }
        #endregion Records

        #region Headers
        private static HttpRequestHeaders Headers()
        {
                return Client().DefaultRequestHeaders;
        }

        private static void AddDefaultHeaders()
        {
            Headers().Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion Headers

        #region HttpMessaging
        private static async Task<HttpResponseMessage> Send(CHttpRequest request)
        {
            HttpResponseMessage response = await Client().SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                return new HttpResponseMessage()
                {
                    StatusCode = response.StatusCode,
                    Content = new StringContent($"Error: {response.StatusCode} - {response.ReasonPhrase}")
                };
            }
        }
        #endregion HttpMessaging

        #region PublicMethods
        public static async Task<List<CClinician>> GetClinicians()
            => (await Records()).Clinicians();
        public static async Task<List<CClinic>> GetClinics()
            => (await Records()).Clinics();
        public static async Task<List<CMedicalGroup>> GetMedicalGroups()
            => (await Records()).Organizations();

        public static async Task<List<CAssociable>> GetAssociables()
        {
            // Group the data into a list of CAssociable objects for processing
            List<CAssociable> associables = new List<CAssociable>();
            associables.AddRange(await GetClinicians());
            associables.AddRange(await GetClinics());
            associables.AddRange(await GetMedicalGroups());
            return associables;
        }

        #endregion PublicMethods

    }
}
