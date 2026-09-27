using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jess.Application.Services;

public class ArquivoService : IArquivoService
{
    private readonly IArquivoRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<ArquivoService> _logger;

    public ArquivoService(IArquivoRepository repository, IFileStorageService fileStorage, ILogger<ArquivoService> logger)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ArquivoDto>> ListarAsync(int? projetoId = null, CancellationToken cancellationToken = default)
    {
        var arquivos = await _repository.ListarAsync(projetoId, cancellationToken);
        return arquivos.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<string>> ListarTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _repository.ListarTagsAsync(cancellationToken);
        return tags.Select(t => t.Nome).ToList();
    }

    public async Task<ArquivoDto> ImportarAsync(
        string caminhoOrigemAbsoluto,
        IReadOnlyCollection<int>? projetoIds = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var (caminhoRelativo, tamanhoBytes) = _fileStorage.Armazenar(caminhoOrigemAbsoluto);
        var nomeArquivo = Path.GetFileName(caminhoOrigemAbsoluto);

        var arquivo = new Arquivo
        {
            NomeArquivo = nomeArquivo,
            CaminhoArquivo = caminhoRelativo,
            TipoArquivo = InferirTipo(Path.GetExtension(nomeArquivo)),
            TamanhoBytes = tamanhoBytes,
            DataImportacao = DateTime.Now
        };

        foreach (var nomeTag in NormalizarTags(tags))
        {
            arquivo.Tags.Add(await _repository.ObterOuCriarTagAsync(nomeTag, cancellationToken));
        }

        foreach (var projetoId in projetoIds ?? [])
        {
            var projeto = await _repository.ObterProjetoAsync(projetoId, cancellationToken);
            if (projeto is not null)
            {
                arquivo.Projetos.Add(projeto);
            }
        }

        await _repository.AdicionarAsync(arquivo, cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Arquivo {ArquivoId} importado ({Nome})", arquivo.Id, arquivo.NomeArquivo);

        return MapToDto(arquivo);
    }

    public async Task VincularProjetoAsync(int arquivoId, int projetoId, CancellationToken cancellationToken = default)
    {
        var arquivo = await ObterOuFalharAsync(arquivoId, cancellationToken);
        if (arquivo.Projetos.Any(p => p.Id == projetoId))
        {
            return;
        }

        var projeto = await _repository.ObterProjetoAsync(projetoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Projeto {projetoId} não encontrado.");

        arquivo.Projetos.Add(projeto);
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task DesvincularProjetoAsync(int arquivoId, int projetoId, CancellationToken cancellationToken = default)
    {
        var arquivo = await ObterOuFalharAsync(arquivoId, cancellationToken);
        var projeto = arquivo.Projetos.FirstOrDefault(p => p.Id == projetoId);

        if (projeto is not null)
        {
            arquivo.Projetos.Remove(projeto);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
        }
    }

    public async Task AtualizarTagsAsync(int arquivoId, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        var arquivo = await ObterOuFalharAsync(arquivoId, cancellationToken);
        arquivo.Tags.Clear();

        foreach (var nomeTag in NormalizarTags(tags))
        {
            arquivo.Tags.Add(await _repository.ObterOuCriarTagAsync(nomeTag, cancellationToken));
        }

        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task AlternarFavoritoAsync(int arquivoId, CancellationToken cancellationToken = default)
    {
        var arquivo = await ObterOuFalharAsync(arquivoId, cancellationToken);
        arquivo.Favorito = !arquivo.Favorito;
        await _repository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(int arquivoId, CancellationToken cancellationToken = default)
    {
        var arquivo = await ObterOuFalharAsync(arquivoId, cancellationToken);
        var caminhoRelativo = arquivo.CaminhoArquivo;

        _repository.Remover(arquivo);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _fileStorage.Remover(caminhoRelativo);

        _logger.LogInformation("Arquivo {ArquivoId} excluído ({Nome})", arquivoId, arquivo.NomeArquivo);
    }

    private static IEnumerable<string> NormalizarTags(IReadOnlyCollection<string>? tags) =>
        (tags ?? [])
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private async Task<Arquivo> ObterOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Arquivo {id} não encontrado.");

    private static TipoArquivo InferirTipo(string extensao) => extensao.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" => TipoArquivo.Imagem,
        ".pdf" => TipoArquivo.PDF,
        ".doc" or ".docx" or ".txt" or ".rtf" or ".odt" => TipoArquivo.Documento,
        _ => TipoArquivo.Outro
    };

    private static ArquivoDto MapToDto(Arquivo arquivo) => new()
    {
        Id = arquivo.Id,
        NomeArquivo = arquivo.NomeArquivo,
        CaminhoRelativo = arquivo.CaminhoArquivo,
        TipoArquivo = arquivo.TipoArquivo,
        TamanhoBytes = arquivo.TamanhoBytes,
        DataImportacao = arquivo.DataImportacao,
        Favorito = arquivo.Favorito,
        Tags = arquivo.Tags.Select(t => t.Nome).OrderBy(n => n).ToList(),
        ProjetoIds = arquivo.Projetos.Select(p => p.Id).ToList()
    };
}
