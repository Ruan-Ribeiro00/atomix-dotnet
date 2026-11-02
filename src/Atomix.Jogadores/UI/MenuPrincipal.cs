using Atomix.Jogadores.Exceptions;
using Atomix.Jogadores.Models;
using Atomix.Jogadores.Services;

namespace Atomix.Jogadores.UI;

/// <summary>
/// Menu do console. Cada opção roda dentro de try/catch: qualquer erro é mostrado
/// ao usuário e o programa volta para o menu, sem encerrar.
/// </summary>
public class MenuPrincipal
{
    private readonly JogadorService _jogadores;
    private readonly ConquistaService _conquistas;

    public MenuPrincipal(JogadorService jogadores, ConquistaService conquistas)
    {
        _jogadores = jogadores;
        _conquistas = conquistas;
    }

    public void Executar()
    {
        while (true)
        {
            MostrarOpcoes();
            var opcao = Tela.LerLinha("Escolha uma opção");

            if (opcao == "0")
            {
                Tela.Sucesso("Até a próxima missão na cidade nuclear!");
                return;
            }

            try
            {
                switch (opcao)
                {
                    case "1": Cadastrar(); break;
                    case "2": Listar(); break;
                    case "3": Buscar(); break;
                    case "4": RegistrarQuiz(); break;
                    case "5": VerConquistasDoJogador(); break;
                    case "6": VerCatalogoConquistas(); break;
                    case "7": VerRanking(); break;
                    case "8": Editar(); break;
                    case "9": Remover(); break;
                    default:
                        Tela.Aviso("Opção inválida. Digite um número de 0 a 9.");
                        continue;
                }
            }
            catch (ValidacaoException ex)
            {
                Tela.Erro("Dados inválidos:");
                foreach (var erro in ex.Erros)
                    Tela.Erro($"  - {erro}");
            }
            catch (RegraNegocioException ex)
            {
                Tela.Erro(ex.Message);
            }
            catch (PersistenciaException ex)
            {
                Tela.Erro($"Problema ao acessar os dados: {ex.Message}");
            }
            catch (EntradaEncerradaException)
            {
                throw; // tratado no Program.cs (encerra de forma limpa)
            }
            catch (Exception ex)
            {
                Tela.Erro($"Erro inesperado: {ex.Message}");
                Tela.Aviso("A operação foi cancelada, mas o sistema continua funcionando.");
            }

            Tela.Pausar();
        }
    }

    private static void MostrarOpcoes()
    {
        Console.WriteLine();
        Tela.Escrever(ConsoleColor.Cyan, "╔══════════════════════════════════════════╗");
        Tela.Escrever(ConsoleColor.Cyan, "║   ATOMIX ⚛  Jogadores e Conquistas       ║");
        Tela.Escrever(ConsoleColor.Cyan, "╚══════════════════════════════════════════╝");
        Console.WriteLine("  1. Cadastrar jogador");
        Console.WriteLine("  2. Listar jogadores");
        Console.WriteLine("  3. Buscar jogadores");
        Console.WriteLine("  4. Registrar resultado de quiz (ganhar pontos)");
        Console.WriteLine("  5. Ver conquistas de um jogador");
        Console.WriteLine("  6. Catálogo de conquistas");
        Console.WriteLine("  7. Ranking");
        Console.WriteLine("  8. Editar jogador");
        Console.WriteLine("  9. Remover jogador");
        Console.WriteLine("  0. Sair");
    }

    // 1
    private void Cadastrar()
    {
        Tela.Titulo("Cadastrar jogador");
        var nome = Tela.LerLinha("Nome");
        var email = Tela.LerLinha("E-mail");
        var senha = Tela.LerSenha("Senha (6 a 50 caracteres, com letra e número)");

        var jogador = _jogadores.Cadastrar(nome, email, senha);
        Tela.Sucesso($"Jogador \"{jogador.Nome}\" cadastrado com o ID {jogador.IdJogador}. Bem-vindo à cidade nuclear!");
        foreach (var (conquista, _) in _conquistas.ConquistasDoJogador(jogador))
            Tela.Destaque($"🏆 Conquista desbloqueada: {conquista.Nome} — {conquista.Descricao}");
    }

