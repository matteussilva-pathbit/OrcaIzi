﻿namespace OrcaIzi.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed")]
    public class ExternalController : ControllerBase
    {
        private readonly ExternalApiService _externalApiService;

        public ExternalController(ExternalApiService externalApiService)
        {
            _externalApiService = externalApiService;
        }

        /// <summary>
        /// Busca dados de endereço pelo CEP.
        /// </summary>
        /// <param name="cep">CEP com 8 dígitos.</param>
        /// <returns>Dados do endereço ou 404.</returns>
        [HttpGet("cep/{cep}")]
        public async Task<IActionResult> GetCep(string cep)
        {
            var result = await _externalApiService.ConsultarCepAsync(cep);
            if (result == null || result.Erro)
            {
                return NotFound(new { Message = "CEP não encontrado ou inválido." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Busca dados de empresa pelo CNPJ (via BrasilAPI).
        /// </summary>
        /// <param name="cnpj">CNPJ com 14 dígitos.</param>
        /// <returns>Dados da empresa ou 404.</returns>
        [HttpGet("cnpj/{cnpj}")]
        public async Task<IActionResult> GetCnpj(string cnpj)
        {
            var result = await _externalApiService.ConsultarCnpjAsync(cnpj);
            if (result == null)
            {
                return NotFound(new { Message = "CNPJ não encontrado ou inválido." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Valida CPF e retorna região fiscal (simulação de dados básicos).
        /// </summary>
        /// <param name="cpf">CPF com 11 dígitos.</param>
        /// <returns>Status de validação e região.</returns>
        [HttpGet("cpf/{cpf}")]
        public async Task<IActionResult> GetCpf(string cpf)
        {
            var result = await _externalApiService.ConsultarCpfAsync(cpf);
            return Ok(result);
        }
    }
}



