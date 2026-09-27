using Jess.Entities.Interfaces;

namespace Jess.Infrastructure.FileSystem;

public class FileStorageService : IFileStorageService
{
    private readonly string _raizArquivos;

    public FileStorageService() : this(AppPaths.ArquivosFolder)
    {
    }

    public FileStorageService(string raizArquivos)
    {
        _raizArquivos = raizArquivos;
    }

    public (string CaminhoRelativo, long TamanhoBytes) Armazenar(string caminhoOrigemAbsoluto)
    {
        var hoje = DateTime.Now;
        var pastaRelativa = Path.Combine(hoje.Year.ToString(), hoje.Month.ToString("D2"), hoje.Day.ToString("D2"));
        var pastaAbsoluta = Path.Combine(_raizArquivos, pastaRelativa);
        Directory.CreateDirectory(pastaAbsoluta);

        var nomeFinal = GarantirNomeUnico(pastaAbsoluta, Path.GetFileName(caminhoOrigemAbsoluto));
        var destinoAbsoluto = Path.Combine(pastaAbsoluta, nomeFinal);

        File.Copy(caminhoOrigemAbsoluto, destinoAbsoluto, overwrite: false);

        var caminhoRelativo = Path.Combine(pastaRelativa, nomeFinal);
        var tamanhoBytes = new FileInfo(destinoAbsoluto).Length;

        return (caminhoRelativo, tamanhoBytes);
    }

    public string ResolverCaminhoAbsoluto(string caminhoRelativo) => Path.Combine(_raizArquivos, caminhoRelativo);

    public void Remover(string caminhoRelativo)
    {
        var caminhoAbsoluto = ResolverCaminhoAbsoluto(caminhoRelativo);
        if (File.Exists(caminhoAbsoluto))
        {
            File.Delete(caminhoAbsoluto);
        }
    }

    private static string GarantirNomeUnico(string pastaAbsoluta, string nomeArquivo)
    {
        var nomeFinal = nomeArquivo;
        var contador = 1;

        while (File.Exists(Path.Combine(pastaAbsoluta, nomeFinal)))
        {
            var semExtensao = Path.GetFileNameWithoutExtension(nomeArquivo);
            var extensao = Path.GetExtension(nomeArquivo);
            nomeFinal = $"{semExtensao} ({contador}){extensao}";
            contador++;
        }

        return nomeFinal;
    }
}
