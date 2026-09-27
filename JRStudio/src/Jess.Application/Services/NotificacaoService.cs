using System.Timers;
using Jess.Application.Interfaces;
using Jess.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Jess.Application.Services;

/// <summary>
/// Verifica periodicamente tarefas com prazo para hoje e dispara uma notificação toast nativa
/// (Seção 2.2). Singleton com <see cref="IServiceScopeFactory"/> — mesmo motivo/padrão do
/// <see cref="CronometroStateService"/>: precisa sobreviver à navegação entre páginas e continuar
/// rodando mesmo sem nenhuma tela de Tarefas aberta.
/// </summary>
public class NotificacaoService : IDisposable
{
    private static readonly TimeSpan IntervaloVerificacao = TimeSpan.FromMinutes(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IToastService _toastService;
    private readonly ILogger<NotificacaoService> _logger;
    private readonly Timer _timer;
    private readonly ElapsedEventHandler _timerHandler;

    // Em memória, não persistido: reinicia a cada execução do app. Evita reenviar a mesma
    // notificação a cada verificação periódica dentro do mesmo dia — não precisa sobreviver a um
    // reinício do app (a usuária vendo a notificação de novo ao reabrir o app não é um problema real).
    private readonly HashSet<(int TarefaId, DateOnly Data)> _jaNotificadas = [];

    public NotificacaoService(IServiceScopeFactory scopeFactory, IToastService toastService, ILogger<NotificacaoService> logger)
    {
        _scopeFactory = scopeFactory;
        _toastService = toastService;
        _logger = logger;
        _timer = new Timer(IntervaloVerificacao.TotalMilliseconds) { AutoReset = true };
        _timerHandler = async (_, _) => await VerificarAsync();
        _timer.Elapsed += _timerHandler;
    }

    /// <summary>Inicia a verificação periódica (chamado uma vez, na inicialização do app).</summary>
    public void Iniciar()
    {
        _timer.Start();
        _ = VerificarAsync();
    }

    /// <summary>Roda uma verificação imediatamente (exposto público pra ser testável sem depender do timer).</summary>
    public async Task VerificarAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tarefaService = scope.ServiceProvider.GetRequiredService<ITarefaService>();
            var tarefas = await tarefaService.ListarTodasAsync();

            var hoje = DateOnly.FromDateTime(DateTime.Today);

            foreach (var tarefa in tarefas)
            {
                if (tarefa.Coluna == ColunaTarefa.Concluido || tarefa.Prazo is null)
                {
                    continue;
                }

                if (DateOnly.FromDateTime(tarefa.Prazo.Value) != hoje)
                {
                    continue;
                }

                if (!_jaNotificadas.Add((tarefa.Id, hoje)))
                {
                    continue;
                }

                _toastService.Mostrar(tarefa.Titulo, "Termina hoje");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao verificar tarefas com prazo para notificação");
        }
    }

    public void Dispose()
    {
        _timer.Elapsed -= _timerHandler;
        _timer.Dispose();
    }
}
