namespace Jess.Entities.Entities;

/// <summary>
/// Linha única de configurações do app (sempre Id = 1). Guarda hoje só o nome da arquiteta, exibido
/// na saudação do Dashboard — pensado como semente de um futuro cadastro de usuário mais completo,
/// não como o cadastro em si.
/// </summary>
public class Configuracao
{
    public int Id { get; set; }

    public string? NomeArquiteto { get; set; }
}
