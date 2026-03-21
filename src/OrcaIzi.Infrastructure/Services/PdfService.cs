﻿﻿﻿﻿namespace OrcaIzi.Infrastructure.Services
{
    public class PdfService : OrcaIzi.Domain.Interfaces.IPdfService
    {
        public PdfService()
        {
            // Set license for community use
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateBudgetPdfAsync(Budget budget)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("OrçaIzi").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Orçamento Profissional").FontSize(12).Italic();
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Orçamento #{budget.Id.ToString().Substring(0, 8)}");
                            col.Item().Text($"Data: {budget.CreatedAt:dd/MM/yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text($"Cliente: {budget.Customer?.Name ?? "N/A"}").SemiBold();
                        col.Item().Text($"Título: {budget.Title}");
                        col.Item().Text($"Descrição: {budget.Description}");
                        col.Item().PaddingTop(10).Text("Itens do Orçamento:").SemiBold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item");
                                header.Cell().AlignRight().Text("Qtd");
                                header.Cell().AlignRight().Text("Unitário");
                                header.Cell().AlignRight().Text("Total");
                            });

                            foreach (var item in budget.Items)
                            {
                                table.Cell().Text(item.Name);
                                table.Cell().AlignRight().Text(item.Quantity.ToString());
                                table.Cell().AlignRight().Text(item.UnitPrice.ToString("C"));
                                table.Cell().AlignRight().Text(item.TotalPrice.ToString("C"));
                            }
                        });

                        col.Item().PaddingTop(20).AlignRight().Text($"Total Geral: {budget.TotalAmount:C}").FontSize(16).SemiBold();
                        
                        if (!string.IsNullOrEmpty(budget.Observations))
                        {
                            col.Item().PaddingTop(10).Text("Observações:").SemiBold();
                            col.Item().Text(budget.Observations);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return await Task.FromResult(document.GeneratePdf());
        }
    }
}



