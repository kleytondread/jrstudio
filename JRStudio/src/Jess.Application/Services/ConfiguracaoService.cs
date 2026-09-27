using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jess.Application.Services;

public class ConfiguracaoService : IConfiguracaoService
{
    private readonly IConfiguracaoRepository _repository;
    private readonly ILogger<ConfiguracaoService> _logger;

    public ConfiguracaoService(IConfiguracaoRepository repository, ILogger<ConfiguracaoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConfiguracaoDto> ObterAsync(CancellationToken cancellationToken = default)
    {
        var configuracao = await _repository.ObterOuCriarAsync(cancellationToken);
        return new ConfiguracaoDto { NomeArquiteto = configuracao.NomeArquiteto };
    }

    public async Task<ConfiguracaoDto> AtualizarNomeArquitetoAsync(string? nomeArquiteto, CancellationToken cancellationToken = default)
    {
        var configuracao = await _repository.ObterOuCriarAsync(cancellationToken);
        configuracao.NomeArquiteto = string.IsNullOrWhiteSpace(nomeArquiteto) ? null : nomeArquiteto.Trim();

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Nome da arquiteta atualizado nas configurações");

        return new ConfiguracaoDto { NomeArquiteto = configuracao.NomeArquiteto };
    }
}
