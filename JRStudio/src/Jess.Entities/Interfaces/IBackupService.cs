using Jess.Entities.Enums;

namespace Jess.Entities.Interfaces;

public interface IBackupService
{
    /// <summary>Gera o arquivo .zip de backup no caminho indicado.</summary>
    Task CriarBackupAsync(TipoBackup tipo, string destinoZipPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restaura o banco (e, se for um backup completo, a pasta de arquivos) a partir do .zip indicado,
    /// substituindo integralmente os dados locais correspondentes. Retorna o tipo de backup identificado.
    /// </summary>
    Task<TipoBackup> RestaurarBackupAsync(string origemZipPath, CancellationToken cancellationToken = default);
}
