using System.Net.Http.Headers;
using LibUtil.UtilDisplay;
using LibCMS.Params;
using LibCMS.Record;
using LibCMS.Http;
using LibCMS.Data.Associable;
using System.Threading.Tasks;

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

            if (records is null) records = new CRecordOutput();
            var requests = await BuildRequests(parameters);

            SDisplay.Print($"Built {requests.Count} requests to pull records from CMS. Processing.");

            var responses = await Task.WhenAll(requests); // send all requests in parallel

            SDisplay.Print($"Received {responses.Length} responses from CMS. Processing records.");
            SDisplay.StartProgressBar("Processing record sets", responses.Length);

            foreach (var response in responses)
            {
                var recordResponse = await ProcessRecordResponse(response); // convert the response to usable data in a struct
                records.AddRecordInput(recordResponse); // Convert the data to most usable form (clinicians, clinics, medical groups, etc)
                SDisplay.UpdateProgressBar();
            }

            SDisplay.StopProgressBar();
            SDisplay.Print("Records from CMS queried and processed.");

            /*do
            {
                var message = Send(new CHttpRequest(parameters)); // query CMS
                var response = CRecordResponse.BuildFromHttpResponse(message); // convert text to usable data

                records.AddRecordInput(initResponse); // Convert the response to most usable form (clinicians, clinics, orgs, etc)

                var tasks = new List<Task<HttpResponseMessage>>(); // list of send tasks to run in parallel
                var responses = new List<CRecordResponse>(); // list of responses to convert into usable forms


                if (response is null) throw new Exception("Failed to build record httpResponse from text.");
                if (recordTotal is null || recordTotal == 0) recordTotal = response.RecordCountDB; // note the total # of records

                if (!SDisplay.InProgress()) SDisplay.StartProgressBar("CMS records pulled and formatted", recordTotal);


                int recordsPulled = response.Records().Count();
                
                SDisplay.UpdateProgressBar(recordsPulled);
                
                // Increment the offset so that the query will return the next set of records (default 2000 at a time)
                parameters.Offset += recordsPulled;

                // update the number of records recorded so we know when to stop looping
                recordsRecorded += recordsPulled;

                // keep looping until we have all the records
            } while (recordTotal is not null
                    && recordsRecorded < recordTotal);

            SDisplay.StopProgressBar();*/

        }

        private static async Task<HttpResponseMessage> SendRequest(CParameters parameters)
        {
            var request = await Send(new CHttpRequest(parameters));
            
            if (request is null) throw new Exception("Failed to send request to CMS.");
            if (!request.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send request to CMS. Status code: {request.StatusCode}, Reason: {request.ReasonPhrase}");
            }

            if (SDisplay.InProgress())
                SDisplay.UpdateProgressBar(parameters.Limit ?? 2000);

            return request;
        }
        private static async Task<CRecordResponse> ProcessRecordResponse(Task<HttpResponseMessage> response)
            => await ProcessRecordResponse(await response);
        private static async Task<CRecordResponse> ProcessRecordResponse(HttpResponseMessage response)
        {
            var recordResponse = await BuildRecordResponse(response);
            if (recordResponse is null) throw new Exception("Failed to build record CRecordResponse from HttpResponse.");
            return recordResponse;
        }
        private static Task<CRecordResponse?> BuildRecordResponse(HttpResponseMessage response)
            => CRecordResponse.BuildFromHttpResponse(response);

        // This builds a list of requests to send to the CMS based on the parameters provided.
        // Note that none of the requests are sent until the list is returned and Task.Run() or await is used.
        private static async Task<List<Task<HttpResponseMessage>>> BuildRequests(CParameters parameters)
        {
            var httpResponse = SendRequest(parameters); // query CMS to retrieve certain information such as records total
            var recordResponse = await ProcessRecordResponse(httpResponse); // convert the response to usable data

            // TODO: Reset this back to RecordCountDB when done testing
            var recordTotal = 50; //recordResponse.RecordCountDB;
            var recordsPulled = recordResponse.Records().Count();
            var queryLimit = parameters.Limit ?? 2000; // default to 2000 if not specified

            SDisplay.Print("Querying CMS records.");
            SDisplay.StartProgressBar("CMS records queried", recordTotal);

            int remainingRecords = recordTotal - recordsPulled; // how many records are left to query?
            var requests = new List<Task<HttpResponseMessage>>();
            while (remainingRecords > 0)
            {
                var newLimit = Math.Min(queryLimit, remainingRecords);
                // Create a new request with the updated offset and limit
                var newParameters = new CParameters()
                {
                    Offset = parameters.Offset + recordsPulled,
                    Limit = newLimit,
                };
                requests.Add(SendRequest(newParameters));
                recordsPulled += newLimit;
                remainingRecords -= newLimit;
            }

            SDisplay.StopProgressBar();
            
            return requests;
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
