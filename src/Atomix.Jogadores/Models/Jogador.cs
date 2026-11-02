using System.Text.Json.Serialization;

namespace Atomix.Jogadores.Models;

/// <summary>
/// Jogador do ATOMIX. Tabela TB_ATX_JOGADOR no Oracle (modelo do projeto de Banco de Dados).
/// </summary>
public class Jogador
{
    public int IdJogador { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>Hash BCrypt da senha (nunca a senha em texto). Coluna SENHA.</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Pontuação acumulada. Coluna PONTUACAO.</summary>
    public int Pontos { get; set; }
    public DateTime DataCadastro { get; set; }

    /// <summary>Conquistas já desbloqueadas (tabela TB_ATX_JOGADOR_CONQUISTA).</summary>
    public List<JogadorConquista> Conquistas { get; set; } = new();

    /// <summary>Nível derivado dos pontos — não é persistido.</summary>
    [JsonIgnore]
    public NivelJogador Nivel => NivelJogadorRegras.Calcular(Pontos);

    public bool PossuiConquista(int idConquista) => Conquistas.Any(c => c.IdConquista == idConquista);
}
