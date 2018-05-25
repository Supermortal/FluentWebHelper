using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FluentWebHelper
{
    public class FluentWebHelperBase
    {
        internal readonly WebHelperContext Context;

        protected FluentWebHelperBase()
        {
            Context = new WebHelperContext();
        }

        public string ToFullUrl()
        {
            var queryStringSb = new StringBuilder();

            queryStringSb.Append(Context.Url);
            var queryString = ToQueryString(Context.Parameters);
            queryStringSb.Append(queryString);

            return queryStringSb.ToString();
        }

        protected void SetAcceptHeader(HttpClient client)
        {
            if (Context.AcceptHeader == null) return;

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(Context.AcceptHeader);
        }

        protected void SetAuthenticationHeader(HttpClient client)
        {
            if (Context.AuthenticationHeader == null) return;

            client.DefaultRequestHeaders.Authorization = Context.AuthenticationHeader;
        }

        protected void SetOtherHeaders(HttpClient client)
        {
            if (Context.Headers == null) return;

            foreach (var headerEntry in Context.Headers)
            {
                client.DefaultRequestHeaders.Add(headerEntry.Key, headerEntry.Value);
            }
        }

        protected void AddFileContents(HttpClient client)
        {
            
        }

        protected void ReplaceAtSymbols(WebHelperStringResponse webHelperStringResponse)
        {
            if (Context.ReplaceAtSymbols == true)
                webHelperStringResponse.Response = webHelperStringResponse.Response.Replace("@", string.Empty);
        }

        protected string CheckGetFullUrl()
        {
            if (string.IsNullOrEmpty(Context.Url) || string.IsNullOrWhiteSpace(Context.Url))
                throw new ArgumentException("A Url is requried");

            var fullUrl = ToFullUrl();
            return fullUrl;
        }

        protected void CheckCreateHeaders()
        {
            if (Context.Headers == null) Context.Headers = new Dictionary<string, string>();
        }

        protected void CheckCreateParameters()
        {
            if (Context.Parameters == null) Context.Parameters = new Dictionary<string, object>();
        }

        protected void CheckCreateFiles()
        {
            if (Context.Files == null) Context.Files = new Dictionary<string, byte[]>();
        }

        protected void CheckHeadersForAuthorizationHeader(IDictionary<string, string> headers)
        {
            if (headers.ContainsKey("Authorization"))
            {
                var headerValue = headers["Authorization"];
                var parts = headerValue.Split(' ');

                Context.AuthorizationScheme = parts[0].ToUpper();
                Context.AuthorizationParameter = parts[1];

                headers.Remove("Authorization");
            }
            else if (headers.ContainsKey("authorization"))
            {
                var headerValue = headers["authorization"];
                var parts = headerValue.Split(' ');

                Context.AuthorizationScheme = parts[0].ToUpper();
                Context.AuthorizationParameter = parts[1];

                headers.Remove("authorization");
            }
        }

        protected async Task<WebHelperStringResponse> CheckReturnResponse(HttpResponseMessage response)
        {
            if (response == null) return new WebHelperStringResponse() {Success = false};
            if (response.Content == null) return new WebHelperStringResponse() {Success = false, StatusCode = response.StatusCode};

            var webHelperStringResponse = new WebHelperStringResponse
            {
                StatusCode = response.StatusCode,
                Response = await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                Success = response.IsSuccessStatusCode
            };

            ReplaceAtSymbols(webHelperStringResponse);

            return webHelperStringResponse;
        }

        internal HttpContent GetHttpContent()
        {
            if (Context.HasFiles)
            {
                var formDataContent = new MultipartFormDataContent();

                if (Context.Parameters != null)
                {
                    foreach (var parameter in Context.Parameters)
                    {
                        if (parameter.Value == null) continue;

                        var stringContent = new StringContent(parameter.Value.ToString());
                        stringContent.Headers.ContentType = null;
                        formDataContent.Add(stringContent, parameter.Key);
                    }
                }

                foreach (var file in Context.Files)
                {
                    if (file.Value == null) continue;

                    var byteArrayContent = new ByteArrayContent(file.Value);
                    byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    formDataContent.Add(byteArrayContent, file.Key);
                }

                return formDataContent;
            }

            switch (Context.RequestType)
            {
                case "application/json":
                    return new StringContent(JsonConvert.SerializeObject(Context.Parameters), Encoding.UTF8, Context.RequestType);
            }

            throw new Exception(
                            "Problem initializing HttpContent. Make sure RequestType is a valid value (Default is 'application/json')");
        }

        internal static string ToQueryString(IDictionary<string, object> parameters)
        {
            if (parameters == null || !parameters.Any()) return string.Empty;

            var queryStringArray = parameters.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value.ToString())}").ToArray();

            var queryStringSb = new StringBuilder();
            queryStringSb.Append('?');
            queryStringSb.Append(string.Join("&", queryStringArray));

            return queryStringSb.ToString();
        }
    }

    internal class WebHelperContext
    {
        public IDictionary<string, string> Headers { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public IDictionary<string, byte[]> Files { get; set; }
        public bool HasFiles => (Files != null && Files.Any());
        public string Url { get; set; }
        public string AuthorizationScheme { get; set; }
        public string AuthorizationParameter { get; set; }
        public string AcceptType { get; set; }
        public string RequestType { get; set; } = "application/json";
        public bool? ReplaceAtSymbols { get; set; }

        public MediaTypeWithQualityHeaderValue AcceptHeader
            =>
                (string.IsNullOrEmpty(AcceptType))
                    ? new MediaTypeWithQualityHeaderValue("application/json")
                    : new MediaTypeWithQualityHeaderValue(AcceptType);

        private HttpClient _httpClient;
        internal HttpClient HttpClient
        {
            get
            {
                if (_httpClient != null) return _httpClient;
                return (_httpClient = new HttpClient());
            }
            set { _httpClient = value; }
        }

        private AuthenticationHeaderValue _authenticationHeader;
        public AuthenticationHeaderValue AuthenticationHeader
        {
            get
            {
                if (_authenticationHeader != null) return _authenticationHeader;
                if (AuthorizationScheme == null) return null;

                if (AuthorizationParameter != null)
                {
                    return
                    (_authenticationHeader =
                        new AuthenticationHeaderValue(AuthorizationScheme, AuthorizationParameter));
                }

                return (_authenticationHeader = new AuthenticationHeaderValue(AuthorizationScheme));
            }
        }
    }

    public class WebHelperStringResponse
    {
        public string Response { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
        public bool Success { get; internal set; }
    }
}
