using Atomix.Jogadores.Models;
using Microsoft.EntityFrameworkCore;

namespace Atomix.Jogadores.Data;

/// <summary>
/// Mapeamento EF Core para as tabelas TB_ATX_JOGADOR, TB_ATX_CONQUISTA e TB_ATX_JOGADOR_CONQUISTA,
/// do modelo do projeto de Banco de Dados (scripts/oracle_tabelas_atomix.sql). Os IDs vêm das
/// sequences SQ_ATX_JOGADOR e SQ_ATX_CONQUISTA (ver OracleRepository).
/// </summary>
public class AtomixDbContext : DbContext
{
    private readonly string _connectionString;

    public AtomixDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Jogador> Jogadores => Set<Jogador>();
    public DbSet<Conquista> Conquistas => Set<Conquista>();
    public DbSet<JogadorConquista> JogadorConquistas => Set<JogadorConquista>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseOracle(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jogador>(e =>
        {
            e.ToTable("TB_ATX_JOGADOR");
            e.HasKey(j => j.IdJogador);
            e.Property(j => j.IdJogador).HasColumnName("ID_JOGADOR").ValueGeneratedNever();
            e.Property(j => j.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
            e.Property(j => j.Email).HasColumnName("EMAIL").HasMaxLength(120).IsRequired();
            e.Property(j => j.SenhaHash).HasColumnName("SENHA").HasMaxLength(100).IsRequired();
            e.Property(j => j.Pontos).HasColumnName("PONTUACAO").IsRequired();
            e.Property(j => j.DataCadastro).HasColumnName("DT_CADASTRO").IsRequired();
            e.HasIndex(j => j.Email).IsUnique();
            e.Ignore(j => j.Nivel);
        });

        modelBuilder.Entity<Conquista>(e =>
        {
            e.ToTable("TB_ATX_CONQUISTA");
            e.HasKey(c => c.IdConquista);
            e.Property(c => c.IdConquista).HasColumnName("ID_CONQUISTA").ValueGeneratedNever();
            e.Property(c => c.IdArea).HasColumnName("ID_AREA");
            e.Property(c => c.Nome).HasColumnName("NOME").HasMaxLength(60).IsRequired();
            e.Property(c => c.Descricao).HasColumnName("DESCRICAO").HasMaxLength(300).IsRequired();
            e.Property(c => c.PontosNecessarios).HasColumnName("PONTOS_NECESSARIOS").IsRequired();
        });

        modelBuilder.Entity<JogadorConquista>(e =>
        {
            e.ToTable("TB_ATX_JOGADOR_CONQUISTA");
            e.HasKey(jc => new { jc.IdJogador, jc.IdConquista });
            e.Property(jc => jc.IdJogador).HasColumnName("ID_JOGADOR");
            e.Property(jc => jc.IdConquista).HasColumnName("ID_CONQUISTA");
            e.Property(jc => jc.DataDesbloqueio).HasColumnName("DT_DESBLOQUEIO").IsRequired();

            e.HasOne(jc => jc.Jogador)
                .WithMany(j => j.Conquistas)
                .HasForeignKey(jc => jc.IdJogador)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(jc => jc.Conquista)
                .WithMany()
                .HasForeignKey(jc => jc.IdConquista)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
