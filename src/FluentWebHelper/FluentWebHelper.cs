using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Supermortal.Common.Helpers.FluentWebHelper
{
    public class FluentWebHelper : FluentWebHelperBase
    {

        public FluentWebHelper() : base()
        {
            
        }

        public FluentWebHelper(string url) : this()
        {
            Context.Url = url;
        }

        internal FluentWebHelper(HttpClient client) : this()
        {
            Context.HttpClient = client;
        }

        internal FluentWebHelper(HttpClient client, string url) : this(url)
        {
            Context.HttpClient = client;
        }

        public FluentWebHelper ObjectParameter(object parameter)
        {
            CheckCreateParameters();
            var props = parameter.GetType().GetRuntimeProperties();
            foreach (var prop in props)
            {
                Context.Parameters[prop.Name] = prop.GetValue(parameter);
            }
            return this;
        }

        public FluentWebHelper Parameters(IDictionary<string, object> parameters)
        {
            Context.Parameters = parameters;
            return this;
        }

        public FluentWebHelper ClearParameters()
        {
            Context.Parameters = null;
            return this;
        }

        public FluentWebHelper AddParameter(string key, object value)
        {
            CheckCreateParameters();
            Context.Parameters.Add(key, value);
            return this;
        }

        public FluentWebHelper RemoveParameter(string key)
        {
            CheckCreateParameters();
            Context.Parameters.Remove(key);
            return this;
        }

        public FluentWebHelper UpdateParameter(string key, object value)
        {
            CheckCreateParameters();
            Context.Parameters[key] = value;
            return this;
        }

        public FluentWebHelper Url(string url)
        {
            Context.Url = url;
            return this;
        }

        public FluentWebHelper Headers(IDictionary<string, string> headers)
        {
            var cloneHeaders = headers.ToDictionary(entry => entry.Key, entry => entry.Value);
            CheckHeadersForAuthorizationHeader(cloneHeaders);
            Context.Headers = cloneHeaders;
            return this;
        }

        public FluentWebHelper ClearHeaders()
        {
            Context.Headers = null;
            return this;
        }

        public FluentWebHelper AddHeader(string key, string value)
        {
            CheckCreateHeaders();
            Context.Headers.Add(key, value);
            CheckHeadersForAuthorizationHeader(Context.Headers);
            return this;
        }

        public FluentWebHelper RemoveHeader(string key)
        {
            CheckCreateHeaders();
            Context.Headers.Remove(key);
            return this;
        }

        public FluentWebHelper UpdateHeader(string key, string value)
        {
            CheckCreateHeaders();
            Context.Headers[key] = value;
            CheckHeadersForAuthorizationHeader(Context.Headers);
            return this;
        }

        public FluentWebHelper AuthorizationHeader(string scheme, string parameter)
        {
            AuthorizationHeader(scheme);
            Context.AuthorizationParameter = parameter;
            return this;
        }

        public FluentWebHelper AuthorizationHeader(string scheme)
        {
            Context.AuthorizationScheme = scheme.ToUpper();
            return this;
        }

        public FluentWebHelper Accept(string acceptType)
        {
            Context.AcceptType = acceptType;
            return this;
        }

        public FluentWebHelper RequestType(string requestType)
        {
            Context.RequestType = requestType;
            return this;
        }

        public FluentWebHelper ReplaceAtSymbolsInResponse(bool replaceAtSymbols)
        {
            Context.ReplaceAtSymbols = replaceAtSymbols;
            return this;
        }

        public FluentWebHelper Files(IDictionary<string, byte[]> files)
        {
            Context.Files = files;
            return this;
        }

        public FluentWebHelper ClearFiles()
        {
            Context.Files = null;
            return this;
        }

        public FluentWebHelper AddFile(string key, byte[] value)
        {
            CheckCreateFiles();
            Context.Files.Add(key, value);
            return this;
        }

        public FluentWebHelper RemoveFile(string key)
        {
            CheckCreateFiles();
            Context.Files.Remove(key);
            return this;
        }

        public FluentWebHelper UpdateFile(string key, byte[] value)
        {
            CheckCreateFiles();
            Context.Files[key] = value;
            return this;
        }

        public async Task<WebHelperStringResponse> Get()
        {
            try
            {
                var fullUrl = CheckGetFullUrl();

                using (var client = Context.HttpClient)
                {
                    SetAcceptHeader(client);
                    SetAuthenticationHeader(client);
                    SetOtherHeaders(client);

                    var response = await client.GetAsync(fullUrl).ConfigureAwait(false);
                    return await CheckReturnResponse(response);
                }
            }
            finally
            {
                Context.HttpClient = null;
            }
        }

        public async Task<T> Get<T>()
        {
            var stringResponse = await Get();
            return JsonConvert.DeserializeObject<T>(stringResponse.Response);
        }

        public async Task<dynamic> GetDynamic()
        {
            var stringResponse = await Get();
            return JsonConvert.DeserializeObject(stringResponse.Response);
        }

        public async Task<WebHelperStringResponse> Post()
        {
            try
            {
                var fullUrl = CheckGetFullUrl();

                using (var client = Context.HttpClient)
                {
                    SetAcceptHeader(client);
                    SetAuthenticationHeader(client);
                    SetOtherHeaders(client);

                    var httpContent = GetHttpContent();                    

                    var response = await client.PostAsync(fullUrl, httpContent).ConfigureAwait(false);
                    return await CheckReturnResponse(response);
                }
            }
            finally
            {
                Context.HttpClient = null;
            }
        }

        public async Task<T> Post<T>()
        {
            var stringResponse = await Post();
            return JsonConvert.DeserializeObject<T>(stringResponse.Response);
        }

        public async Task<dynamic> PostDynamic()
        {
            var stringResponse = await Post();
            return JsonConvert.DeserializeObject(stringResponse.Response);
        }
    }
}
