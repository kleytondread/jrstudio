using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jess.Application.Services;

public class FinanceiroService : IFinanceiroService
{
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly ILogger<FinanceiroService> _logger;

    public FinanceiroService(IPagamentoRepository pagamentoRepository, ILogger<FinanceiroService> logger)
    {
        _pagamentoRepository = pagamentoRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PagamentoDto>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var pagamentos = await _pagamentoRepository.ListarPorProjetoAsync(projetoId, cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        return pagamentos.Select(p => MapToDto(p, hoje)).ToList();
    }

    public async Task<IReadOnlyList<PagamentoDto>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        var pagamentos = await _pagamentoRepository.ListarTodosAsync(cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        return pagamentos.Select(p => MapToDto(p, hoje)).ToList();
    }

    public async Task<PagamentoDto> CriarAsync(int projetoId, SalvarPagamentoRequest request, CancellationToken cancellationToken = default)
    {
        var pagamento = new Pagamento
        {
            ProjetoId = projetoId,
            ValorPrevisto = request.ValorPrevisto,
            DataPrevista = request.DataPrevista!.Value,
            Observacoes = request.Observacoes
        };

        await _pagamentoRepository.AdicionarAsync(pagamento, cancellationToken);
        await _pagamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Parcela {PagamentoId} criada para o projeto {ProjetoId}", pagamento.Id, projetoId);

        return MapToDto(pagamento, DateOnly.FromDateTime(DateTime.Now));
    }

    public async Task<PagamentoDto> AtualizarAsync(int id, SalvarPagamentoRequest request, CancellationToken cancellationToken = default)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Pagamento {id} não encontrado.");

        pagamento.ValorPrevisto = request.ValorPrevisto;
        pagamento.DataPrevista = request.DataPrevista!.Value;
        pagamento.Observacoes = request.Observacoes;

        await _pagamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Parcela {PagamentoId} atualizada", id);

        return MapToDto(pagamento, DateOnly.FromDateTime(DateTime.Now));
    }

    public async Task ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Pagamento {id} não encontrado.");

        _pagamentoRepository.Remover(pagamento);
        await _pagamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Parcela {PagamentoId} excluída", id);
    }

    public async Task<PagamentoDto> MarcarComoRecebidoAsync(int id, CancellationToken cancellationToken = default)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Pagamento {id} não encontrado.");

        pagamento.ValorRecebido = pagamento.ValorPrevisto;
        pagamento.DataRecebimento = DateOnly.FromDateTime(DateTime.Now);

        await _pagamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Parcela {PagamentoId} marcada como recebida", id);

        return MapToDto(pagamento, DateOnly.FromDateTime(DateTime.Now));
    }

    public async Task<decimal> ObterSaldoAsync(int projetoId, decimal valorTotalAcordado, CancellationToken cancellationToken = default)
    {
        var pagamentos = await _pagamentoRepository.ListarPorProjetoAsync(projetoId, cancellationToken);
        return FinanceiroCalculos.CalcularSaldo(valorTotalAcordado, pagamentos.Select(p => p.ValorRecebido));
    }

    private static PagamentoDto MapToDto(Pagamento pagamento, DateOnly hoje) => new()
    {
        Id = pagamento.Id,
        ProjetoId = pagamento.ProjetoId,
        ValorPrevisto = pagamento.ValorPrevisto,
        DataPrevista = pagamento.DataPrevista,
        ValorRecebido = pagamento.ValorRecebido,
        DataRecebimento = pagamento.DataRecebimento,
        Observacoes = pagamento.Observacoes,
        Status = FinanceiroCalculos.CalcularStatus(pagamento.ValorRecebido, pagamento.DataPrevista, hoje)
    };
}
