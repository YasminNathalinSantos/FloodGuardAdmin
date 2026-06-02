using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodGuardAdmin.Models
{
    [Table("TBL_EQUIPES_RESPOSTA_2TDSPI")]
    public class EquipeResposta
    {
        // Encapsulamento
        [Key]
        [Column("ID")]
        public int Id { get; private set; }

        [Required(ErrorMessage = "O nome da equipe é obrigatório")]
        [MaxLength(150)]
        [Column("NOME")]
        public string Nome { get; private set; }

        [Required(ErrorMessage = "A especialidade é obrigatória")]
        [MaxLength(100)]
        [Column("ESPECIALIDADE")]
        public string Especialidade { get; private set; }  // Ex: Resgate, Médica, Infraestrutura

        [Column("CAPACIDADE_MAX")]
        public int CapacidadeMax { get; private set; }

        [Column("DISPONIVEL")]
        public bool Disponivel { get; private set; }

        // Relacionamento 1:N — uma EquipeResposta pode executar várias AcoesEmergenciais
        public ICollection<AcaoEmergencial>? AcoesEmergenciais { get; set; }

        // Construtor padrão: Necessário para o Entity Framework criar instâncias do objeto
        protected EquipeResposta() { }

        // Construtor rico: Garante que o objeto vá possuir um estado inicial válido
        public EquipeResposta(string nome, string especialidade, int capacidadeMax)
        {
            if (capacidadeMax <= 0)
                throw new ArgumentException("A capacidade máxima deve ser maior que zero.");

            Nome = nome;
            Especialidade = especialidade;
            CapacidadeMax = capacidadeMax;
            Disponivel = true;
        }

        // Método de regra de negócio para alternar disponibilidade
        public void AlternarDisponibilidade()
        {
            Disponivel = !Disponivel;
        }
    }
}
