using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Mock chat completion service para demonstração quando não há API key configurada
    /// </summary>
    public class MockChatCompletionService : IChatCompletionService, IChatCompletionAdapter
    {
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            CancellationToken cancellationToken = default)
        {
            // Simular resposta baseada no prompt
            var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
            var response = GenerateMockResponse(lastMessage);
            
            await Task.Delay(100, cancellationToken); // Simular latência
            
            return new List<ChatMessageContent>
            {
                new ChatMessageContent(AuthorRole.Assistant, response)
            };
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = GenerateMockResponse(chatHistory.LastOrDefault()?.Content ?? "");
            
            // Simular streaming dividindo a resposta
            var words = response.Split(' ');
            foreach (var word in words)
            {
                await Task.Delay(50, cancellationToken);
                yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
            }
        }

        private string GenerateMockResponse(string prompt)
        {
            var promptLower = prompt.ToLower();
            
            if (promptLower.Contains("log") || promptLower.Contains("análise"))
            {
                return @"**ANÁLISE DE LOGS (DEMO)**

Baseado na análise dos logs fornecidos, identifiquei os seguintes pontos:

**Problemas Encontrados:**
- Erro de conexão com banco de dados PostgreSQL
- Falha na autenticação do SignalR Hub
- Assets JavaScript não encontrados (404)

**Recomendações:**
1. Verificar configuração do banco de dados
2. Implementar autenticação adequada para SignalR
3. Configurar assets estáticos corretamente

**Métricas de Performance:**
- Tempo médio de resposta: 45ms
- Taxa de erro: 15%
- Pico de uso: 14:30-15:00

*Nota: Esta é uma resposta simulada para demonstração.*";
            }
            
            if (promptLower.Contains("security") || promptLower.Contains("segurança"))
            {
                return @"**ANÁLISE DE SEGURANÇA (DEMO)**

Verificação de vulnerabilidades realizada:

**Vulnerabilidades Identificadas:**
- CORS configurado de forma muito permissiva
- Falta de rate limiting em endpoints públicos
- Logs contendo informações sensíveis

**Recomendações de Segurança:**
1. Implementar CORS mais restritivo
2. Adicionar rate limiting
3. Configurar sanitização de logs
4. Implementar autenticação JWT mais robusta

**Score de Segurança:** 7.5/10

*Nota: Esta é uma análise simulada para demonstração.*";
            }
            
            if (promptLower.Contains("performance") || promptLower.Contains("otimização"))
            {
                return @"**ANÁLISE DE PERFORMANCE (DEMO)**

Métricas de performance analisadas:

**Gargalos Identificados:**
- Consultas ao banco não otimizadas
- Cache Redis não configurado adequadamente
- Falta de compressão nas respostas HTTP

**Otimizações Sugeridas:**
1. Implementar índices no banco de dados
2. Configurar cache distribuído
3. Ativar compressão GZIP
4. Otimizar queries Entity Framework

**Melhorias Esperadas:**
- Redução de 40% no tempo de resposta
- Diminuição de 60% no uso de CPU
- Melhoria de 35% na experiência do usuário

*Nota: Esta é uma análise simulada para demonstração.*";
            }
            
            return @"**ANÁLISE INTELIGENTE (DEMO)**

Análise realizada com sucesso. Esta é uma resposta simulada do sistema de IA.

**Resumo:**
- Sistema funcionando adequadamente
- Algumas otimizações podem ser aplicadas
- Monitoramento contínuo recomendado

**Próximos Passos:**
1. Configurar chave de API para análises reais
2. Implementar monitoramento avançado
3. Otimizar configurações de produção

*Para funcionalidades completas de IA, configure uma chave de API válida.*";
        }

        // Implementação de IChatCompletionAdapter
        public async Task<string?> GetChatResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return null;

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            try
            {
                var response = await GetChatMessageContentsAsync(chatHistory, cancellationToken: cancellationToken);
                var first = response.FirstOrDefault();
                return first?.Content;
            }
            catch (Exception)
            {
                return "Erro no serviço mock de IA";
            }
        }
    }
}
