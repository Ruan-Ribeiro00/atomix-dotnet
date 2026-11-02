namespace Atomix.Jogadores.Models;

/// <summary>
/// Conquista que o jogador desbloqueia ao atingir uma pontuação. Tabela TB_ATX_CONQUISTA no Oracle.
/// </summary>
public class Conquista
{
    public int IdConquista { get; set; }

    /// <summary>Área temática da conquista (nula = conquista geral).</summary>
    public int? IdArea { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int PontosNecessarios { get; set; }
}
