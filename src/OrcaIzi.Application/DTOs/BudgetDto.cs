using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrcaIzi.Application.DTOs
{
    public class BudgetDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public string? Observations { get; set; }
        public string? DigitalSignature { get; set; }
        public List<BudgetItemDto> Items { get; set; } = new();
    }

    public class CreateBudgetDto
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cliente é obrigatório")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "A data de validade é obrigatória")]
        public DateTime ExpirationDate { get; set; } = DateTime.Now.AddDays(7);

        public string? Observations { get; set; }
        public string Status { get; set; } = "Draft";
        public List<CreateBudgetItemDto> Items { get; set; } = new();
    }

    public class BudgetItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateBudgetItemDto
    {
        [Required(ErrorMessage = "O nome do item é obrigatório")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor unitário deve ser maior que zero")]
        public decimal UnitPrice { get; set; }
    }
}
