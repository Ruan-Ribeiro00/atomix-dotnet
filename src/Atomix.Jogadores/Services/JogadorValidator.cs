using System.Text.RegularExpressions;
using Atomix.Jogadores.Exceptions;

namespace Atomix.Jogadores.Services;

/// <summary>
/// Validações de formato dos dados do jogador. Junta todos os erros antes de lançar a exceção,
/// para o usuário corrigir tudo de uma vez.
/// </summary>
public static partial class JogadorValidator
{
    public const int NomeMinimo = 3;
    public const int NomeMaximo = 80;
    public const int EmailMaximo = 120;
    public const int SenhaMinima = 6;
    public const int SenhaMaxima = 50;

    // Letras (com acento), espaços, apóstrofo e hífen — ex.: "Ana D'Ávila", "João-Pedro"
    [GeneratedRegex(@"^[\p{L}][\p{L} '\-]*$")]
    private static partial Regex NomeRegex();

    [GeneratedRegex(@"^[a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,}$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRegex();

    public static string NormalizarNome(string? nome) => EspacosRegex().Replace((nome ?? string.Empty).Trim(), " ");

    public static string NormalizarEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    public static List<string> ErrosNome(string nome)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(nome))
            erros.Add("O nome é obrigatório.");
        else if (nome.Length < NomeMinimo || nome.Length > NomeMaximo)
            erros.Add($"O nome deve ter entre {NomeMinimo} e {NomeMaximo} caracteres.");
        else if (!NomeRegex().IsMatch(nome))
            erros.Add("O nome deve conter apenas letras, espaços, hífen ou apóstrofo.");
        return erros;
    }

    public static List<string> ErrosEmail(string email)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(email))
            erros.Add("O e-mail é obrigatório.");
        else if (email.Length > EmailMaximo)
            erros.Add($"O e-mail deve ter no máximo {EmailMaximo} caracteres.");
        else if (!EmailRegex().IsMatch(email))
            erros.Add("O e-mail informado não é válido (ex.: nome@dominio.com).");
        return erros;
    }

    public static List<string> ErrosSenha(string? senha)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(senha))
            erros.Add("A senha é obrigatória.");
        else if (senha.Length < SenhaMinima || senha.Length > SenhaMaxima)
            erros.Add($"A senha deve ter entre {SenhaMinima} e {SenhaMaxima} caracteres.");
        else if (!senha.Any(char.IsLetter) || !senha.Any(char.IsDigit))
            erros.Add("A senha deve ter pelo menos uma letra e um número.");
        return erros;
    }

    public static void Validar(string nome, string email, string? senha = null, bool validarSenha = false)
    {
        var erros = ErrosNome(nome);
        erros.AddRange(ErrosEmail(email));
        if (validarSenha)
            erros.AddRange(ErrosSenha(senha));
        if (erros.Count > 0)
            throw new ValidacaoException(erros);
    }
}
