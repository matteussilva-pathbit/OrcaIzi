namespace OrcaIzi.Application.DTOs
{
    public class CpfResultDto
    {
        public string Cpf { get; set; } = string.Empty;
        public string CpfFormatado { get; set; } = string.Empty;
        public bool Valido { get; set; }
        public string RegiaoFiscal { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
    }
}
