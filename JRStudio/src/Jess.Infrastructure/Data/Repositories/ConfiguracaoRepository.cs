using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class ConfiguracaoRepository : IConfiguracaoRepository
{
    private const int IdUnico = 1;

    private readonly AppDbContext _dbContext;

    public ConfiguracaoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Configuracao> ObterOuCriarAsync(CancellationToken cancellationToken = default)
    {
        var configuracao = await _dbContext.Configuracoes.FirstOrDefaultAsync(c => c.Id == IdUnico, cancellationToken);
        if (configuracao is not null)
        {
            return configuracao;
        }

        configuracao = new Configuracao { Id = IdUnico };
        await _dbContext.Configuracoes.AddAsync(configuracao, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return configuracao;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
