namespace Jess.Application.Interfaces;

/// <summary>Ações de ciclo de vida do processo do app, implementadas no host (Jess.Desktop).</summary>
public interface IAppLifecycleService
{
    /// <summary>Encerra o processo atual e abre uma nova instância — usado após restaurar um backup.</summary>
    void Reiniciar();
}
