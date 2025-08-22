#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TutorCopiloto.Services;
using Xunit;

namespace UnitTests
{
    public class FakeHandler : DelegatingHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        private readonly HttpResponseMessage _response;

        public FakeHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_response);
        }
    }

    public class ClaudeHttpClientIntegrationTests
    {
        [Fact]
        public async Task ClaudeTypedClient_IsConfiguredWithBaseAddressAndHeaders()
        {
            // Arrange: response esperado
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"content\": [ { \"text\": \"ok\" } ] }")
            };

            var handler = new FakeHandler(fakeResponse);

            var services = new ServiceCollection();
            services.AddLogging();

            // Registrar typed client usando handler fake
            services.AddHttpClient<IClaudeChatCompletionService, ClaudeChatCompletionService>(client =>
            {
                client.BaseAddress = new System.Uri("https://api.anthropic.com/");
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                client.DefaultRequestHeaders.Add("x-api-key", "test-key");
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);

            // Registrar o adapter para que quem resolver IClaudeChatCompletionService
            // também possa obter IChatCompletionAdapter via cast (espelho do Program.cs)
            services.AddScoped<IChatCompletionAdapter>(provider =>
                (IChatCompletionAdapter)provider.GetRequiredService<IClaudeChatCompletionService>());

            var provider = services.BuildServiceProvider();

            var svc = provider.GetRequiredService<IClaudeChatCompletionService>();

            // Act
            var adapter = svc as IChatCompletionAdapter;
            Assert.NotNull(adapter);

            var text = await adapter.GetChatResponseAsync("hello");

            // Assert
            Assert.Equal("ok", text?.Trim());
            Assert.NotNull(handler.LastRequest);
            Assert.Equal(new System.Uri("https://api.anthropic.com/v1/messages"), handler.LastRequest!.RequestUri);
            Assert.True(handler.LastRequest.Headers.Contains("x-api-key") || handler.LastRequest.Headers.Contains("anthropic-version"));
        }
    }
}
