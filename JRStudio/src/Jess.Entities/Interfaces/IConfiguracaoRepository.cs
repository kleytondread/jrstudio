using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface IConfiguracaoRepository
{
    /// <summary>Retorna a linha única de configurações, criando-a (com valores padrão) se ainda não existir.</summary>
    Task<Configuracao> ObterOuCriarAsync(CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
