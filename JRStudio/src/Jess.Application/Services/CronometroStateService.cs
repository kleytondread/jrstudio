using System.Timers;
using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Jess.Application.Services;

/// <summary>
/// Estado do cronômetro global do app, vivo enquanto o processo roda — não enquanto uma tela está
/// aberta. Registrado como Singleton (ver Guia_Implementacao_IA.md, "Riscos técnicos conhecidos"):
/// como o BlazorWebView cria um escopo de DI por sessão de componentes, um serviço Scoped perderia
/// o estado ao navegar entre páginas. Este serviço usa <see cref="IServiceScopeFactory"/> para
/// resolver os serviços Scoped (<see cref="IRastreamentoHorasService"/>, <see cref="IProjetoService"/>)
/// sob demanda, a cada operação.
/// </summary>
public class CronometroStateService : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CronometroStateService> _logger;
    private readonly Timer _timer;
    private readonly ElapsedEventHandler _timerHandler;
    private bool _inicializado;

    public event Action? OnChange;

    public RegistroHorasDto? Ativo { get; private set; }

    public string? ProjetoNomeAtivo { get; private set; }

    public CronometroStateService(IServiceScopeFactory scopeFactory, ILogger<CronometroStateService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timer = new Timer(1000) { AutoReset = true };
        _timerHandler = (_, _) => OnChange?.Invoke();
        _timer.Elapsed += _timerHandler;
    }

    public TimeSpan TempoDecorrido
    {
        get
        {
            if (Ativo is null)
            {
                return TimeSpan.Zero;
            }

            var referencia = Ativo.Status == StatusRegistroHoras.Pausado && Ativo.PausaAbertaEm.HasValue
                ? Ativo.PausaAbertaEm.Value
                : DateTime.Now;

            var decorrido = referencia - Ativo.InicioSessao - Ativo.TempoPausadoFechado;
            return decorrido < TimeSpan.Zero ? TimeSpan.Zero : decorrido;
        }
    }

    /// <summary>Carrega, se existir, um cronômetro deixado ativo de uma execução anterior do app.</summary>
    public async Task InicializarAsync()
    {
        if (_inicializado)
        {
            return;
        }

        _inicializado = true;

        using var scope = _scopeFactory.CreateScope();
        var horasService = scope.ServiceProvider.GetRequiredService<IRastreamentoHorasService>();
        var ativo = await horasService.ObterEmAndamentoAsync();

        if (ativo is null)
        {
            return;
        }

        await AtualizarProjetoNomeAsync(scope, ativo.ProjetoId);
        DefinirAtivo(ativo);
    }

    public async Task IniciarAsync(int projetoId)
    {
        using var scope = _scopeFactory.CreateScope();
        var horasService = scope.ServiceProvider.GetRequiredService<IRastreamentoHorasService>();

        var registro = await horasService.IniciarAsync(projetoId);
        await AtualizarProjetoNomeAsync(scope, projetoId);
        DefinirAtivo(registro);
    }

    /// <summary>Para o cronômetro ativo (se houver) sem pedir categoria/nota — usado quando a usuária troca de projeto com um cronômetro já rodando.</summary>
    public async Task PararSemCategorizarAsync()
    {
        if (Ativo is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var horasService = scope.ServiceProvider.GetRequiredService<IRastreamentoHorasService>();
        await horasService.PararAsync(Ativo.Id, categoria: null, nota: null);

        LimparAtivo();
    }

    public async Task PausarAsync()
    {
        if (Ativo is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var horasService = scope.ServiceProvider.GetRequiredService<IRastreamentoHorasService>();
        var registro = await horasService.PausarAsync(Ativo.Id);

        DefinirAtivo(registro);
    }

    public async Task RetomarAsync()
    {
        if (Ativo is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var horasService = scope.ServiceProvider.GetRequiredService<IRastreamentoHorasService>();
        var registro = await horasService.RetomarAsync(Ativo.Id);

        DefinirAtivo(registro);
    }

    public async Task<RegistroHorasDto?> PararAsync(CategoriaHoras? categoria, string? nota)
    {
        if (Ativo is null)
        {
            return null;
        }

        using var scope = _scopeFactory.CreateScope();
        var horasService = scope.ServiceProvider.GetRequiredService<IRastreamentoHorasService>();
        var concluido = await horasService.PararAsync(Ativo.Id, categoria, nota);

        LimparAtivo();

        return concluido;
    }

    private async Task AtualizarProjetoNomeAsync(IServiceScope scope, int projetoId)
    {
        var projetoService = scope.ServiceProvider.GetRequiredService<IProjetoService>();
        var projeto = await projetoService.ObterPorIdAsync(projetoId);
        ProjetoNomeAtivo = projeto?.Nome;
    }

    private void DefinirAtivo(RegistroHorasDto registro)
    {
        Ativo = registro;

        if (!_timer.Enabled)
        {
            _timer.Start();
        }

        OnChange?.Invoke();
    }

    private void LimparAtivo()
    {
        Ativo = null;
        ProjetoNomeAtivo = null;
        _timer.Stop();
        OnChange?.Invoke();
    }

    public void Dispose()
    {
        _timer.Elapsed -= _timerHandler;
        _timer.Dispose();
    }
}
