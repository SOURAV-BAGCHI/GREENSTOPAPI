using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;

namespace GreenStop.API.Helpers
{
    public class HttpRequestHelper
    {
        IHttpClientFactory _clientFactory;
        private readonly AppSettings _appSettings;
        public HttpRequestHelper(IHttpClientFactory clientFactory,IOptions<AppSettings> appSettings)
        {
            _clientFactory=clientFactory;
            _appSettings=appSettings.Value;
        }

        private async void RequestNotification(Object Obj)
        {
            var todoItemJson = new StringContent(
                Obj.ToString(),//JsonSerializer.Serialize(todoItem, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json");
            var _client = _clientFactory.CreateClient("notification");
            _client.BaseAddress=new Uri(_appSettings.NotificationServer);

            using var httpResponse =
            await _client.PostAsync("api/SendNotification/SendNotification", todoItemJson);
        }
    }
}