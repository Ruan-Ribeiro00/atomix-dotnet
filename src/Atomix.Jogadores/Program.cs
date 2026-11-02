using System.Text;
using Atomix.Jogadores.Data;
using Atomix.Jogadores.Exceptions;
using Atomix.Jogadores.Services;
using Atomix.Jogadores.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

IAtomixRepository? repositorio = null;
try
{
    repositorio = RepositoryFactory.Criar(out var avisos);
    foreach (var aviso in avisos)
        Tela.Aviso(aviso);
    Tela.Escrever(ConsoleColor.DarkGray, $"Dados salvos em: {repositorio.Descricao}");

    var conquistas = new ConquistaService(repositorio);
    var jogadores = new JogadorService(repositorio, conquistas);
    new MenuPrincipal(jogadores, conquistas).Executar();
}
catch (EntradaEncerradaException)
{
    Console.WriteLine();
    Tela.Aviso("Entrada encerrada. Saindo do ATOMIX.");
}
catch (PersistenciaException ex)
{
    Tela.Erro($"Não foi possível iniciar o armazenamento de dados: {ex.Message}");
    Environment.ExitCode = 1;
}
catch (Exception ex)
{
    Tela.Erro($"Erro inesperado ao iniciar o ATOMIX: {ex.Message}");
    Environment.ExitCode = 1;
}
finally
{
    repositorio?.Dispose();
}
