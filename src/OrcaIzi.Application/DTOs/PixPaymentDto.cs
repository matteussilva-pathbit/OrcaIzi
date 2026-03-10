namespace OrcaIzi.Application.DTOs
{
    public class PixPaymentDto
    {
        public string Provider { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? QrCode { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? TicketUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class MercadoPagoWebhookDto
    {
        public MercadoPagoWebhookDataDto? Data { get; set; }
        public string? Type { get; set; }
        public string? Action { get; set; }
    }

    public class MercadoPagoWebhookDataDto
    {
        public string? Id { get; set; }
    }
}
