using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface IClienteRepository
{
    Task<Cliente> ObterOuCriarPorNomeAsync(string nome, CancellationToken cancellationToken = default);
}
