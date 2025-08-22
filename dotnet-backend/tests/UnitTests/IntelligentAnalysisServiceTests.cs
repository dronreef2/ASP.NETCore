using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TutorCopiloto.Services;
using TutorCopiloto.Services.Dto;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace UnitTests
{
    public class FakeChatCompletionService : IChatCompletionService
    {
        private readonly string _responseContent;

        public FakeChatCompletionService(string responseContent)
        {
            _responseContent = responseContent;
            Attributes = new Dictionary<string, object?>();
        }

        public IReadOnlyDictionary<string, object?> Attributes { get; }

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, System.Threading.CancellationToken cancellationToken = default)
        {
            var list = new List<ChatMessageContent>
            {
                new ChatMessageContent(AuthorRole.Assistant, _responseContent)
            };

            return Task.FromResult((IReadOnlyList<ChatMessageContent>)list);
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, [System.Runtime.CompilerServices.EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, _responseContent);
            await Task.CompletedTask;
        }
    }

    public class IntelligentAnalysisServiceTests
    {
        [Fact]
        public async Task AnalyzeDeploymentLogsAsync_ParsesValidJsonResponse()
        {
            var json = "{ \"status\": \"success\", \"issues\": [\"error1\"], \"recommendations\": [\"rec1\"], \"severity\": \"Média\", \"estimatedResolutionMinutes\": 45 }";
            var fake = new FakeChatCompletionService(json);

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
            var fake = new FakeChatCompletionService(invalid);

            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<IntelligentAnalysisService>.Instance;

            var service = new IntelligentAnalysisService(configuration, fake, logger);

            var result = await service.AnalyzeDeploymentLogsAsync("dep1", "logs with ERROR");

            Assert.Equal("Analisado", result.Status);
            Assert.NotNull(result.Issues);
        }
    }
}
