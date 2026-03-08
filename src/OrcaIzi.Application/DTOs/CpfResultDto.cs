namespace OrcaIzi.Application.DTOs
{
    public class CpfResultDto
    {
        public string Cpf { get; set; }
        public string CpfFormatado { get; set; }
        public bool Valido { get; set; }
        public string RegiaoFiscal { get; set; }
        public string Mensagem { get; set; }
    }
}
