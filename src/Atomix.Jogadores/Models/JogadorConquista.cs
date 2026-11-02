using System.Text.Json.Serialization;

namespace Atomix.Jogadores.Models;

/// <summary>
/// Associação N:N entre Jogador e Conquista, com a data do desbloqueio. Tabela TB_ATX_JOGADOR_CONQUISTA no Oracle.
/// </summary>
public class JogadorConquista
{
    public int IdJogador { get; set; }
    public int IdConquista { get; set; }
    public DateTime DataDesbloqueio { get; set; }

    [JsonIgnore] public Jogador? Jogador { get; set; }
    [JsonIgnore] public Conquista? Conquista { get; set; }
}
