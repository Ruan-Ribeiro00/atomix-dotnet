using Atomix.Jogadores.Models;

namespace Atomix.Jogadores.Data;

/// <summary>
/// Catálogo padrão de conquistas do modo JSON: o mesmo cadastrado no banco Oracle pelo projeto
/// de Banco de Dados (coleção Postman) e usado no app Flutter. IdArea nulo = conquista geral.
/// </summary>
public static class DadosIniciais
{
    public static List<Conquista> Conquistas() => new()
    {
        new() { IdConquista = 1,  IdArea = null, Nome = "Primeiro Átomo",          Descricao = "Criou a conta e começou a explorar a cidade nuclear.",              PontosNecessarios = 0 },
        new() { IdConquista = 2,  IdArea = 1,    Nome = "Explorador da Saúde",     Descricao = "Descobriu como a radiação salva vidas na medicina.",                 PontosNecessarios = 50 },
        new() { IdConquista = 3,  IdArea = 2,    Nome = "Guardião do Agro",        Descricao = "Aprendeu como a tecnologia nuclear protege as lavouras.",            PontosNecessarios = 100 },
        new() { IdConquista = 4,  IdArea = 3,    Nome = "Engenheiro da Indústria", Descricao = "Dominou as aplicações nucleares na indústria.",                      PontosNecessarios = 150 },
        new() { IdConquista = 5,  IdArea = 4,    Nome = "Energia Limpa",           Descricao = "Entendeu como a energia nuclear gera eletricidade com baixa emissão.", PontosNecessarios = 200 },
        new() { IdConquista = 6,  IdArea = 5,    Nome = "Mente Científica",        Descricao = "Explorou os reatores de pesquisa e a datação de materiais.",         PontosNecessarios = 300 },
        new() { IdConquista = 7,  IdArea = null, Nome = "Mestre do Quiz",          Descricao = "Acertou perguntas de todas as áreas da cidade nuclear.",             PontosNecessarios = 400 },
        new() { IdConquista = 8,  IdArea = null, Nome = "Cidadão Nuclear",         Descricao = "Conhece as principais aplicações nucleares do dia a dia.",           PontosNecessarios = 500 },
        new() { IdConquista = 9,  IdArea = 9,    Nome = "Submarinista",            Descricao = "Conheceu o Programa Nuclear da Marinha e a propulsão naval.",        PontosNecessarios = 750 },
        new() { IdConquista = 10, IdArea = null, Nome = "Mestre Nuclear",          Descricao = "Alcançou o nível máximo do ATOMIX.",                                 PontosNecessarios = 1000 },
    };
}
