namespace Atomix.Jogadores.Models;

/// <summary>
/// As 5 áreas da cidade nuclear do ATOMIX (mesmas do app Flutter, da API Java e da tabela AREA no Oracle).
/// </summary>
public enum AreaNuclear
{
    Saude = 1,
    Agro = 2,
    Industria = 3,
    Energia = 4,
    Pesquisa = 5
}

public static class AreaNuclearExtensions
{
    public static string Nome(this AreaNuclear area) => area switch
    {
        AreaNuclear.Saude => "Saúde",
        AreaNuclear.Agro => "Agro",
        AreaNuclear.Industria => "Indústria",
        AreaNuclear.Energia => "Energia",
        AreaNuclear.Pesquisa => "Pesquisa",
        _ => area.ToString()
    };
}
