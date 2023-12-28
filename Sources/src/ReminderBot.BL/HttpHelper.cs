using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReminderBot.BL.Interfaces;
using ReminderBot.Shared.Helpers;

namespace ReminderBot.BL
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpHelper> _logger;

        public HttpHelper(IHttpClientFactory httpClientFactory, ILogger<HttpHelper> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<T> GetAsync<T>(string query, string? externalAccessToken = null, CancellationToken token = default)
        {
            var request = BuildRequest(HttpMethod.Get, query, externalAccessToken);

            return await ExecuteRequestAsync<T>(request, token);
        }

        public async Task<T> PostAsync<T>(string query, object @object, string? externalAccessToken = null, CancellationToken token = default)
        {
            var request = BuildRequest(HttpMethod.Post, query, externalAccessToken, @object);

            return await ExecuteRequestAsync<T>(request, token);
        }

        private async Task<T> ExecuteRequestAsync<T>(HttpRequestMessage request, CancellationToken token = default)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(60);

            HttpResponseMessage response;

            try
            {
                response = await client.SendAsync(request, token);
            }
            catch (Exception ex)
            {
                _logger.LogError("ExecuteRequestAsync exception: {0}", ex);
                throw;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                return JsonConvertHelper.GetObject<T>(json);
            }
            else
            {
                throw new Exception($"External Api error:{response.StatusCode}");
            }
        }

        private static HttpRequestMessage BuildRequest(HttpMethod httpMethod, string url, string? externalAccessToken = null, object? @object = null)
        {
            var request = new HttpRequestMessage(httpMethod, url);

            if (@object != null)
            {
                var postObj = JsonConvertHelper.GetJson(@object);
                request.Content = new StringContent(postObj, Encoding.UTF8, Constants.ContentType.Json);
            }

            AddHeaders(request, externalAccessToken);

            return request;
        }

        private static void AddHeaders(HttpRequestMessage request, string? externalAccessToken = null)
        {
            request.Headers.Add(Constants.CacheControl.HeaderName, Constants.CacheControl.NoCache);

            if (externalAccessToken != null)
            {
                var bearerToken = externalAccessToken.Contains("Bearer") ? externalAccessToken : $"Bearer {externalAccessToken}";
                request.Headers.Add(Constants.Authorization.HeaderName, bearerToken);
            }
        }

        internal static class Constants
        {
            public static class ContentType
            {
                internal const string HeaderName = "content-type";
                internal const string Json = "application/json";
            }

            public static class Authorization
            {
                internal const string HeaderName = "authorization";
            }

            public static class CacheControl
            {
                internal const string HeaderName = "cache-control";
                internal const string NoCache = "no-cache";
            }
        }
    }
}
