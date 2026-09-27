using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _dbContext;

    public ClienteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cliente> ObterOuCriarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var existente = await _dbContext.Clientes
            .FirstOrDefaultAsync(c => c.Nome == nome, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var cliente = new Cliente
        {
            Nome = nome,
            DataCadastro = DateTime.Now
        };

        await _dbContext.Clientes.AddAsync(cliente, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return cliente;
    }
}
