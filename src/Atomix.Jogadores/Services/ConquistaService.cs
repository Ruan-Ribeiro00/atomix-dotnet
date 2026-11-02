using Atomix.Jogadores.Data;
using Atomix.Jogadores.Models;

namespace Atomix.Jogadores.Services;

/// <summary>
/// Regra de negócio de conquistas: uma conquista é desbloqueada automaticamente
/// quando os pontos do jogador alcançam os pontos necessários dela.
/// Cada conquista é desbloqueada uma única vez e nunca é retirada.
/// </summary>
public class ConquistaService
{
    private readonly IAtomixRepository _repositorio;

    public ConquistaService(IAtomixRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public IReadOnlyList<Conquista> ListarCatalogo() => _repositorio.ListarConquistas();

    /// <summary>
    /// Aplica a regra de desbloqueio no jogador (sem salvar) e devolve as conquistas novas.
    /// </summary>
    public List<Conquista> AplicarDesbloqueios(Jogador jogador, DateTime agora)
    {
        var novas = _repositorio.ListarConquistas()
            .Where(c => jogador.Pontos >= c.PontosNecessarios && !jogador.PossuiConquista(c.IdConquista))
            .OrderBy(c => c.PontosNecessarios)
            .ToList();

        foreach (var conquista in novas)
        {
            jogador.Conquistas.Add(new JogadorConquista
            {
                IdJogador = jogador.IdJogador,
                IdConquista = conquista.IdConquista,
                DataDesbloqueio = agora
            });
        }

        return novas;
    }

    /// <summary>Conquistas do jogador com a data de desbloqueio, da mais antiga para a mais recente.</summary>
    public List<(Conquista Conquista, DateTime Data)> ConquistasDoJogador(Jogador jogador)
    {
        var catalogo = _repositorio.ListarConquistas().ToDictionary(c => c.IdConquista);
        return jogador.Conquistas
            .Where(jc => catalogo.ContainsKey(jc.IdConquista))
            .Select(jc => (catalogo[jc.IdConquista], jc.DataDesbloqueio))
            .OrderBy(x => x.Item1.PontosNecessarios)
            .ToList();
    }

    /// <summary>
    /// Próxima conquista a alcançar: a bloqueada de menor pontuação que o jogador ainda não atingiu,
    /// ou null se ele já tem pontos para todas.
    /// </summary>
    public Conquista? ProximaConquista(Jogador jogador) =>
        _repositorio.ListarConquistas()
            .Where(c => !jogador.PossuiConquista(c.IdConquista) && c.PontosNecessarios > jogador.Pontos)
            .OrderBy(c => c.PontosNecessarios)
            .FirstOrDefault();

    /// <summary>
    /// Conquistas que o jogador já tem pontos para ganhar, mas que ainda não foram registradas
    /// (acontece com dados cadastrados direto pela API). São liberadas no próximo quiz.
    /// </summary>
    public int ConquistasPendentes(Jogador jogador) =>
        _repositorio.ListarConquistas()
            .Count(c => !jogador.PossuiConquista(c.IdConquista) && c.PontosNecessarios <= jogador.Pontos);
}
