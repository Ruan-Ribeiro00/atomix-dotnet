using Atomix.Jogadores.Models;

namespace Atomix.Jogadores.Data;

/// <summary>
/// Contrato de persistência. Há duas implementações: arquivo JSON (padrão) e Oracle via EF Core.
/// </summary>
public interface IAtomixRepository : IDisposable
{
    /// <summary>Texto curto para mostrar ao usuário onde os dados estão sendo salvos.</summary>
    string Descricao { get; }

    IReadOnlyList<Jogador> ListarJogadores();
    Jogador? ObterJogador(int idJogador);
    void AdicionarJogador(Jogador jogador);

    /// <summary>Grava as alterações feitas num jogador obtido por <see cref="ObterJogador"/>.</summary>
    void AtualizarJogador(Jogador jogador);

    void RemoverJogador(int idJogador);

    IReadOnlyList<Conquista> ListarConquistas();
}
