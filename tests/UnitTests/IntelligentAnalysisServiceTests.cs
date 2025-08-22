#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorCopiloto.Services;
using TutorCopiloto.Services.Dto;
// Removido uso direto de tipos do Semantic Kernel para facilitar testes via adapter
using Xunit;

namespace UnitTests
{
    public class FakeChatCompletionAdapter : IChatCompletionAdapter
    {
        private readonly string _responseContent;

        public FakeChatCompletionAdapter(string responseContent)
        {
            _responseContent = responseContent;
        }

        public Task<string?> GetChatResponseAsync(string prompt, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(_responseContent);
        }
    }

    public class IntelligentAnalysisServiceTests
    {
        [Fact]
        public async Task AnalyzeDeploymentLogsAsync_ParsesValidJsonResponse()
        {
            var json = "{ \"status\": \"success\", \"issues\": [\"error1\"], \"recommendations\": [\"rec1\"], \"severity\": \"Média\", \"estimatedResolutionMinutes\": 45 }";
            var fake = new FakeChatCompletionAdapter(json);

            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<IntelligentAnalysisService>.Instance;

            var service = new IntelligentAnalysisService(configuration, fake, logger);

            var result = await service.AnalyzeDeploymentLogsAsync("dep1", "some logs");

            Assert.Equal("success", result.Status);
            Assert.Contains("error1", result.Issues);
            Assert.Contains("rec1", result.Recommendations);
            Assert.Equal(TimeSpan.FromMinutes(45), result.EstimatedResolutionTime);
        }

        [Fact]
        public async Task AnalyzeDeploymentLogsAsync_HandlesInvalidJsonGracefully()
        {
            var invalid = "This is not JSON";
            var fake = new FakeChatCompletionAdapter(invalid);

            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<IntelligentAnalysisService>.Instance;

            var service = new IntelligentAnalysisService(configuration, fake, logger);

            var result = await service.AnalyzeDeploymentLogsAsync("dep1", "logs with ERROR");

            Assert.Equal("Analisado", result.Status);
            Assert.NotNull(result.Issues);
        }
    }
}
