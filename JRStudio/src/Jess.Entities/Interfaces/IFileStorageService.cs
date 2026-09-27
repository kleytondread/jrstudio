namespace Jess.Entities.Interfaces;

/// <summary>Copia/remove os arquivos físicos da Biblioteca (Seção 2.5). Nunca guarda caminho absoluto no banco.</summary>
public interface IFileStorageService
{
    /// <summary>Copia o arquivo de origem para a pasta de arquivos do app, organizado por data de importação. Retorna o caminho relativo salvo e o tamanho em bytes.</summary>
    (string CaminhoRelativo, long TamanhoBytes) Armazenar(string caminhoOrigemAbsoluto);

    string ResolverCaminhoAbsoluto(string caminhoRelativo);

    void Remover(string caminhoRelativo);
}
