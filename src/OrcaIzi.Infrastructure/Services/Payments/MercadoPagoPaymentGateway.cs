using OrcaIzi.Application.DTOs;
using OrcaIzi.Application.Interfaces.Services;
using OrcaIzi.Domain.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrcaIzi.Infrastructure.Services.Payments
{
    public class MercadoPagoPaymentGateway : IPaymentGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public MercadoPagoPaymentGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<PixPaymentDto> CreatePixPaymentAsync(Budget budget, Customer customer)
        {
            var accessToken = _configuration["Payments:MercadoPago:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Mercado Pago não configurado. Defina Payments:MercadoPago:AccessToken.");
            }

            var notificationUrl = _configuration["Payments:MercadoPago:NotificationUrl"];

            var client = _httpClientFactory.CreateClient("MercadoPago");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new Dictionary<string, object?>
            {
                ["transaction_amount"] = budget.TotalAmount,
                ["description"] = $"Orçamento: {budget.Title}",
                ["payment_method_id"] = "pix",
                ["external_reference"] = budget.Id.ToString(),
                ["payer"] = new Dictionary<string, object?>
                {
                    ["email"] = customer.Email
                }
            };

            if (!string.IsNullOrWhiteSpace(notificationUrl))
            {
                payload["notification_url"] = notificationUrl;
            }

            var response = await client.PostAsJsonAsync("/v1/payments", payload);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao criar pagamento Pix no Mercado Pago ({response.StatusCode}): {content}");
            }

            var mp = JsonSerializer.Deserialize<MercadoPagoCreatePaymentResponse>(content, _jsonOptions)
                     ?? throw new Exception("Resposta inválida do Mercado Pago.");

            return new PixPaymentDto
            {
                Provider = "MercadoPago",
                ExternalId = mp.Id?.ToString() ?? string.Empty,
                Status = mp.Status ?? string.Empty,
                Amount = mp.TransactionAmount ?? budget.TotalAmount,
                TicketUrl = mp.PointOfInteraction?.TransactionData?.TicketUrl,
                QrCode = mp.PointOfInteraction?.TransactionData?.QrCode,
                QrCodeBase64 = mp.PointOfInteraction?.TransactionData?.QrCodeBase64,
                CreatedAt = mp.DateCreated
            };
        }

        public async Task<PixPaymentDto> GetPaymentAsync(string externalPaymentId)
        {
            var accessToken = _configuration["Payments:MercadoPago:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Mercado Pago não configurado. Defina Payments:MercadoPago:AccessToken.");
            }

            var client = _httpClientFactory.CreateClient("MercadoPago");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync($"/v1/payments/{externalPaymentId}");
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao consultar pagamento no Mercado Pago ({response.StatusCode}): {content}");
            }

            var mp = JsonSerializer.Deserialize<MercadoPagoCreatePaymentResponse>(content, _jsonOptions)
                     ?? throw new Exception("Resposta inválida do Mercado Pago.");

            return new PixPaymentDto
            {
                Provider = "MercadoPago",
                ExternalId = mp.Id?.ToString() ?? string.Empty,
                Status = mp.Status ?? string.Empty,
                Amount = mp.TransactionAmount ?? 0,
                TicketUrl = mp.PointOfInteraction?.TransactionData?.TicketUrl,
                QrCode = mp.PointOfInteraction?.TransactionData?.QrCode,
                QrCodeBase64 = mp.PointOfInteraction?.TransactionData?.QrCodeBase64,
                CreatedAt = mp.DateCreated
            };
        }

        private class MercadoPagoCreatePaymentResponse
        {
            public long? Id { get; set; }
            public string? Status { get; set; }
            public decimal? TransactionAmount { get; set; }
            public DateTime? DateCreated { get; set; }
            public MercadoPagoPointOfInteraction? PointOfInteraction { get; set; }
        }

        private class MercadoPagoPointOfInteraction
        {
            public MercadoPagoTransactionData? TransactionData { get; set; }
        }

        private class MercadoPagoTransactionData
        {
            public string? QrCode { get; set; }
            public string? QrCodeBase64 { get; set; }
            public string? TicketUrl { get; set; }
        }
    }
}

