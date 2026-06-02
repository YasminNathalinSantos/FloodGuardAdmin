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
    public class OcorrenciasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OcorrenciasController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Retorna todas as ocorrências cadastradas</summary>
        /// <response code="200">Lista de ocorrências retornada com sucesso</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var ocorrencias = await _context.Ocorrencias.ToListAsync();
            return Ok(ocorrencias);
        }

        /// <summary>Busca uma ocorrência pelo Id</summary>
        /// <response code="200">Ocorrência encontrada</response>
        /// <response code="404">Nenhuma ocorrência encontrada com esse Id</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var ocorrencia = await _context.Ocorrencias
                .Include(o => o.AcoesEmergenciais)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ocorrencia == null)
                return NotFound(new { erro = $"Ocorrência com Id {id} não encontrada." });

            return Ok(ocorrencia);
        }

        /// <summary>Registra uma nova ocorrência de enchente</summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     POST /api/ocorrencias
        ///     {
        ///         "titulo": "Alagamento na Av. Paulista",
        ///         "localizacao": "Av. Paulista, 1000 - São Paulo/SP",
        ///         "nivelSeveridade": 3
        ///     }
        ///
        /// NivelSeveridade: 1=Baixo, 2=Médio, 3=Alto, 4=Crítico
        /// </remarks>
        /// <response code="201">Ocorrência criada com sucesso</response>
        /// <response code="400">Dados inválidos — verifique os campos obrigatórios</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OcorrenciaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { erro = "Dados inválidos.", detalhes = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var ocorrencia = new Ocorrencia(dto.Titulo, dto.Localizacao, dto.NivelSeveridade);
                _context.Ocorrencias.Add(ocorrencia);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = ocorrencia.Id }, ocorrencia);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>Atualiza o status de uma ocorrência</summary>
        /// <remarks>Valores válidos: Aberta | EmAtendimento | Encerrada</remarks>
        /// <response code="204">Status atualizado com sucesso</response>
        /// <response code="400">Status inválido</response>
        /// <response code="404">Ocorrência não encontrada</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] string novoStatus)
        {
            var ocorrencia = await _context.Ocorrencias.FindAsync(id);
            if (ocorrencia == null)
                return NotFound(new { erro = $"Ocorrência com Id {id} não encontrada." });

            try
            {
                ocorrencia.AtualizarStatus(novoStatus);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message, statusValidos = new[] { "Aberta", "EmAtendimento", "Encerrada" } });
            }
        }

        /// <summary>Escala o nível de severidade da ocorrência em +1</summary>
        /// <response code="200">Severidade escalada com sucesso</response>
        /// <response code="400">Ocorrência já está no nível crítico (4)</response>
        /// <response code="404">Ocorrência não encontrada</response>
        [HttpPut("{id}/escalar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EscalarSeveridade(int id)
        {
            var ocorrencia = await _context.Ocorrencias.FindAsync(id);
            if (ocorrencia == null)
                return NotFound(new { erro = $"Ocorrência com Id {id} não encontrada." });

            try
            {
                ocorrencia.EscalarSeveridade();
                await _context.SaveChangesAsync();
                return Ok(new { mensagem = $"Severidade escalada para nível {ocorrencia.NivelSeveridade}.", ocorrencia });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>Remove uma ocorrência pelo Id</summary>
        /// <remarks>Ao deletar, todas as ações emergenciais vinculadas são removidas automaticamente (CASCADE).</remarks>
        /// <response code="200">Ocorrência removida com sucesso</response>
        /// <response code="404">Ocorrência não encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ocorrencia = await _context.Ocorrencias.FindAsync(id);
            if (ocorrencia == null)
                return NotFound(new { erro = $"Ocorrência com Id {id} não encontrada." });

            _context.Ocorrencias.Remove(ocorrencia);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = $"Ocorrência '{ocorrencia.Titulo}' removida com sucesso. Todas as ações emergenciais vinculadas foram removidas." });
        }
    }

    public record OcorrenciaCreateDto(
        [Required(ErrorMessage = "O título é obrigatório")][MaxLength(200)] string Titulo,
        [Required(ErrorMessage = "A localização é obrigatória")][MaxLength(300)] string Localizacao,
        [Range(1, 4, ErrorMessage = "NivelSeveridade deve ser entre 1 (Baixo) e 4 (Crítico)")] int NivelSeveridade
    );
}
