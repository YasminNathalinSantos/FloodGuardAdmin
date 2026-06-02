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
    public class EquipesRespostaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipesRespostaController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Retorna todas as equipes de resposta cadastradas</summary>
        /// <response code="200">Lista de equipes retornada com sucesso</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var equipes = await _context.EquipesResposta.ToListAsync();
            return Ok(equipes);
        }

        /// <summary>Busca uma equipe de resposta pelo Id</summary>
        /// <response code="200">Equipe encontrada</response>
        /// <response code="404">Nenhuma equipe encontrada com esse Id</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var equipe = await _context.EquipesResposta
                .Include(e => e.AcoesEmergenciais)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipe == null)
                return NotFound(new { erro = $"Equipe com Id {id} não encontrada." });

            return Ok(equipe);
        }

        /// <summary>Cadastra uma nova equipe de resposta</summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     POST /api/equipesresposta
        ///     {
        ///         "nome": "Equipe Alpha - Resgate",
        ///         "especialidade": "Resgate",
        ///         "capacidadeMax": 12
        ///     }
        ///
        /// A equipe é cadastrada como disponível por padrão.
        /// </remarks>
        /// <response code="201">Equipe criada com sucesso</response>
        /// <response code="400">Dados inválidos — verifique os campos obrigatórios</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] EquipeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { erro = "Dados inválidos.", detalhes = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var equipe = new EquipeResposta(dto.Nome, dto.Especialidade, dto.CapacidadeMax);
                _context.EquipesResposta.Add(equipe);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = equipe.Id }, equipe);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>Alterna a disponibilidade da equipe (disponível ↔ indisponível)</summary>
        /// <response code="200">Disponibilidade alterada com sucesso</response>
        /// <response code="404">Equipe não encontrada</response>
        [HttpPut("{id}/disponibilidade")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AlternarDisponibilidade(int id)
        {
            var equipe = await _context.EquipesResposta.FindAsync(id);
            if (equipe == null)
                return NotFound(new { erro = $"Equipe com Id {id} não encontrada." });

            equipe.AlternarDisponibilidade();
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = $"Equipe '{equipe.Nome}' agora está {(equipe.Disponivel ? "disponível" : "indisponível")}." });
        }

        /// <summary>Remove uma equipe de resposta pelo Id</summary>
        /// <remarks>Não é possível remover uma equipe que possua ações emergenciais vinculadas.</remarks>
        /// <response code="200">Equipe removida com sucesso</response>
        /// <response code="400">Equipe possui ações emergenciais vinculadas e não pode ser removida</response>
        /// <response code="404">Equipe não encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var equipe = await _context.EquipesResposta.FindAsync(id);
            if (equipe == null)
                return NotFound(new { erro = $"Equipe com Id {id} não encontrada." });

            var possuiAcoes = await _context.AcoesEmergenciais.AnyAsync(a => a.EquipeRespostaId == id);
            if (possuiAcoes)
                return BadRequest(new { erro = $"Não é possível excluir a equipe '{equipe.Nome}' pois ela possui ações emergenciais vinculadas.", solucao = "Remova ou reatribua as ações antes de deletar a equipe." });

            _context.EquipesResposta.Remove(equipe);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = $"Equipe '{equipe.Nome}' removida com sucesso." });
        }
    }

    public record EquipeCreateDto(
        [Required(ErrorMessage = "O nome da equipe é obrigatório")][MaxLength(150)] string Nome,
        [Required(ErrorMessage = "A especialidade é obrigatória")][MaxLength(100)] string Especialidade,
        [Range(1, int.MaxValue, ErrorMessage = "A capacidade máxima deve ser maior que zero")] int CapacidadeMax
    );
}
