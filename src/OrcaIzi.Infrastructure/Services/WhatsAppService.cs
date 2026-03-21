﻿namespace OrcaIzi.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public string GenerateWhatsAppLink(Budget budget)
        {
            var phone = budget.Customer?.Phone ?? "";
            if (string.IsNullOrEmpty(phone)) return "";

            // Remove non-numeric characters
            phone = new string(phone.Where(char.IsDigit).ToArray());
            if (!phone.StartsWith("55")) phone = "55" + phone;

            var message = new StringBuilder();
            message.AppendLine($"*Olá {budget.Customer?.Name}!*");
            message.AppendLine($"Aqui está o orçamento para *{budget.Title}*:");
            message.AppendLine($"Descrição: {budget.Description}");
            message.AppendLine("");
            message.AppendLine("*Itens:*");
            foreach (var item in budget.Items)
            {
                message.AppendLine($"- {item.Name} ({item.Quantity}x {item.UnitPrice:C}): {item.TotalPrice:C}");
            }
            message.AppendLine("");
            message.AppendLine($"*Total Geral: {budget.TotalAmount:C}*");
            message.AppendLine($"Validade: {budget.ExpirationDate:dd/MM/yyyy}");
            
            if (!string.IsNullOrEmpty(budget.Observations))
            {
                message.AppendLine("");
                message.AppendLine($"Obs: {budget.Observations}");
            }

            message.AppendLine("");
            message.AppendLine("Gerado via *OrçaIzi* 🚀");

            var encodedMessage = WebUtility.UrlEncode(message.ToString());
            return $"https://wa.me/{phone}?text={encodedMessage}";
        }
    }
}



