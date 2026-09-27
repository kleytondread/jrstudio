using Jess.Entities.Enums;

namespace Jess.Application.Services;

/// <summary>
/// Regras de cálculo do módulo Financeiro (Seção 6.3 da especificação). Extraídas como funções
/// puras — sem repositório nem estado — para serem testáveis diretamente, sem mocks.
/// </summary>
public static class FinanceiroCalculos
{
    public static StatusPagamento CalcularStatus(decimal? valorRecebido, DateOnly dataPrevista, DateOnly hoje)
    {
        if (valorRecebido is not null)
        {
            return StatusPagamento.Recebido;
        }

        return dataPrevista < hoje ? StatusPagamento.Atrasado : StatusPagamento.Pendente;
    }

    public static decimal CalcularSaldo(decimal valorTotalAcordado, IEnumerable<decimal?> valoresRecebidos)
    {
        return valorTotalAcordado - valoresRecebidos.Sum(v => v ?? 0m);
    }
}
