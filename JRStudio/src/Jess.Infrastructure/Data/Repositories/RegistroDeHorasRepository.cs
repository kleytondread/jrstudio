using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class RegistroDeHorasRepository : IRegistroDeHorasRepository
{
    private readonly AppDbContext _dbContext;

    public RegistroDeHorasRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private IQueryable<RegistroDeHoras> ComIncludes() => _dbContext.RegistrosDeHoras
        .Include(r => r.Segmentos)
        .Include(r => r.Pausas)
        .AsSplitQuery();

    public async Task<List<RegistroDeHoras>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        // AsNoTracking: este método só alimenta telas de leitura. Sem ele, a entidade fica presa no
        // identity map do DbContext (de vida longa nesta app — um único escopo por sessão do
        // BlazorWebView) e uma escrita feita por OUTRO DbContext (ex.: CronometroStateService, que usa
        // seu próprio escopo via IServiceScopeFactory) não é refletida aqui: a consulta volta com a
        // instância antiga já rastreada em vez dos valores atuais do banco.
        return await ComIncludes()
            .AsNoTracking()
            .Where(r => r.ProjetoId == projetoId)
            .OrderByDescending(r => r.InicioSessao)
            .ToListAsync(cancellationToken);
    }

    public async Task<RegistroDeHoras?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ComIncludes().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<RegistroDeHoras?> ObterEmAndamentoAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking: mesmo motivo de ListarPorProjetoAsync — usado só para leitura/checagem.
        return await ComIncludes()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Status != StatusRegistroHoras.Concluido, cancellationToken);
    }

    public async Task AdicionarAsync(RegistroDeHoras registro, CancellationToken cancellationToken = default)
    {
        await _dbContext.RegistrosDeHoras.AddAsync(registro, cancellationToken);
    }

    public void Remover(RegistroDeHoras registro)
    {
        _dbContext.RegistrosDeHoras.Remove(registro);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
