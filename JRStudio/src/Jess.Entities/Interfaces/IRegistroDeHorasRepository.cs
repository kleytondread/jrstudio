using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface IRegistroDeHorasRepository
{
    Task<List<RegistroDeHoras>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<RegistroDeHoras?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Só pode existir um registro não concluído por vez no app inteiro (cronômetro único e global).</summary>
    Task<RegistroDeHoras?> ObterEmAndamentoAsync(CancellationToken cancellationToken = default);

    Task AdicionarAsync(RegistroDeHoras registro, CancellationToken cancellationToken = default);

    void Remover(RegistroDeHoras registro);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
