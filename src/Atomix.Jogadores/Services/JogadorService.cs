using System.Globalization;
using Atomix.Jogadores.Data;
using Atomix.Jogadores.Exceptions;
using Atomix.Jogadores.Models;

namespace Atomix.Jogadores.Services;

/// <summary>Resultado de registrar um quiz concluído no app.</summary>
public record ResultadoQuiz(
    Jogador Jogador,
    AreaNuclear Area,
    int Acertos,
    int PontosGanhos,
    bool GanhouBonus,
    NivelJogador NivelAnterior,
    List<Conquista> NovasConquistas)
{
    public bool SubiuDeNivel => Jogador.Nivel != NivelAnterior;
}

/// <summary>
/// Casos de uso de Jogador: cadastro, listagem, buscas, pontuação, edição e remoção.
/// </summary>
public class JogadorService
{
    // Regras de pontuação do quiz (cada área do app tem um quiz de 5 perguntas)
    public const int PerguntasPorQuiz = 5;
    public const int PontosPorAcerto = 10;
    public const int BonusQuizPerfeito = 20;

    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;
    private const CompareOptions IgnorarCaixaEAcento = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private readonly IAtomixRepository _repositorio;
    private readonly ConquistaService _conquistas;

    public JogadorService(IAtomixRepository repositorio, ConquistaService conquistas)
    {
        _repositorio = repositorio;
        _conquistas = conquistas;
    }

    // ---------- Cadastro ----------

    public Jogador Cadastrar(string nome, string email, string senha)
    {
        nome = JogadorValidator.NormalizarNome(nome);
        email = JogadorValidator.NormalizarEmail(email);
        JogadorValidator.Validar(nome, email, senha, validarSenha: true);
        GarantirEmailDisponivel(email, idIgnorado: null);

        var jogador = new Jogador
        {
            Nome = nome,
            Email = email,
            // Mesmo algoritmo da API Java (BCrypt): só o hash é gravado
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
            Pontos = 0,
            DataCadastro = DateTime.Now
        };
        _repositorio.AdicionarJogador(jogador);

        // Conquistas que não exigem pontos (ex.: "Primeiro Átomo") já são liberadas no cadastro
        if (_conquistas.AplicarDesbloqueios(jogador, jogador.DataCadastro).Count > 0)
            _repositorio.AtualizarJogador(jogador);
        return jogador;
    }

    // ---------- Listagem e buscas ----------

    public IReadOnlyList<Jogador> Listar() => _repositorio.ListarJogadores();

    public Jogador ObterPorId(int idJogador) =>
        _repositorio.ObterJogador(idJogador)
        ?? throw new RegraNegocioException($"Nenhum jogador encontrado com o ID {idJogador}.");

    /// <summary>Busca por parte do nome, sem diferenciar maiúsculas nem acentos ("joao" encontra "João").</summary>
    public IReadOnlyList<Jogador> BuscarPorNome(string termo)
    {
        termo = JogadorValidator.NormalizarNome(termo);
        if (termo.Length < 2)
            throw new ValidacaoException("Digite pelo menos 2 caracteres para buscar por nome.");

        return Listar().Where(j => Comparador.IndexOf(j.Nome, termo, IgnorarCaixaEAcento) >= 0).ToList();
    }

    public IReadOnlyList<Jogador> BuscarPorEmail(string termo)
    {
        termo = JogadorValidator.NormalizarEmail(termo);
        if (termo.Length < 2)
            throw new ValidacaoException("Digite pelo menos 2 caracteres para buscar por e-mail.");

        return Listar().Where(j => j.Email.Contains(termo, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public IReadOnlyList<Jogador> BuscarPorNivel(NivelJogador nivel) =>
        Listar().Where(j => j.Nivel == nivel).ToList();

    public IReadOnlyList<Jogador> BuscarPorPontuacaoMinima(int pontosMinimos)
    {
        if (pontosMinimos < 0)
            throw new ValidacaoException("A pontuação mínima não pode ser negativa.");

        return Listar().Where(j => j.Pontos >= pontosMinimos).OrderByDescending(j => j.Pontos).ToList();
    }

    /// <summary>Ranking: mais pontos primeiro; empate → quem tem mais conquistas; depois quem se cadastrou antes.</summary>
    public IReadOnlyList<Jogador> Ranking(int quantidade = 10) =>
        Listar()
            .OrderByDescending(j => j.Pontos)
            .ThenByDescending(j => j.Conquistas.Count)
            .ThenBy(j => j.DataCadastro)
            .Take(quantidade)
            .ToList();

    // ---------- Regra de negócio: pontuação e desbloqueio ----------

    /// <summary>
    /// Registra um quiz concluído numa área da cidade:
    /// 10 pontos por acerto, +20 de bônus se acertar as 5 perguntas,
    /// e desbloqueia automaticamente as conquistas cuja pontuação foi alcançada.
    /// </summary>
    public ResultadoQuiz RegistrarQuiz(int idJogador, AreaNuclear area, int acertos)
    {
        if (!Enum.IsDefined(area))
            throw new ValidacaoException("Área inválida.");
        if (acertos < 0 || acertos > PerguntasPorQuiz)
            throw new ValidacaoException($"O número de acertos deve estar entre 0 e {PerguntasPorQuiz}.");

        var jogador = ObterPorId(idJogador);
        var nivelAnterior = jogador.Nivel;

        var ganhouBonus = acertos == PerguntasPorQuiz;
        var pontosGanhos = acertos * PontosPorAcerto + (ganhouBonus ? BonusQuizPerfeito : 0);

        checked { jogador.Pontos += pontosGanhos; }
        var novas = _conquistas.AplicarDesbloqueios(jogador, DateTime.Now);

        _repositorio.AtualizarJogador(jogador);
        return new ResultadoQuiz(jogador, area, acertos, pontosGanhos, ganhouBonus, nivelAnterior, novas);
    }

    // ---------- Edição e remoção ----------

    /// <summary>Atualiza nome e/ou e-mail. Valor nulo ou vazio mantém o atual.</summary>
    public Jogador Editar(int idJogador, string? novoNome, string? novoEmail)
    {
        var jogador = ObterPorId(idJogador);

        var nome = string.IsNullOrWhiteSpace(novoNome) ? jogador.Nome : JogadorValidator.NormalizarNome(novoNome);
        var email = string.IsNullOrWhiteSpace(novoEmail) ? jogador.Email : JogadorValidator.NormalizarEmail(novoEmail);

        JogadorValidator.Validar(nome, email);
        GarantirEmailDisponivel(email, idIgnorado: idJogador);

        jogador.Nome = nome;
        jogador.Email = email;
        _repositorio.AtualizarJogador(jogador);
        return jogador;
    }

    public void Remover(int idJogador)
    {
        ObterPorId(idJogador); // lança se não existir
        _repositorio.RemoverJogador(idJogador);
    }

    private void GarantirEmailDisponivel(string email, int? idIgnorado)
    {
        var emUso = _repositorio.ListarJogadores()
            .Any(j => j.IdJogador != idIgnorado && string.Equals(j.Email, email, StringComparison.OrdinalIgnoreCase));

        if (emUso)
            throw new RegraNegocioException($"Já existe um jogador cadastrado com o e-mail {email}.");
    }
}
