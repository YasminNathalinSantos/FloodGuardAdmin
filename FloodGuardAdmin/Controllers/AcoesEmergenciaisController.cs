using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FloodGuardAdmin.Data;
using FloodGuardAdmin.Models;

namespace FloodGuardAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AcoesEmergenciaisController : ControllerBase
    {
        private readonly AppDbContext _context;
        private static readonly string[] StatusValidos = { "Planejada", "EmExecucao", "Concluida", "Cancelada" };

        public AcoesEmergenciaisController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Retorna todas as ações emergenciais de uma ocorrência</summary>
        /// <response code="200">Lista de ações retornada com sucesso</response>
        /// <response code="404">Ocorrência não encontrada</response>
        [HttpGet("ocorrencia/{ocorrenciaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByOcorrencia(int ocorrenciaId)
        {
            var ocorrenciaExiste = await _context.Ocorrencias.AnyAsync(o => o.Id == ocorrenciaId);
            if (!ocorrenciaExiste)
                return NotFound(new { erro = $"Ocorrência com Id {ocorrenciaId} não encontrada." });

            var acoes = await _context.AcoesEmergenciais
                .Where(a => a.OcorrenciaId == ocorrenciaId)
                .ToListAsync();

            return Ok(acoes);
        }

        /// <summary>Retorna todas as ações emergenciais de uma equipe</summary>
        /// <response code="200">Lista de ações retornada com sucesso</response>
        /// <response code="404">Equipe não encontrada</response>
        [HttpGet("equipe/{equipeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEquipe(int equipeId)
        {
            var equipeExiste = await _context.EquipesResposta.AnyAsync(e => e.Id == equipeId);
            if (!equipeExiste)
                return NotFound(new { erro = $"Equipe com Id {equipeId} não encontrada." });

            var acoes = await _context.AcoesEmergenciais
                .Where(a => a.EquipeRespostaId == equipeId)
                .ToListAsync();

            return Ok(acoes);
        }

        /// <summary>Registra uma nova ação emergencial</summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     POST /api/acoesemergenciais
        ///     {
        ///         "descricao": "Evacuação de 50 famílias do bairro Vila Esperança",
        ///         "tipoAcao": "Evacuacao",
        ///         "dataInicio": "2025-06-01T08:00:00",
        ///         "statusAcao": "Planejada",
        ///         "ocorrenciaId": 1,
        ///         "equipeRespostaId": 1
        ///     }
        ///
        /// StatusAcao válidos: Planejada | EmExecucao | Concluida | Cancelada
        /// </remarks>
        /// <response code="201">Ação emergencial criada com sucesso</response>
        /// <response code="400">Dados inválidos ou status inválido</response>
        /// <response code="404">Ocorrência ou equipe não encontrada</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] AcaoEmergencial novaAcao)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { erro = "Dados inválidos.", detalhes = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            if (!StatusValidos.Contains(novaAcao.StatusAcao))
                return BadRequest(new { erro = $"StatusAcao '{novaAcao.StatusAcao}' é inválido.", statusValidos = StatusValidos });

            var ocorrenciaExiste = await _context.Ocorrencias.AnyAsync(o => o.Id == novaAcao.OcorrenciaId);
            if (!ocorrenciaExiste)
                return NotFound(new { erro = $"Ocorrência com Id {novaAcao.OcorrenciaId} não encontrada. Crie a ocorrência antes de registrar a ação." });

            var equipeExiste = await _context.EquipesResposta.AnyAsync(e => e.Id == novaAcao.EquipeRespostaId);
            if (!equipeExiste)
                return NotFound(new { erro = $"Equipe com Id {novaAcao.EquipeRespostaId} não encontrada. Cadastre a equipe antes de registrar a ação." });

            _context.AcoesEmergenciais.Add(novaAcao);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByOcorrencia), new { ocorrenciaId = novaAcao.OcorrenciaId }, novaAcao);
        }

        /// <summary>Atualiza o status de uma ação emergencial</summary>
        /// <remarks>Valores válidos: Planejada | EmExecucao | Concluida | Cancelada</remarks>
        /// <response code="200">Status atualizado com sucesso</response>
        /// <response code="400">Status inválido</response>
        /// <response code="404">Ação não encontrada</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] string novoStatus)
        {
            var acao = await _context.AcoesEmergenciais.FindAsync(id);
            if (acao == null)
                return NotFound(new { erro = $"Ação emergencial com Id {id} não encontrada." });

            if (!StatusValidos.Contains(novoStatus))
                return BadRequest(new { erro = $"Status '{novoStatus}' é inválido.", statusValidos = StatusValidos });

            acao.StatusAcao = novoStatus;

            if (novoStatus == "Concluida")
                acao.DataFim = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = $"Status da ação atualizado para '{novoStatus}' com sucesso.", acao });
        }

        /// <summary>Remove uma ação emergencial pelo Id</summary>
        /// <response code="200">Ação removida com sucesso</response>
        /// <response code="404">Ação não encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var acao = await _context.AcoesEmergenciais.FindAsync(id);
            if (acao == null)
                return NotFound(new { erro = $"Ação emergencial com Id {id} não encontrada." });

            _context.AcoesEmergenciais.Remove(acao);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = $"Ação emergencial com Id {id} removida com sucesso." });
        }
    }
}
