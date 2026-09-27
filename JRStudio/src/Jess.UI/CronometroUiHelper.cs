using Jess.Application.Services;

namespace Jess.UI;

/// <summary>
/// Orquestra a regra "só um cronômetro ativo por vez" (Seção 2.4 da especificação) do lado da UI —
/// o <see cref="CronometroStateService"/> fica em Jess.Application e não conhece a confirmação com a
/// usuária (isso é uma questão de UI), então ela mora aqui. Compartilhado entre o widget da sidebar e
/// a aba Horas.
/// </summary>
public static class CronometroUiHelper
{
    public static async Task<bool> IniciarComConfirmacaoAsync(CronometroStateService state, ConfirmState confirmState, int projetoId)
    {
        if (state.Ativo is not null && state.Ativo.ProjetoId == projetoId)
        {
            return true;
        }

        if (state.Ativo is not null)
        {
            var confirmado = await confirmState.ConfirmarAsync(
                $"Já existe um cronômetro ativo em \"{state.ProjetoNomeAtivo}\". Deseja pará-lo e iniciar neste projeto?",
                "Cronômetro já ativo");

            if (!confirmado)
            {
                return false;
            }

            await state.PararSemCategorizarAsync();
        }

        await state.IniciarAsync(projetoId);
        return true;
    }
}
