using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jess.Application.Services;

public class ProjetoService : IProjetoService
{
    private readonly IProjetoRepository _projetoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<ProjetoService> _logger;

    public ProjetoService(
        IProjetoRepository projetoRepository,
        IClienteRepository clienteRepository,
        ILogger<ProjetoService> logger)
    {
        _projetoRepository = projetoRepository;
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProjetoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var projetos = await _projetoRepository.ListarAsync(cancellationToken);
        return projetos.Select(MapToDto).ToList();
    }

    public async Task<ProjetoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken);
        return projeto is null ? null : MapToDto(projeto);
    }

    public async Task<ProjetoDto> CriarAsync(SalvarProjetoRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterOuCriarPorNomeAsync(request.ClienteNome, cancellationToken);

        var projeto = new Projeto
        {
            Nome = request.Nome,
            ClienteId = cliente.Id,
            Cliente = cliente,
            Endereco = request.Endereco,
            DataInicio = request.DataInicio!.Value,
            PrazoEstimado = request.PrazoEstimado,
            ValorTotalAcordado = request.ValorTotalAcordado,
            Observacoes = request.Observacoes,
            Status = request.Status,
            DataCriacao = DateTime.Now,
            DataConclusao = request.Status == StatusProjeto.Concluido ? DateTime.Now : null
        };

        await _projetoRepository.AdicionarAsync(projeto, cancellationToken);
        await _projetoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Projeto {ProjetoId} criado ({Nome})", projeto.Id, projeto.Nome);

        return MapToDto(projeto);
    }

    public async Task<ProjetoDto> AtualizarAsync(int id, SalvarProjetoRequest request, CancellationToken cancellationToken = default)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Projeto {id} não encontrado.");

        var cliente = await _clienteRepository.ObterOuCriarPorNomeAsync(request.ClienteNome, cancellationToken);

        projeto.Nome = request.Nome;
        projeto.ClienteId = cliente.Id;
        projeto.Cliente = cliente;
        projeto.Endereco = request.Endereco;
        projeto.DataInicio = request.DataInicio!.Value;
        projeto.PrazoEstimado = request.PrazoEstimado;
        projeto.ValorTotalAcordado = request.ValorTotalAcordado;
        projeto.Observacoes = request.Observacoes;
        AplicarStatus(projeto, request.Status);

        await _projetoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Projeto {ProjetoId} atualizado", projeto.Id);

        return MapToDto(projeto);
    }

    public async Task ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Projeto {id} não encontrado.");

        _projetoRepository.Remover(projeto);
        await _projetoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Projeto {ProjetoId} excluído ({Nome})", id, projeto.Nome);
    }

    public async Task<ProjetoDto> AlterarStatusAsync(int id, StatusProjeto novoStatus, CancellationToken cancellationToken = default)
    {
        var projeto = await _projetoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Projeto {id} não encontrado.");

        AplicarStatus(projeto, novoStatus);

        await _projetoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Projeto {ProjetoId} mudou de status para {Status}", projeto.Id, novoStatus);

        return MapToDto(projeto);
    }

    private static void AplicarStatus(Projeto projeto, StatusProjeto novoStatus)
    {
        if (novoStatus == StatusProjeto.Concluido && projeto.Status != StatusProjeto.Concluido)
        {
            projeto.DataConclusao = DateTime.Now;
        }
        else if (novoStatus != StatusProjeto.Concluido)
        {
            projeto.DataConclusao = null;
        }

        projeto.Status = novoStatus;
    }

    private static ProjetoDto MapToDto(Projeto projeto) => new()
    {
        Id = projeto.Id,
        Nome = projeto.Nome,
        ClienteNome = projeto.Cliente?.Nome ?? string.Empty,
        Endereco = projeto.Endereco,
        DataInicio = projeto.DataInicio,
        PrazoEstimado = projeto.PrazoEstimado,
        Status = projeto.Status,
        ValorTotalAcordado = projeto.ValorTotalAcordado,
        Observacoes = projeto.Observacoes,
        DataCriacao = projeto.DataCriacao,
        DataConclusao = projeto.DataConclusao
    };
}
