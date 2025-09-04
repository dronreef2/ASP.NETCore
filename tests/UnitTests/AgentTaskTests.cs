using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TutorCopiloto.Services.AgentTasks;
using TutorCopiloto.Services.AgentTasks.Agents;
using TutorCopiloto.Services;
using Moq;

namespace UnitTests.AgentTasks
{
    public class AgentTaskTests
    {
        private readonly Mock<IAIService> _mockAIService;
        private readonly Mock<ILogger<CodeReviewAgent>> _mockCodeReviewLogger;
        private readonly Mock<ILogger<SecurityAnalysisAgent>> _mockSecurityLogger;
        private readonly Mock<ILogger<DocumentationAgent>> _mockDocumentationLogger;
        private readonly Mock<ILogger<AgentTaskOrchestrator>> _mockOrchestratorLogger;

        public AgentTaskTests()
        {
            _mockAIService = new Mock<IAIService>();
            _mockCodeReviewLogger = new Mock<ILogger<CodeReviewAgent>>();
            _mockSecurityLogger = new Mock<ILogger<SecurityAnalysisAgent>>();
            _mockDocumentationLogger = new Mock<ILogger<DocumentationAgent>>();
            _mockOrchestratorLogger = new Mock<ILogger<AgentTaskOrchestrator>>();

            // Setup AI Service
            _mockAIService.Setup(x => x.IsAvailableAsync()).ReturnsAsync(true);
            _mockAIService.Setup(x => x.GetChatResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("AI response for testing");
        }

        [Fact]
        public void CodeReviewAgent_ShouldHaveCorrectProperties()
        {
            // Arrange
            var agent = new CodeReviewAgent(_mockAIService.Object, _mockCodeReviewLogger.Object);

            // Assert
            Assert.Equal("Code Review Agent", agent.AgentName);
            Assert.Equal(1, agent.Priority);
            Assert.False(string.IsNullOrEmpty(agent.Description));
            Assert.True(agent.EstimateExecutionTime() > TimeSpan.Zero);
        }

        [Fact]
        public void SecurityAnalysisAgent_ShouldHaveCorrectProperties()
        {
            // Arrange
            var agent = new SecurityAnalysisAgent(_mockAIService.Object, _mockSecurityLogger.Object);

            // Assert
            Assert.Equal("Security Analysis Agent", agent.AgentName);
            Assert.Equal(2, agent.Priority);
            Assert.False(string.IsNullOrEmpty(agent.Description));
            Assert.True(agent.EstimateExecutionTime() > TimeSpan.Zero);
        }

        [Fact]
        public void DocumentationAgent_ShouldHaveCorrectProperties()
        {
            // Arrange
            var agent = new DocumentationAgent(_mockAIService.Object, _mockDocumentationLogger.Object);

            // Assert
            Assert.Equal("Documentation Agent", agent.AgentName);
            Assert.Equal(5, agent.Priority);
            Assert.False(string.IsNullOrEmpty(agent.Description));
            Assert.True(agent.EstimateExecutionTime() > TimeSpan.Zero);
        }

        [Fact]
        public void AgentTaskOrchestrator_ShouldInitializeWithAgents()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_mockAIService.Object);
            serviceCollection.AddSingleton(_mockCodeReviewLogger.Object);
            serviceCollection.AddSingleton(_mockSecurityLogger.Object);
            serviceCollection.AddSingleton(_mockDocumentationLogger.Object);
            serviceCollection.AddSingleton(_mockOrchestratorLogger.Object);
            serviceCollection.AddTransient<CodeReviewAgent>();
            serviceCollection.AddTransient<SecurityAnalysisAgent>();
            serviceCollection.AddTransient<DocumentationAgent>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var orchestrator = new AgentTaskOrchestrator(serviceProvider, _mockOrchestratorLogger.Object);
            var availableAgents = orchestrator.GetAvailableAgents();

            // Assert
            Assert.Equal(3, availableAgents.Count);
            Assert.Contains(availableAgents, a => a.Name == "Code Review Agent");
            Assert.Contains(availableAgents, a => a.Name == "Security Analysis Agent");
            Assert.Contains(availableAgents, a => a.Name == "Documentation Agent");
        }

        [Fact]
        public void RepositoryAnalysisContext_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var context = new RepositoryAnalysisContext();

            // Assert
            Assert.Equal("main", context.Branch);
            Assert.False(context.IncludeAutoFix);
            Assert.NotNull(context.Options);
            Assert.NotNull(context.ProgrammingLanguages);
            Assert.NotNull(context.Metadata);
        }

        [Fact]
        public void AgentTaskResult_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var result = new AgentTaskResult();

            // Assert
            Assert.NotNull(result.Findings);
            Assert.NotNull(result.AppliedFixes);
            Assert.NotNull(result.Recommendations);
            Assert.NotNull(result.AdditionalData);
            Assert.Equal(DateTime.UtcNow.Date, result.ExecutedAt.Date);
        }

        [Fact]
        public void AgentFinding_ShouldHaveUniqueId()
        {
            // Arrange & Act
            var finding1 = new AgentFinding();
            var finding2 = new AgentFinding();

            // Assert
            Assert.NotEqual(finding1.Id, finding2.Id);
            Assert.False(string.IsNullOrEmpty(finding1.Id));
            Assert.False(string.IsNullOrEmpty(finding2.Id));
        }

        [Fact]
        public void AgentTaskExecutionOptions_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var options = new AgentTaskExecutionOptions();

            // Assert
            Assert.False(options.IncludeAutoFix);
            Assert.False(options.DryRun);
            Assert.Equal(100, options.MaxFindingsPerAgent);
            Assert.Equal(30, options.MaxExecutionTimeMinutes);
            Assert.NotNull(options.ExcludedAgents);
            Assert.NotNull(options.IncludedAgents);
            Assert.NotNull(options.AgentSpecificOptions);
        }

        [Fact]
        public async Task CodeReviewAgent_CanExecute_ShouldReturnFalseForEmptyPath()
        {
            // Arrange
            var agent = new CodeReviewAgent(_mockAIService.Object, _mockCodeReviewLogger.Object);
            var context = new RepositoryAnalysisContext();

            // Act
            var canExecute = await agent.CanExecuteAsync("", context);

            // Assert
            Assert.False(canExecute);
        }
    }
}