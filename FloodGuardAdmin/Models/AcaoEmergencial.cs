using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FloodGuardAdmin.Models
{
    [Table("TBL_ACOES_EMERGENCIAIS_2TDSPI")]
    public class AcaoEmergencial
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição da ação é obrigatória")]
        [MaxLength(500)]
        [Column("DESCRICAO")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O tipo de ação é obrigatório")]
        [MaxLength(100)]
        [Column("TIPO_ACAO")]
        public string TipoAcao { get; set; }  // Ex: Evacuacao, Socorro, Contencao

        [Column("DATA_INICIO")]
        public DateTime DataInicio { get; set; }

        [Column("DATA_FIM")]
        public DateTime? DataFim { get; set; }

        [Column("STATUS_ACAO")]
        [MaxLength(50)]
        public string StatusAcao { get; set; }  // Planejada, EmExecucao, Concluida, Cancelada

        // Relacionamento: uma AcaoEmergencial pertence a uma Ocorrencia
        [Required]
        [Column("OCORRENCIA_ID")]
        public int OcorrenciaId { get; set; }

        [JsonIgnore] // Evita referência circular durante a serialização JSON
        [ForeignKey("OcorrenciaId")]
        public Ocorrencia? Ocorrencia { get; set; }

        // Relacionamento: uma AcaoEmergencial é executada por uma EquipeResposta
        [Required]
        [Column("EQUIPE_RESPOSTA_ID")]
        public int EquipeRespostaId { get; set; }

        [JsonIgnore] // Evita referência circular durante a serialização JSON
        [ForeignKey("EquipeRespostaId")]
        public EquipeResposta? EquipeResposta { get; set; }
    }
}
