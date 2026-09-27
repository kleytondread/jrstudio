using FluentAssertions;
using Jess.Entities.Enums;

namespace Jess.Entities.Tests.Enums;

public class FaseProjetoTests
{
    [Fact]
    public void GetValues_RetornaAs4FasesNaOrdemFixaDoProcessoDeProjetoArquitetonico()
    {
        var fases = Enum.GetValues<FaseProjeto>();

        fases.Should().Equal(
            FaseProjeto.PreProjeto,
            FaseProjeto.EstudoPreliminar,
            FaseProjeto.Anteprojeto,
            FaseProjeto.ProjetoExecutivo);
    }
}
