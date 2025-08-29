using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using TutorCopiloto.Models;

namespace TutorCopiloto.Hubs;

/// <summary>
/// Hub SignalR para comunicação em tempo real do chat educacional
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IStringLocalizer<ChatHub> _localizer;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IStringLocalizer<ChatHub> localizer,
        ILogger<ChatHub> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Conecta usuário ao hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name;
        _logger.LogInformation("Usuário {UserId} conectado ao chat hub", userId);

        if (!string.IsNullOrEmpty(userId))
        {
            // Adicionar usuário ao grupo pessoal para notificações diretas
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Notificar outros usuários sobre a conexão (se apropriado)
            await Clients.Others.SendAsync("UserConnected", new
            {
                UserId = userId,
                ConnectionTime = DateTime.Now,
                Message = _localizer["UsuarioConectado", userId]
            });
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Desconecta usuário do hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name;
        _logger.LogInformation("Usuário {UserId} desconectado do chat hub", userId);

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            await Clients.Others.SendAsync("UserDisconnected", new
            {
                UserId = userId,
                DisconnectionTime = DateTime.Now,
                Message = _localizer["UsuarioDesconectado", userId]
            });
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Erro durante desconexão do usuário {UserId}", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Envia mensagem no chat
    /// </summary>
    /// <param name="message">Mensagem a ser enviada</param>
    public async Task SendMessage(ChatMessage message)
    {
        try
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", _localizer["UsuarioNaoAutenticado"]);
                return;
            }

            // Validar mensagem
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                await Clients.Caller.SendAsync("Error", _localizer["MensagemVazia"]);
                return;
            }

            if (message.Content.Length > 4000)
            {
                await Clients.Caller.SendAsync("Error", _localizer["MensagemMuitoLonga"]);
                return;
            }

            _logger.LogInformation("Usuário {UserId} enviou mensagem no chat", userId);

            // Criar objeto de mensagem completo
            var mensagemCompleta = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Content = message.Content,
                SenderId = userId,
                SenderName = Context.User?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value ?? userId,
                Timestamp = DateTime.Now,
                Type = message.Type,
                SessionId = message.SessionId,
                Metadata = message.Metadata ?? new Dictionary<string, object>()
            };

            // Determinar destinatários baseado no tipo de mensagem
            switch (message.Type)
            {
                case ChatMessageType.DirectMessage:
                    if (!string.IsNullOrEmpty(message.TargetUserId))
                    {
                        await Clients.Group($"user_{message.TargetUserId}")
                            .SendAsync("ReceiveMessage", mensagemCompleta);
                    }
                    break;

                case ChatMessageType.GroupMessage:
                    if (!string.IsNullOrEmpty(message.GroupId))
                    {
                        await Clients.Group($"group_{message.GroupId}")
                            .SendAsync("ReceiveMessage", mensagemCompleta);
                    }
                    break;

                case ChatMessageType.TutorInteraction:
                    // Mensagem para o sistema de IA - processar e retornar resposta
                    await ProcessTutorMessage(mensagemCompleta);
                    break;

                default:
                    // Mensagem geral - enviar para todos conectados
                    await Clients.All.SendAsync("ReceiveMessage", mensagemCompleta);
                    break;
            }

            // Confirmar envio para o remetente
            await Clients.Caller.SendAsync("MessageSent", new
            {
                MessageId = mensagemCompleta.Id,
                Status = "Delivered",
                Timestamp = mensagemCompleta.Timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem do usuário {UserId}", Context.User?.Identity?.Name);
            await Clients.Caller.SendAsync("Error", _localizer["ErroProcessarMensagem"]);
        }
    }

    /// <summary>
    /// Ingressa em um grupo (turma, projeto, etc.)
    /// </summary>
    /// <param name="groupName">Nome do grupo</param>
    public async Task JoinGroup(string groupName)
    {
        try
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", _localizer["UsuarioNaoAutenticado"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(groupName))
            {
                await Clients.Caller.SendAsync("Error", _localizer["NomeGrupoObrigatorio"]);
                return;
            }

            _logger.LogInformation("Usuário {UserId} ingressando no grupo {GroupName}", userId, groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupName}");
            
            await Clients.Group($"group_{groupName}").SendAsync("UserJoinedGroup", new
            {
                UserId = userId,
                GroupName = groupName,
                Timestamp = DateTime.Now,
                Message = _localizer["UsuarioIngressouGrupo", userId, groupName]
            });

            await Clients.Caller.SendAsync("JoinedGroup", new
            {
                GroupName = groupName,
                Status = "Success",
                Message = _localizer["IngressoGrupoSucesso", groupName]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ingressar no grupo {GroupName}", groupName);
            await Clients.Caller.SendAsync("Error", _localizer["ErroIngressarGrupo"]);
        }
    }

    /// <summary>
    /// Sai de um grupo
    /// </summary>
    /// <param name="groupName">Nome do grupo</param>
    public async Task LeaveGroup(string groupName)
    {
        try
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            _logger.LogInformation("Usuário {UserId} saindo do grupo {GroupName}", userId, groupName);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupName}");
            
            await Clients.Group($"group_{groupName}").SendAsync("UserLeftGroup", new
            {
                UserId = userId,
                GroupName = groupName,
                Timestamp = DateTime.Now,
                Message = _localizer["UsuarioSaiuGrupo", userId, groupName]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sair do grupo {GroupName}", groupName);
        }
    }

    /// <summary>
    /// Inicia sessão de programação em pair
    /// </summary>
    /// <param name="partnerId">ID do parceiro</param>
    public async Task StartPairProgramming(string partnerId)
    {
        try
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", _localizer["UsuarioNaoAutenticado"]);
                return;
            }

            _logger.LogInformation("Usuário {UserId} iniciando pair programming com {PartnerId}", userId, partnerId);

            var sessionId = $"pair_{userId}_{partnerId}_{DateTime.Now.Ticks}";
            
            // Adicionar ambos usuários ao grupo da sessão
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            
            // Notificar o parceiro sobre o convite
            await Clients.Group($"user_{partnerId}").SendAsync("PairProgrammingInvite", new
            {
                SessionId = sessionId,
                InviterId = userId,
                InviterName = Context.User?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value ?? userId,
                Timestamp = DateTime.Now,
                Message = _localizer["ConvitePairProgramming", userId]
            });

            await Clients.Caller.SendAsync("PairProgrammingStarted", new
            {
                SessionId = sessionId,
                PartnerId = partnerId,
                Status = "Waiting",
                Message = _localizer["AguardandoAceitePair"]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar pair programming");
            await Clients.Caller.SendAsync("Error", _localizer["ErroIniciarPairProgramming"]);
        }
    }

    /// <summary>
    /// Aceita convite para pair programming
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    public async Task AcceptPairProgramming(string sessionId)
    {
        try
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            _logger.LogInformation("Usuário {UserId} aceitou pair programming na sessão {SessionId}", userId, sessionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            
            await Clients.Group(sessionId).SendAsync("PairProgrammingAccepted", new
            {
                SessionId = sessionId,
                AccepterId = userId,
                Timestamp = DateTime.Now,
                Message = _localizer["PairProgrammingAceito"]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar pair programming");
        }
    }

    /// <summary>
    /// Compartilha código durante pair programming
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <param name="code">Código a ser compartilhado</param>
    /// <param name="filename">Nome do arquivo</param>
    public async Task ShareCode(string sessionId, string code, string filename)
    {
        try
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            _logger.LogInformation("Usuário {UserId} compartilhando código na sessão {SessionId}", userId, sessionId);

            await Clients.GroupExcept(sessionId, Context.ConnectionId).SendAsync("CodeShared", new
            {
                SessionId = sessionId,
                Code = code,
                Filename = filename,
                SharedBy = userId,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao compartilhar código");
        }
    }

    /// <summary>
    /// Processa mensagem direcionada ao tutor IA
    /// </summary>
    private async Task ProcessTutorMessage(ChatMessage message)
    {
        try
        {
            // Placeholder para integração com o serviço de IA
            // Em produção, integrar com o ClaudeService
            
            await Clients.Caller.SendAsync("TutorTyping", new
            {
                SessionId = message.SessionId,
                Timestamp = DateTime.Now
            });

            // Simular tempo de processamento
            await Task.Delay(1000);

            var tutorResponse = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Content = _localizer["RespostaTutorPlaceholder", message.Content],
                SenderId = "tutor-ai",
                SenderName = _localizer["TutorIA"],
                Timestamp = DateTime.Now,
                Type = ChatMessageType.TutorResponse,
                SessionId = message.SessionId,
                Metadata = new Dictionary<string, object>
                {
                    ["original_question"] = message.Content,
                    ["processing_time_ms"] = 1000,
                    ["model_used"] = "claude-3.5-sonnet"
                }
            };

            await Clients.Caller.SendAsync("ReceiveMessage", tutorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem do tutor");
            await Clients.Caller.SendAsync("Error", _localizer["ErroProcessarTutor"]);
        }
    }
}