    // 2
    private void Listar()
    {
        Tela.Titulo("Jogadores cadastrados");
        ImprimirTabela(_jogadores.Listar());
    }

    // 3
    private void Buscar()
    {
        Tela.Titulo("Buscar jogadores");
        Console.WriteLine("  1. Por nome (parte do nome, ignora acentos)");
        Console.WriteLine("  2. Por e-mail");
        Console.WriteLine("  3. Por nível");
        Console.WriteLine("  4. Por pontuação mínima");
        var criterio = Tela.LerInteiro("Critério", 1, 4);

        IReadOnlyList<Jogador> resultado;
        switch (criterio)
        {
            case 1:
                resultado = _jogadores.BuscarPorNome(Tela.LerTextoObrigatorio("Nome ou parte do nome"));
                break;
            case 2:
                resultado = _jogadores.BuscarPorEmail(Tela.LerTextoObrigatorio("E-mail ou parte do e-mail"));
                break;
            case 3:
                foreach (var nivel in Enum.GetValues<NivelJogador>())
                    Console.WriteLine($"  {(int)nivel}. {nivel.Nome()}");
                resultado = _jogadores.BuscarPorNivel((NivelJogador)Tela.LerInteiro("Nível", 1, Enum.GetValues<NivelJogador>().Length));
                break;
            default:
                resultado = _jogadores.BuscarPorPontuacaoMinima(Tela.LerInteiro("Pontuação mínima", 0, 1_000_000));
                break;
        }

        Console.WriteLine();
        Console.WriteLine($"{resultado.Count} jogador(es) encontrado(s).");
        ImprimirTabela(resultado);
    }

    // 4
    private void RegistrarQuiz()
    {
        Tela.Titulo("Registrar resultado de quiz");
        var id = Tela.LerId();
        var jogador = _jogadores.ObterPorId(id);
        Console.WriteLine($"Jogador: {jogador.Nome} — {jogador.Pontos} pts ({jogador.Nivel.Nome()})");

        Console.WriteLine("Área da cidade:");
        foreach (var area in Enum.GetValues<AreaNuclear>())
            Console.WriteLine($"  {(int)area}. {area.Nome()}");
        var areaEscolhida = (AreaNuclear)Tela.LerInteiro("Área", 1, 5);

        var acertos = Tela.LerInteiro($"Acertos no quiz de {JogadorService.PerguntasPorQuiz} perguntas", 0, JogadorService.PerguntasPorQuiz);

        var resultado = _jogadores.RegistrarQuiz(id, areaEscolhida, acertos);

        Console.WriteLine();
        Tela.Sucesso($"Quiz de {resultado.Area.Nome()}: {resultado.Acertos}/{JogadorService.PerguntasPorQuiz} acertos → +{resultado.PontosGanhos} pontos" +
                     (resultado.GanhouBonus ? $" (inclui bônus de quiz perfeito +{JogadorService.BonusQuizPerfeito})" : ""));
        Console.WriteLine($"Pontuação total: {resultado.Jogador.Pontos} pts");

        if (resultado.SubiuDeNivel)
            Tela.Destaque($"⬆ Subiu de nível: {resultado.NivelAnterior.Nome()} → {resultado.Jogador.Nivel.Nome()}!");

        foreach (var conquista in resultado.NovasConquistas)
            Tela.Destaque($"🏆 Conquista desbloqueada: {conquista.Nome} — {conquista.Descricao}");

        MostrarProximaConquista(resultado.Jogador);
    }

    // 5
    private void VerConquistasDoJogador()
    {
        Tela.Titulo("Conquistas do jogador");
        var jogador = _jogadores.ObterPorId(Tela.LerId());
        Console.WriteLine($"{jogador.Nome} — {jogador.Pontos} pts — nível {jogador.Nivel.Nome()}");
        Console.WriteLine();

        var conquistas = _conquistas.ConquistasDoJogador(jogador);
        if (conquistas.Count == 0)
            Tela.Aviso("Nenhuma conquista desbloqueada ainda. Responda quizzes para ganhar pontos!");
        else
            foreach (var (conquista, data) in conquistas)
                Console.WriteLine($"  🏆 {conquista.Nome,-24} desbloqueada em {data:dd/MM/yyyy HH:mm}");

        MostrarProximaConquista(jogador);
    }

