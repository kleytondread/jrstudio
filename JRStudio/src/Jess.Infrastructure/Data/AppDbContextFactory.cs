using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jess.Infrastructure.Data;

// Usado pela CLI do EF Core (dotnet ef migrations add / update) em tempo de design,
// já que Jess.Desktop (WPF) não expõe o padrão Program.CreateBuilder esperado pelas ferramentas do EF.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabaseFilePath}");

        return new AppDbContext(optionsBuilder.Options);
    }
}
