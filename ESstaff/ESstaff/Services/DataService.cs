using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using ESstaff;

namespace XamarinMovieListApp.Data
{
    public class DataService
    {
        private HttpClient Client { get; set; }
        private Dictionary<string, string> ModelEndpoints { get; set; }
        private string BaseUrlAddress { get; set; }

        private Action LoginHandler { get; set; }
        public bool IsStartLoginOnUnauthorizedEnabled { get; private set; }
        public bool ExceptionOnHttpClientError { get; private set; }

        public HttpResponseMessage LastResponse { get; private set; }

        public DataService(string baseAddress)
        {
            // add the insecure SSL certificate handler to the HttpClient only if this is a debug build
#if DEBUG
            HttpClientHandler insecureHandler = DependencyService.Get<IHttpClientHandlerService>().GetInsecureHandler();
            this.Client = new HttpClient(insecureHandler);
#else
            this.client = new HttpClient();
#endif
            // set base address of the RESTful web service 
            this.BaseUrlAddress = baseAddress;

            // dictionary of endpoints, one for each model class in service
            this.ModelEndpoints = new Dictionary<string, string>();

            this.EnableThrowExceptionOnHttpClientError();
        }

        public DataService AddEntityModelEndpoint<TEntity>(string endpoint)
        {
            // this uses reflection to get the name of the class type and use that as the dictionary entry's key
            // the endpoint is the value for this key/value pair
            this.ModelEndpoints.Add(typeof(TEntity).FullName, endpoint);
            return this;
        }

        public DataService AddBearerToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                this.RemoveBearerToken();
            }
            else
            {
                this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return this;
        }

        public DataService RemoveBearerToken()
        {
            this.Client.DefaultRequestHeaders.Authorization = null;
            return this;
        }

        public DataService SetLoginFlowAction(Action handler)
        {
            this.LoginHandler = handler;
            return this;
        }

        public DataService RemoveLoginFlowAction()
        {
            this.LoginHandler = null;
            return this;
        }

        public DataService EnableLoginOnUnauthorized()
        {
            // set the flag to enable but only if the LoginHandler delegate action has been set
            this.IsStartLoginOnUnauthorizedEnabled = this.LoginHandler != null;
            return this;
        }

        public DataService DisableLoginOnUnauthorized()
        {
            this.IsStartLoginOnUnauthorizedEnabled = false;
            return this;
        }

        public DataService EnableThrowExceptionOnHttpClientError()
        {
            this.ExceptionOnHttpClientError = true;
            return this;
        }

        public DataService DisableThrowExceptionOnHttpClientError()
        {
            this.ExceptionOnHttpClientError = false;
            return this;
        }

        private string GetEntityEndpoint<TEntity>(string nonDefaultEndpoint = null)
        {
            // private helper method to get the endpoint, factored out into separate method for code reuse purposes
            StringBuilder endpoint = new StringBuilder(this.ModelEndpoints[typeof(TEntity).FullName]);
            if (!string.IsNullOrEmpty(nonDefaultEndpoint))
            {
                endpoint.Append($"/{nonDefaultEndpoint}");
            }
            return endpoint.ToString();
        }

        private void HandleUnsuccesfulResponse()
        {
            if (LastResponse.StatusCode == HttpStatusCode.Unauthorized
                    && this.IsStartLoginOnUnauthorizedEnabled)
            {
                this.RemoveBearerToken();
                App.LoggedInUserAccount = null;
                this.LoginHandler();
            }
            else
            {
                if (LastResponse.StatusCode == HttpStatusCode.Unauthorized
                         && !this.IsStartLoginOnUnauthorizedEnabled)
                {
                    this.RemoveBearerToken();
                }

                if (this.ExceptionOnHttpClientError)
                {
                    if (LastResponse.Content is object)
                    {
                        var problemDetailsStr = LastResponse.Content.ReadAsStringAsync().Result;
                        ValidationProblemDetails problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(problemDetailsStr);
                        if (problemDetails != null)
                        {
                            var validationErrors = problemDetails.Errors.SelectMany(x => x.Value.Select(y => y));
                            var flattenedErrors = string.Join(Environment.NewLine, validationErrors);
                            throw new Exception($"{problemDetails.Status} {LastResponse.ReasonPhrase} {Environment.NewLine}{problemDetails.Title} {Environment.NewLine}{problemDetails.Detail} {Environment.NewLine}{flattenedErrors}");
                        }
                        else
                        {
                            throw new Exception($"{LastResponse.StatusCode} {LastResponse.ReasonPhrase}");
                        }
                    }
                    else
                    {
                        throw new Exception($"{LastResponse.StatusCode} {LastResponse.ReasonPhrase}");
                    }
                }
            }
        }

