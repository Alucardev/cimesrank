using Microsoft.AspNetCore.Mvc;
using ApiRanking.Services;  // Importa el espacio de nombres de tu servicio
using ApiRanking.Core.Abstractions;

namespace ApiRanking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingController : ControllerBase
    {
        private readonly RankingFiadoService _rankingFiadoService;
        private readonly RankingRecaudacionService _rankingRecaudacionService;

        // Inyectamos el servicio en el controlador
        public RankingController(RankingFiadoService rankingFiadoService, RankingRecaudacionService rankingRecaudacionService)
        {
            _rankingFiadoService = rankingFiadoService;
            _rankingRecaudacionService = rankingRecaudacionService; 
        }

        // Endpoint para WS_GET_RECAUDACION_NETA
        [HttpGet("recaudacion")]
        public async Task<IActionResult> GetRecaudacionNeta()
        {     
               var result = await _rankingRecaudacionService.GetRecaudacionNetaAsync();   

               if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

            return Ok(result.Value);
        }

        [HttpGet("fiados")]
        public async Task<IActionResult> GetFiados()
        {
                // Llamamos al servicio para obtener la lista ordenada
                var result = await _rankingFiadoService.GetFiadoRepartosAsync();
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }
                // Retornamos la lista ordenada
                return Ok(result.Value);
        }
    }
}
