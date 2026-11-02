namespace Atomix.Jogadores.Models;

/// <summary>
/// Nível do jogador, calculado a partir da pontuação (não é armazenado no banco).
/// Mesma escala da function FN_ATX_RANKING_JOGADORES do projeto de Banco de Dados.
/// </summary>
public enum NivelJogador
{
    AprendizAtomico = 1,
    ExploradorNuclear = 2,
    CientistaDoAtomo = 3,
    EspecialistaCtmsp = 4,
    MestreNuclear = 5
}

public static class NivelJogadorRegras
{
    public const int PontosExploradorNuclear = 100;
    public const int PontosCientistaDoAtomo = 300;
    public const int PontosEspecialistaCtmsp = 600;
    public const int PontosMestreNuclear = 1000;

    public static NivelJogador Calcular(int pontos) => pontos switch
    {
        >= PontosMestreNuclear => NivelJogador.MestreNuclear,
        >= PontosEspecialistaCtmsp => NivelJogador.EspecialistaCtmsp,
        >= PontosCientistaDoAtomo => NivelJogador.CientistaDoAtomo,
        >= PontosExploradorNuclear => NivelJogador.ExploradorNuclear,
        _ => NivelJogador.AprendizAtomico
    };

    public static string Nome(this NivelJogador nivel) => nivel switch
    {
        NivelJogador.AprendizAtomico => "Aprendiz Atômico",
        NivelJogador.ExploradorNuclear => "Explorador Nuclear",
        NivelJogador.CientistaDoAtomo => "Cientista do Átomo",
        NivelJogador.EspecialistaCtmsp => "Especialista CTMSP",
        NivelJogador.MestreNuclear => "Mestre Nuclear",
        _ => nivel.ToString()
    };
}
