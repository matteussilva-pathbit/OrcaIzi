using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrcaIzi.Application.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OrcaIzi.Infrastructure.Services.External
{
    public class ExternalApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExternalApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExternalApiService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<ExternalApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<CepResultDto?> ConsultarCepAsync(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep)) return null;
            cep = Regex.Replace(cep, "[^0-9]", "");
            if (cep.Length != 8) return null;

            if (_cache.TryGetValue($"cep_{cep}", out CepResultDto? cached))
            {
                return cached;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "OrcaIzi-App/1.0");

            try
            {
                var response = await client.GetAsync($"https://viacep.com.br/ws/{cep}/json/");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CepResultDto>(_jsonOptions);
                    if (result != null && !result.Erro)
                    {
                        _cache.Set($"cep_{cep}", result, TimeSpan.FromHours(24));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar CEP {Cep}", cep);
            }
            return null;
        }

        public async Task<CnpjResultDto?> ConsultarCnpjAsync(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return null;
            cnpj = Regex.Replace(cnpj, "[^0-9]", "");
            
            _logger.LogInformation("Iniciando consulta CNPJ: {Cnpj}", cnpj);

            // Validação local
            if (!IsCnpjValid(cnpj)) 
            {
                _logger.LogWarning("CNPJ inválido na validação local: {Cnpj}", cnpj);
                return null;
            }

            if (_cache.TryGetValue($"cnpj_{cnpj}", out CnpjResultDto? cached))
            {
                _logger.LogInformation("CNPJ retornado do cache: {Cnpj}", cnpj);
                return cached;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "OrcaIzi-App/1.0");
            
            // Tentativa 1: BrasilAPI
            try
            {
                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpj}";
                _logger.LogInformation("Consultando BrasilAPI: {Url}", url);
                
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CnpjResultDto>(_jsonOptions);
                    if (result != null)
                    {
                        _cache.Set($"cnpj_{cnpj}", result, TimeSpan.FromHours(2));
                        return result;
                    }
                }
                else
                {
                    _logger.LogWarning("BrasilAPI retornou erro: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar BrasilAPI para CNPJ {Cnpj}", cnpj);
            }

            // Tentativa 2: ReceitaWS (Fallback)
            try
            {
                var url = $"https://www.receitaws.com.br/v1/cnpj/{cnpj}";
                _logger.LogInformation("Tentando fallback ReceitaWS: {Url}", url);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // ReceitaWS tem formato diferente, precisamos adaptar ou usar um DTO intermediário
                    // Por simplicidade, vamos tentar mapear o básico se possível ou usar dynamic
                    var resultWs = await response.Content.ReadFromJsonAsync<ReceitaWsDto>(_jsonOptions);
                    if (resultWs != null && resultWs.Status == "OK")
                    {
                        var result = new CnpjResultDto
                        {
                            Cnpj = resultWs.Cnpj.Replace(".", "").Replace("/", "").Replace("-", ""),
                            Razao_Social = resultWs.Nome,
                            Nome_Fantasia = resultWs.Fantasia,
                            Logradouro = resultWs.Logradouro,
                            Numero = resultWs.Numero,
                            Bairro = resultWs.Bairro,
                            Municipio = resultWs.Municipio,
                            Uf = resultWs.Uf,
                            Cep = resultWs.Cep.Replace(".", "").Replace("-", ""),
                            Ddd_Telefone_1 = resultWs.Telefone,
                            Email = resultWs.Email,
                            Situacao_Cadastral = resultWs.Situacao,
                            Data_Inicio_Atividade = resultWs.Abertura
                        };
                        
                        _cache.Set($"cnpj_{cnpj}", result, TimeSpan.FromHours(2));
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar ReceitaWS para CNPJ {Cnpj}", cnpj);
            }

            return null;
        }

        // DTO Interno para ReceitaWS
        private class ReceitaWsDto
        {
            public string Cnpj { get; set; }
            public string Nome { get; set; }
            public string Fantasia { get; set; }
            public string Logradouro { get; set; }
            public string Numero { get; set; }
            public string Bairro { get; set; }
            public string Municipio { get; set; }
            public string Uf { get; set; }
            public string Cep { get; set; }
            public string Telefone { get; set; }
            public string Email { get; set; }
            public string Situacao { get; set; }
            public string Abertura { get; set; }
            public string Status { get; set; }
        }

        public Task<CpfResultDto> ConsultarCpfAsync(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return Task.FromResult(new CpfResultDto { Valido = false, Mensagem = "CPF não informado." });
            }

            cpf = Regex.Replace(cpf, "[^0-9]", "");
            
            if (cpf.Length != 11)
            {
                return Task.FromResult(new CpfResultDto { Cpf = cpf, Valido = false, Mensagem = "CPF deve ter 11 dígitos." });
            }

            bool valido = IsCpfValid(cpf);
            string regiao = GetCpfRegion(cpf);
            string formatado = Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");

            var result = new CpfResultDto
            {
                Cpf = cpf,
                CpfFormatado = formatado,
                Valido = valido,
                RegiaoFiscal = regiao,
                Mensagem = valido ? "CPF válido." : "CPF inválido (dígitos verificadores incorretos)."
            };

            return Task.FromResult(result);
        }

        private static bool IsCnpjValid(string cnpj)
        {
            if (cnpj.Length != 14) return false;
            
            // Check for repeated digits (blacklist)
            var invalidNumbers = new[]
            {
                "00000000000000", "11111111111111", "22222222222222",
                "33333333333333", "44444444444444", "55555555555555",
                "66666666666666", "77777777777777", "88888888888888",
                "99999999999999"
            };
            if (invalidNumbers.Contains(cnpj)) return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = (soma % 11);
            if (resto < 2) resto = 0;
            else resto = 11 - resto;

            string digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2) resto = 0;
            else resto = 11 - resto;

            digito += resto.ToString();
            return cnpj.EndsWith(digito);
        }

        private static bool IsCpfValid(string cpf)
        {
            // Check for repeated digits (blacklist)
            var invalidNumbers = new[]
            {
                "00000000000", "11111111111", "22222222222",
                "33333333333", "44444444444", "55555555555",
                "66666666666", "77777777777", "88888888888",
                "99999999999"
            };
            if (invalidNumbers.Contains(cpf)) return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2) resto = 0;
            else resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2) resto = 0;
            else resto = 11 - resto;

            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }

        private static string GetCpfRegion(string cpf)
        {
            if (cpf.Length < 9) return "Desconhecida";
            char digit = cpf[8];
            return digit switch
            {
                '1' => "DF, GO, MS, MT, TO",
                '2' => "AC, AM, AP, PA, RO, RR",
                '3' => "CE, MA, PI",
                '4' => "AL, PB, PE, RN",
                '5' => "BA, SE",
                '6' => "MG",
                '7' => "ES, RJ",
                '8' => "SP",
                '9' => "PR, SC",
                '0' => "RS",
                _ => "Desconhecida"
            };
        }
    }
}