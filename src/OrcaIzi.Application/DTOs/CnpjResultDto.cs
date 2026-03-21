﻿namespace OrcaIzi.Application.DTOs
{
    public class CnpjResultDto
    {
        public string Cnpj { get; set; } = string.Empty;
        public string Razao_Social { get; set; } = string.Empty;
        public string Nome_Fantasia { get; set; } = string.Empty;
        public string Situacao_Cadastral { get; set; } = string.Empty;
        public string Data_Inicio_Atividade { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Ddd_Telefone_1 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cnae_Fiscal_Descricao { get; set; } = string.Empty;
        public List<QsaDto> Qsa { get; set; } = new();
    }

    public class QsaDto
    {
        public string Nome_Socio { get; set; } = string.Empty;
        public string Qualificacao_Socio { get; set; } = string.Empty;
    }
}


