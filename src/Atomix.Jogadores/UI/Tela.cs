using Atomix.Jogadores.Exceptions;

namespace Atomix.Jogadores.UI;

/// <summary>
/// Helpers de entrada e saída no console. Toda leitura numérica é feita com TryParse,
/// então digitar texto onde se espera número nunca derruba o programa.
/// </summary>
public static class Tela
{
    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Escrever(ConsoleColor.Cyan, $"=== {texto.ToUpperInvariant()} ===");
    }

    public static void Sucesso(string mensagem) => Escrever(ConsoleColor.Green, $"✔ {mensagem}");
    public static void Erro(string mensagem) => Escrever(ConsoleColor.Red, $"✖ {mensagem}");
    public static void Aviso(string mensagem) => Escrever(ConsoleColor.Yellow, $"! {mensagem}");
    public static void Destaque(string mensagem) => Escrever(ConsoleColor.Magenta, mensagem);

    public static void Escrever(ConsoleColor cor, string mensagem)
    {
        var anterior = Console.ForegroundColor;
        Console.ForegroundColor = cor;
        Console.WriteLine(mensagem);
        Console.ForegroundColor = anterior;
    }

    /// <summary>Lê uma linha. Lança <see cref="EntradaEncerradaException"/> se a entrada acabou (Ctrl+D/Ctrl+Z).</summary>
    public static string LerLinha(string rotulo)
    {
        Console.Write($"{rotulo}: ");
        var linha = Console.ReadLine();
        if (linha is null)
            throw new EntradaEncerradaException();
        return linha.Trim();
    }

    /// <summary>Lê a senha sem mostrar os caracteres (mostra *). Funciona também com entrada redirecionada.</summary>
    public static string LerSenha(string rotulo)
    {
        Console.Write($"{rotulo}: ");
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? string.Empty;

        var senha = new System.Text.StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter) break;
            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (senha.Length == 0) continue;
                senha.Length--;
                Console.Write("\b \b");
                continue;
            }
            if (char.IsControl(tecla.KeyChar)) continue;
            senha.Append(tecla.KeyChar);
            Console.Write('*');
        }
        Console.WriteLine();
        return senha.ToString();
    }

    public static string LerTextoObrigatorio(string rotulo)
    {
        while (true)
        {
            var texto = LerLinha(rotulo);
            if (!string.IsNullOrWhiteSpace(texto))
                return texto;
            Aviso("Este campo é obrigatório.");
        }
    }

    public static int LerInteiro(string rotulo, int minimo, int maximo)
    {
        while (true)
        {
            var texto = LerLinha($"{rotulo} ({minimo}-{maximo})");
            if (int.TryParse(texto, out var valor) && valor >= minimo && valor <= maximo)
                return valor;
            Aviso($"Digite um número inteiro entre {minimo} e {maximo}.");
        }
    }

    public static int LerId(string rotulo = "ID do jogador")
    {
        while (true)
        {
            var texto = LerLinha(rotulo);
            if (int.TryParse(texto, out var id) && id > 0)
                return id;
            Aviso("Digite um ID numérico maior que zero.");
        }
    }

    public static bool Confirmar(string pergunta)
    {
        while (true)
        {
            var resposta = LerLinha($"{pergunta} (s/n)").ToLowerInvariant();
            if (resposta is "s" or "sim") return true;
            if (resposta is "n" or "nao" or "não") return false;
            Aviso("Responda com s ou n.");
        }
    }

    public static void Pausar()
    {
        Console.WriteLine();
        Console.Write("Pressione ENTER para voltar ao menu...");
        if (Console.ReadLine() is null)
            throw new EntradaEncerradaException();
    }

    /// <summary>Corta o texto para caber na coluna da tabela.</summary>
    public static string Ajustar(string texto, int largura) =>
        texto.Length <= largura ? texto.PadRight(largura) : texto[..(largura - 1)] + "…";
}
