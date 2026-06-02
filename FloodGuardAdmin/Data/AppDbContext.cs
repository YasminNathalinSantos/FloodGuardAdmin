using Microsoft.EntityFrameworkCore;
using FloodGuardAdmin.Models;

namespace FloodGuardAdmin.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ocorrencia> Ocorrencias { get; set; }
        public DbSet<EquipeResposta> EquipesResposta { get; set; }
        public DbSet<AcaoEmergencial> AcoesEmergenciais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da entidade Ocorrencia
            modelBuilder.Entity<Ocorrencia>().ToTable("TBL_OCORRENCIAS_2TDSPI");
            modelBuilder.Entity<Ocorrencia>().Property(o => o.Id).HasColumnName("ID");
            modelBuilder.Entity<Ocorrencia>().Property(o => o.Titulo).HasColumnName("TITULO");
            modelBuilder.Entity<Ocorrencia>().Property(o => o.Localizacao).HasColumnName("LOCALIZACAO");
            modelBuilder.Entity<Ocorrencia>().Property(o => o.NivelSeveridade).HasColumnName("NIVEL_SEVERIDADE");
            modelBuilder.Entity<Ocorrencia>().Property(o => o.Status).HasColumnName("STATUS");
            modelBuilder.Entity<Ocorrencia>().Property(o => o.DataOcorrencia).HasColumnName("DATA_OCORRENCIA");

            // Relacionamento 1:N — Ocorrencia possui muitas AcoesEmergenciais
            modelBuilder.Entity<Ocorrencia>()
                .HasMany(o => o.AcoesEmergenciais)
                .WithOne(a => a.Ocorrencia)
                .HasForeignKey(a => a.OcorrenciaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao deletar Ocorrencia, deleta as Ações

            // Configuração da entidade EquipeResposta
            modelBuilder.Entity<EquipeResposta>().ToTable("TBL_EQUIPES_RESPOSTA_2TDSPI");
            modelBuilder.Entity<EquipeResposta>().Property(e => e.Id).HasColumnName("ID");
            modelBuilder.Entity<EquipeResposta>().Property(e => e.Nome).HasColumnName("NOME");
            modelBuilder.Entity<EquipeResposta>().Property(e => e.Especialidade).HasColumnName("ESPECIALIDADE");
            modelBuilder.Entity<EquipeResposta>().Property(e => e.CapacidadeMax).HasColumnName("CAPACIDADE_MAX");
            modelBuilder.Entity<EquipeResposta>().Property(e => e.Disponivel).HasColumnName("DISPONIVEL");

            // Relacionamento 1:N — EquipeResposta executa muitas AcoesEmergenciais
            modelBuilder.Entity<EquipeResposta>()
                .HasMany(e => e.AcoesEmergenciais)
                .WithOne(a => a.EquipeResposta)
                .HasForeignKey(a => a.EquipeRespostaId)
                .OnDelete(DeleteBehavior.Restrict); // Não deixa deletar equipe com ações vinculadas

            base.OnModelCreating(modelBuilder);
        }
    }
}
