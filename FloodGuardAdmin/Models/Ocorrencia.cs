using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodGuardAdmin.Models
{
    [Table("TBL_OCORRENCIAS_2TDSPI")]
    public class Ocorrencia
    {
        // Encapsulamento
        [Key]
        [Column("ID")]
        public int Id { get; private set; }

        [Required(ErrorMessage = "O título da ocorrência é obrigatório")]
        [MaxLength(200)]
        [Column("TITULO")]
        public string Titulo { get; private set; }

        [Required(ErrorMessage = "A localização é obrigatória")]
        [MaxLength(300)]
        [Column("LOCALIZACAO")]
        public string Localizacao { get; private set; }

        [Required(ErrorMessage = "O nível de severidade é obrigatório")]
        [Column("NIVEL_SEVERIDADE")]
        public int NivelSeveridade { get; private set; }  // 1=Baixo, 2=Médio, 3=Alto, 4=Crítico

        [Column("STATUS")]
        [MaxLength(50)]
        public string Status { get; private set; }  // Aberta, EmAtendimento, Encerrada

        [Column("DATA_OCORRENCIA")]
        public DateTime DataOcorrencia { get; private set; }

        // Relacionamento 1:N — uma Ocorrência possui várias AcoesEmergenciais
        public ICollection<AcaoEmergencial>? AcoesEmergenciais { get; set; }

        // Construtor padrão: Necessário para o Entity Framework criar instâncias do objeto
        protected Ocorrencia() { }

        // Construtor rico: Garante que o objeto vá possuir um estado inicial válido
        public Ocorrencia(string titulo, string localizacao, int nivelSeveridade)
        {
            if (nivelSeveridade < 1 || nivelSeveridade > 4)
                throw new ArgumentException("O nível de severidade deve estar entre 1 e 4.");

            Titulo = titulo;
            Localizacao = localizacao;
            NivelSeveridade = nivelSeveridade;
            Status = "Aberta";
            DataOcorrencia = DateTime.UtcNow;
        }

        // Método de regra de negócio para atualizar o status da ocorrência
        public void AtualizarStatus(string novoStatus)
        {
            var statusValidos = new[] { "Aberta", "EmAtendimento", "Encerrada" };
            if (!statusValidos.Contains(novoStatus))
                throw new ArgumentException($"Status inválido. Use: {string.Join(", ", statusValidos)}");

            Status = novoStatus;
        }

        // Método de regra de negócio para escalar a severidade
        public void EscalarSeveridade()
        {
            if (NivelSeveridade >= 4)
                throw new InvalidOperationException("A ocorrência já está no nível crítico.");

            NivelSeveridade++;
        }
    }
}
