using System.Collections.Generic;

namespace OrcaIzi.Application.DTOs
{
    public class CnpjResultDto
    {
        public string Cnpj { get; set; }
        public string Razao_Social { get; set; }
        public string Nome_Fantasia { get; set; }
        public string Situacao_Cadastral { get; set; }
        public string Data_Inicio_Atividade { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string Uf { get; set; }
        public string Cep { get; set; }
        public string Ddd_Telefone_1 { get; set; }
        public string Email { get; set; }
        public string Cnae_Fiscal_Descricao { get; set; }
        public List<QsaDto> Qsa { get; set; } = new();
    }

    public class QsaDto
    {
        public string Nome_Socio { get; set; }
        public string Qualificacao_Socio { get; set; }
    }
}
