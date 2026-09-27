using System.IO.Compression;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Jess.Infrastructure.Backup;

/// <summary>
/// Gera e restaura backups (Seção 2.7 da especificação): "completo" empacota o banco de dados e a
/// pasta de arquivos importados da Biblioteca; "só-dados" empacota apenas o banco. O tipo de um
/// backup existente é identificado automaticamente pela presença (ou não) da pasta "Arquivos"
/// dentro do .zip — nenhum manifesto extra é necessário.
///
/// Os caminhos do banco/pasta de arquivos são recebidos no construtor (em vez de lidos direto de
/// <see cref="AppPaths"/>) só para permitir testar contra arquivos temporários, sem tocar nos dados
/// reais da usuária em %AppData%. Em produção, a injeção de dependência usa os valores padrão de
/// <see cref="AppPaths"/> (ver <see cref="DependencyInjection.AddInfrastructure"/>).
/// </summary>
public class BackupService : IBackupService
{
    private const string NomeArquivoBanco = "dados.db";
    private const string NomePastaArquivos = "Arquivos";

    private readonly string _bancoDadosPath;
    private readonly string _arquivosFolderPath;
    private readonly ILogger<BackupService> _logger;

    public BackupService(ILogger<BackupService> logger)
        : this(AppPaths.DatabaseFilePath, AppPaths.ArquivosFolder, logger)
    {
    }

    public BackupService(string bancoDadosPath, string arquivosFolderPath, ILogger<BackupService> logger)
    {
        _bancoDadosPath = bancoDadosPath;
        _arquivosFolderPath = arquivosFolderPath;
        _logger = logger;
    }

    public async Task CriarBackupAsync(TipoBackup tipo, string destinoZipPath, CancellationToken cancellationToken = default)
    {
        var tempDir = CriarPastaTemporaria();
        try
        {
            await CopiarBancoDeDadosAsync(_bancoDadosPath, Path.Combine(tempDir, NomeArquivoBanco), cancellationToken);

            // O Microsoft.Data.Sqlite mantém a conexão de destino em um pool nativo mesmo após o
            // DisposeAsync — sem isso, o arquivo temporário fica com um handle aberto e a limpeza
            // da pasta temporária no `finally` falha com "arquivo em uso por outro processo".
            SqliteConnection.ClearAllPools();

            if (tipo == TipoBackup.Completo)
            {
                CopiarDiretorio(_arquivosFolderPath, Path.Combine(tempDir, NomePastaArquivos));
            }

            if (File.Exists(destinoZipPath))
            {
                File.Delete(destinoZipPath);
            }

            ZipFile.CreateFromDirectory(tempDir, destinoZipPath);

            _logger.LogInformation("Backup ({Tipo}) gerado em {Destino}", tipo, destinoZipPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    public async Task<TipoBackup> RestaurarBackupAsync(string origemZipPath, CancellationToken cancellationToken = default)
    {
        var tempDir = CriarPastaTemporaria();
        try
        {
            ZipFile.ExtractToDirectory(origemZipPath, tempDir, overwriteFiles: true);

            var bancoExtraido = Path.Combine(tempDir, NomeArquivoBanco);
            if (!File.Exists(bancoExtraido))
            {
                throw new InvalidOperationException("Arquivo de backup inválido: banco de dados não encontrado dentro do .zip.");
            }

            var pastaArquivosExtraida = Path.Combine(tempDir, NomePastaArquivos);
            var tipo = Directory.Exists(pastaArquivosExtraida) ? TipoBackup.Completo : TipoBackup.SoDados;

            Directory.CreateDirectory(Path.GetDirectoryName(_bancoDadosPath)!);

            SqliteConnection.ClearAllPools();
            RemoverArquivosAuxiliaresSqlite(_bancoDadosPath);
            await CopiarComRetentativasAsync(bancoExtraido, _bancoDadosPath, cancellationToken);

            if (tipo == TipoBackup.Completo)
            {
                if (Directory.Exists(_arquivosFolderPath))
                {
                    Directory.Delete(_arquivosFolderPath, recursive: true);
                }

                CopiarDiretorio(pastaArquivosExtraida, _arquivosFolderPath);
            }

            _logger.LogInformation("Backup restaurado ({Tipo}) a partir de {Origem}", tipo, origemZipPath);

            return tipo;
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static async Task CopiarBancoDeDadosAsync(string origemDbPath, string destinoDbPath, CancellationToken cancellationToken)
    {
        await using var origem = new SqliteConnection($"Data Source={origemDbPath};Mode=ReadOnly");
        await origem.OpenAsync(cancellationToken);

        await using var destino = new SqliteConnection($"Data Source={destinoDbPath}");
        await destino.OpenAsync(cancellationToken);

        origem.BackupDatabase(destino);
    }

    private static async Task CopiarComRetentativasAsync(string origem, string destino, CancellationToken cancellationToken)
    {
        const int tentativas = 5;

        for (var tentativa = 1; tentativa <= tentativas; tentativa++)
        {
            try
            {
                File.Copy(origem, destino, overwrite: true);
                return;
            }
            catch (IOException) when (tentativa < tentativas)
            {
                await Task.Delay(300, cancellationToken);
            }
        }
    }

    private static void RemoverArquivosAuxiliaresSqlite(string dbPath)
    {
        foreach (var sufixo in new[] { "-wal", "-shm", "-journal" })
        {
            var caminho = dbPath + sufixo;
            if (File.Exists(caminho))
            {
                File.Delete(caminho);
            }
        }
    }

    private static void CopiarDiretorio(string origem, string destino)
    {
        Directory.CreateDirectory(destino);

        if (!Directory.Exists(origem))
        {
            return;
        }

        foreach (var arquivo in Directory.GetFiles(origem))
        {
            File.Copy(arquivo, Path.Combine(destino, Path.GetFileName(arquivo)), overwrite: true);
        }

        foreach (var subDiretorio in Directory.GetDirectories(origem))
        {
            CopiarDiretorio(subDiretorio, Path.Combine(destino, Path.GetFileName(subDiretorio)));
        }
    }

    private static string CriarPastaTemporaria()
    {
        var caminho = Path.Combine(Path.GetTempPath(), $"JRStudioBackup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(caminho);
        return caminho;
    }
}