        public async Task<List<TEntity>> GetAllAsync<TEntity>(Expression<Func<TEntity, bool>> searchLambda = null, string nonDefaultEndpoint = null)
            where TEntity : new()
        {
            // form the URI for the webservice GET request 
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}?searchExpression={searchLambda}";
            var uri = new Uri(string.Format(url));

            // make the GET request to the URI
            LastResponse =
                await Client.GetAsync(uri);

            if (LastResponse.IsSuccessStatusCode)
            {
                // read the content returned by the GET request
                var content = await
                   LastResponse.Content
                           .ReadAsStringAsync();

                // deserialise the read content back to C# objects
                var deserialisedContent =
                    JsonConvert.DeserializeObject<IEnumerable<TEntity>>(content);

                // return deserialised objects back to caller as List<TEntity> collection
                return deserialisedContent.ToList<TEntity>();
            }
            else
            {
                this.HandleUnsuccesfulResponse();
                return null;
            }
        }

        public async Task<TEntity> GetAsync<TEntity, T>(T id, string nonDefaultEndpoint = null)
             where TEntity : new()
        {
            // form the URI for the webservice GET request 
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var uri = new Uri($"{this.BaseUrlAddress}/{endpoint}/{id}");

            // make the GET request to the URI
            LastResponse =
                await Client.GetAsync(uri);

            if (LastResponse.IsSuccessStatusCode)
            {
                // read the content returned by the GET request
                var content = await
                   LastResponse.Content
                           .ReadAsStringAsync();

                // deserialise the read content back to C# object
                var deserialisedContent =
                    JsonConvert.DeserializeObject<TEntity>(content);

                // return deserialised object back to caller as TEntity type
                return deserialisedContent;
            }
            else
            {
                this.HandleUnsuccesfulResponse();
                return default(TEntity);
            }
        }

        public async Task<bool> UpdateAsync<TEntity, T>(TEntity entity, T id, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice PUT request 
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}/{id}";
            var uri = new Uri(url);

            // serialise the TEntity object to a JSON string
            string jsonEntity = JsonConvert.SerializeObject(entity);
            StringContent content = new StringContent(jsonEntity, Encoding.UTF8, "application/json");

            // make the PUT request to the URI
            LastResponse = await Client.PutAsync(uri, content);

            if (!LastResponse.IsSuccessStatusCode)
            {
                this.HandleUnsuccesfulResponse();
            }

            // return success or failure boolean
            return LastResponse.IsSuccessStatusCode;
        }

        public bool Update<TEntity, T>(TEntity entity, T id, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice PUT request 
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}/{id}";
            var uri = new Uri(url);

            // serialise the TEntity object to a JSON string
            string jsonEntity = JsonConvert.SerializeObject(entity);
            StringContent content = new StringContent(jsonEntity, Encoding.UTF8, "application/json");

            // make the PUT request to the URI
            LastResponse = Client.PutAsync(uri, content).Result;

            if (!LastResponse.IsSuccessStatusCode)
            {
                this.HandleUnsuccesfulResponse();
            }

            // return success or failure boolean
            return LastResponse.IsSuccessStatusCode;
        }

        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity, string nonDefaultEndpoint = null)
        {
            return await this.InsertAsync<TEntity, TEntity>(entity, nonDefaultEndpoint);
        }

        public async Task<TReturnEntity> InsertAsync<TEntity, TReturnEntity>(TEntity entity, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice POST request 
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}";
            var uri = new Uri(url);

            // serialise the TEntity object to a JSON string
            string jsonEntity = JsonConvert.SerializeObject(entity);
            StringContent content = new StringContent(jsonEntity, Encoding.UTF8, "application/json");

            // make the POST request to the URI
            LastResponse = await Client.PostAsync(uri, content);

            if (LastResponse.IsSuccessStatusCode)
            {
                // read the response content returned by the POST request (the created object)
                var responseContent = await
                   LastResponse.Content
                           .ReadAsStringAsync();

                // deserialise the read content back to C# object
                var deserialisedContent =
                    JsonConvert.DeserializeObject<TReturnEntity>(responseContent);

                // return deserialised object back to caller as TReturnEntity type
                return deserialisedContent;
            }
            else
            {
                this.HandleUnsuccesfulResponse();
                return default(TReturnEntity);
            }
        }

        public async Task<bool> DeleteAsync<TEntity, T>(TEntity entity, T id, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice DELETE request 
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}/{id}";
            var uri = new Uri(url);

            // make the DELETE request to the URI
            LastResponse = await Client.DeleteAsync(uri);

            if (!LastResponse.IsSuccessStatusCode)
            {
                this.HandleUnsuccesfulResponse();
            }

            // return success or failure boolean
            return LastResponse.IsSuccessStatusCode;
        }
    }
}
