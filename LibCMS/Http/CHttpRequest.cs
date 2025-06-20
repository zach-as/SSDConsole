using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LibCMS.Params;
using LibCMS.Path;
using LibCMS.Json;

namespace LibCMS.Http
{

    internal class CHttpRequest : HttpRequestMessage
    {
        private const string CONTENT_TYPE = "application/json";

        public CHttpRequest(CParameters parameters)
        {
         
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DictionaryKeyPolicy = new CUpperCamelNamingPolicy(),
                PropertyNamingPolicy = new CUpperCamelNamingPolicy(),
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            JsonContent content = JsonContent.Create(
                parameters,
                parameters.GetType(),
                MediaTypeWithQualityHeaderValue.Parse(CONTENT_TYPE),
                options
            );

            Content = content;
            Content.Headers.ContentType = new MediaTypeHeaderValue(CONTENT_TYPE);

            Method = HttpMethod.Post;
            RequestUri = SPath.BuildUri();

        }
    }
}