    // 6
    private void VerCatalogoConquistas()
    {
        Tela.Titulo("Catálogo de conquistas");
        Console.WriteLine($"{"ID",-4}{"Conquista",-25}{"Pontos",-8}Descrição");
        foreach (var c in _conquistas.ListarCatalogo())
            Console.WriteLine($"{c.IdConquista,-4}{Tela.Ajustar(c.Nome, 24)} {c.PontosNecessarios,-8}{c.Descricao}");
    }

    // 7
    private void VerRanking()
    {
        Tela.Titulo("Ranking — top 10");
        var ranking = _jogadores.Ranking(10);
        if (ranking.Count == 0)
        {
            Tela.Aviso("Nenhum jogador cadastrado.");
            return;
        }

        var posicao = 1;
        foreach (var j in ranking)
        {
            var medalha = posicao switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => "  " };
            Console.WriteLine($"{medalha} {posicao,2}º  {Tela.Ajustar(j.Nome, 28)} {j.Pontos,6} pts  {j.Nivel.Nome(),-19} {j.Conquistas.Count} conquista(s)");
            posicao++;
        }
    }

    // 8
    private void Editar()
    {
        Tela.Titulo("Editar jogador");
        var jogador = _jogadores.ObterPorId(Tela.LerId());
        Console.WriteLine($"Editando: {jogador.Nome} <{jogador.Email}>");
        Console.WriteLine("Deixe em branco para manter o valor atual.");

        var nome = Tela.LerLinha($"Novo nome [{jogador.Nome}]");
        var email = Tela.LerLinha($"Novo e-mail [{jogador.Email}]");

        var atualizado = _jogadores.Editar(jogador.IdJogador, nome, email);
        Tela.Sucesso($"Jogador {atualizado.IdJogador} atualizado: {atualizado.Nome} <{atualizado.Email}>");
    }

    // 9
    private void Remover()
    {
        Tela.Titulo("Remover jogador");
        var jogador = _jogadores.ObterPorId(Tela.LerId());
        Console.WriteLine($"{jogador.Nome} <{jogador.Email}> — {jogador.Pontos} pts, {jogador.Conquistas.Count} conquista(s)");

        if (!Tela.Confirmar("Tem certeza que deseja remover? As conquistas dele também serão apagadas"))
        {
            Tela.Aviso("Remoção cancelada.");
            return;
        }

        _jogadores.Remover(jogador.IdJogador);
        Tela.Sucesso("Jogador removido.");
    }

    private void MostrarProximaConquista(Jogador jogador)
    {
        var pendentes = _conquistas.ConquistasPendentes(jogador);
        if (pendentes > 0)
            Tela.Aviso($"{pendentes} conquista(s) já alcançada(s) pela pontuação serão liberadas no próximo quiz.");

        var proxima = _conquistas.ProximaConquista(jogador);
        if (proxima is not null)
            Console.WriteLine($"Próxima conquista: {proxima.Nome} — faltam {proxima.PontosNecessarios - jogador.Pontos} pts.");
        else if (pendentes == 0)
            Tela.Destaque("Todas as conquistas desbloqueadas. Lenda da cidade nuclear!");
    }

    private static void ImprimirTabela(IReadOnlyList<Jogador> jogadores)
    {
        if (jogadores.Count == 0)
        {
            Tela.Aviso("Nenhum jogador para mostrar.");
            return;
        }

        Console.WriteLine($"{"ID",-5}{"Nome",-26}{"E-mail",-30}{"Pontos",7}  {"Nível",-19}{"Conq.",5}  {"Cadastro",-10}");
        Console.WriteLine(new string('-', 102));
        foreach (var j in jogadores)
        {
            Console.WriteLine($"{j.IdJogador,-5}{Tela.Ajustar(j.Nome, 25)} {Tela.Ajustar(j.Email, 29)} {j.Pontos,7}  {j.Nivel.Nome(),-19}{j.Conquistas.Count,5}  {j.DataCadastro:dd/MM/yyyy}");
        }
    }
}
