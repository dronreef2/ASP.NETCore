using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorCopiloto.Services;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests
{
    public class FakeLlamaIndexService : ILlamaIndexService
    {
        private readonly string _responseContent;

        public FakeLlamaIndexService(string responseContent)
        {
            _responseContent = responseContent;
        }

        public Task<string> GetChatResponseAsync(string message, string userId = "anonymous")
        {
            return Task.FromResult(_responseContent);
        }

        public Task<string> GetCompletionAsync(string prompt)
        {
            return Task.FromResult(_responseContent);
        }
    }

    public class IntelligentAnalysisServiceTests
    {
        [Fact]
        public async Task AnalyzeDeploymentLogsAsync_ParsesValidJsonResponse()
        {
            var json = @"{ ""status"": ""success"", ""issues"": [""error1""], ""recommendations"": [""rec1""], ""severity"": ""Média"", ""estimatedResolutionMinutes"": 45 }";
            var fakeLlamaService = new FakeLlamaIndexService(json);

            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;

            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            var result = await service.AnalyzeDeploymentLogsAsync("dep1", "some logs");

            Assert.Equal("success", result.Status);
            Assert.Contains("error1", result.Issues);
            Assert.Contains("rec1", result.Recommendations);
            Assert.Equal(TimeSpan.FromMinutes(45), result.EstimatedResolutionTime);
        }

        [Fact]
        public async Task AnalyzeDeploymentLogsAsync_HandlesInvalidJsonGracefully()
        {
            var invalidJson = "This is not JSON";
            var fakeLlamaService = new FakeLlamaIndexService(invalidJson);

            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;

            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            var result = await service.AnalyzeDeploymentLogsAsync("dep1", "logs with ERROR");

            Assert.Equal("Analisado", result.Status);
            Assert.NotNull(result.Issues);
        }

        [Fact]
        public async Task AnalyzePerformanceAsync_ReturnsPerformanceInsight()
        {
            // Arrange
            var aiResponse = "Some performance insights";
            var fakeLlamaService = new FakeLlamaIndexService(aiResponse);
            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;
            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            // Act
            var result = await service.AnalyzePerformanceAsync("some logs");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(aiResponse, result.AiAnalysis);
            Assert.NotEmpty(result.PerformanceOptimizations);
        }

        [Fact]
        public async Task GenerateExecutiveSummaryAsync_ReturnsSummary()
        {
            // Arrange
            var summary = "Executive summary";
            var fakeLlamaService = new FakeLlamaIndexService(summary);
            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;
            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            // Act
            var result = await service.GenerateExecutiveSummaryAsync("dep1", 7);

            // Assert
            Assert.Equal(summary, result);
        }

        [Fact]
        public async Task GenerateDeploymentSummaryAsync_ReturnsSummary()
        {
            // Arrange
            var summary = "Deployment summary";
            var fakeLlamaService = new FakeLlamaIndexService(summary);
            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;
            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            // Act
            var result = await service.GenerateDeploymentSummaryAsync("dep1", "some logs");

            // Assert
            Assert.Equal(summary, result);
        }

        [Fact]
        public async Task AnalyzeSecurityAsync_ReturnsSecurityAnalysisResult()
        {
            // Arrange
            var aiResponse = "Some security insights";
            var fakeLlamaService = new FakeLlamaIndexService(aiResponse);
            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;
            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            // Act
            var result = await service.AnalyzeSecurityAsync("some logs");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(aiResponse, result.AiInsights);
            Assert.NotEmpty(result.Recommendations);
        }

        [Fact]
        public async Task DetectAnomaliesAsync_ReturnsAnomalyDetectionResult()
        {
            // Arrange
            var aiResponse = "Some anomaly insights";
            var fakeLlamaService = new FakeLlamaIndexService(aiResponse);
            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;
            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            // Act
            var result = await service.DetectAnomaliesAsync("some logs");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Recommendations);
        }

        [Fact]
        public async Task GetDeploymentRecommendationsAsync_ReturnsDeploymentRecommendations()
        {
            // Arrange
            var aiResponse = "Some deployment recommendations";
            var fakeLlamaService = new FakeLlamaIndexService(aiResponse);
            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<IntelligentAnalysisService>.Instance;
            var service = new IntelligentAnalysisService(configuration, fakeLlamaService, logger);

            // Act
            var result = await service.GetDeploymentRecommendationsAsync("repoUrl", "some logs");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(aiResponse, result.AiInsights);
            Assert.NotEmpty(result.CiCdRecommendations);
        }
    }
}
