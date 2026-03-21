﻿namespace OrcaIzi.Application.DTOs
{
    public class CatalogItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCatalogItemDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Unit { get; set; }
        public string? Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor unitário deve ser maior que zero")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        public bool IsActive { get; set; } = true;
    }
}




