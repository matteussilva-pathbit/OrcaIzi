using OrcaIzi.Application.DTOs;
using OrcaIzi.Application.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrcaIzi.Infrastructure.Services.Pdf
{
    public class PdfService : OrcaIzi.Application.Interfaces.Services.IPdfService
    {
        public PdfService()
        {
            // Configure QuestPDF license (Community License for open source/personal use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateBudgetPdf(BudgetDto budget)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("ORÇAMENTO").FontSize(32).SemiBold().FontColor("#FFD700"); // Gold
                                column.Item().Text($"#{budget.Id.ToString().Substring(0, 8).ToUpper()}").FontSize(14).FontColor(Colors.Grey.Medium);
                            });

                            // Future: Add Logo here
                            // row.ConstantItem(100).Image(...)
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            // Divider
                            x.Item().LineHorizontal(2).LineColor("#FFD700"); // Gold Line

                            // Header Info
                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("DADOS DO CLIENTE").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                    column.Item().Text(budget.CustomerName).FontSize(14).Bold();
                                    // Future: Add Customer Document/Address
                                });

                                row.RelativeItem().AlignRight().Column(column =>
                                {
                                    column.Item().Text("DETALHES").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                    column.Item().Text($"Data: {DateTime.Now:dd/MM/yyyy}");
                                    column.Item().Text($"Validade: {budget.ExpirationDate:dd/MM/yyyy}");
                                    
                                    var statusText = budget.Status switch
                                    {
                                        "Draft" => "Rascunho",
                                        "Sent" => "Enviado",
                                        "Approved" => "Aprovado",
                                        "Rejected" => "Rejeitado",
                                        _ => budget.Status
                                    };
                                    
                                    column.Item().Text($"Status: {statusText}").FontColor(budget.Status == "Approved" ? Colors.Green.Medium : Colors.Grey.Darken2);
                                });
                            });

                            x.Item().PaddingTop(10).Column(col => 
                            {
                                col.Item().Text("DESCRIÇÃO DO PROJETO").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                                col.Item().Text(budget.Description).FontSize(12);
                            });

                            // Items Table
                            x.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("ITEM");
                                    header.Cell().Element(HeaderCellStyle).Text("DESCRIÇÃO");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("QTD");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("UNITÁRIO");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("TOTAL");
                                });

                                foreach (var item in budget.Items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Name).Bold();
                                    table.Cell().Element(CellStyle).Text(item.Description).FontColor(Colors.Grey.Darken1);
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.UnitPrice.ToString("C2"));
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.TotalPrice.ToString("C2")).SemiBold();
                                }
                            });

                            // Total
                            x.Item().PaddingTop(10).Row(row => 
                            {
                                row.RelativeItem(); // Spacer
                                row.RelativeItem().Column(col => 
                                {
                                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                    col.Item().PaddingTop(5).Row(r => 
                                    {
                                        r.RelativeItem().Text("TOTAL GERAL").FontSize(14).SemiBold();
                                        r.RelativeItem().AlignRight().Text(budget.TotalAmount.ToString("C2")).FontSize(18).Bold().FontColor("#000000"); // Black
                                    });
                                });
                            });

                            if (!string.IsNullOrEmpty(budget.Observations))
                            {
                                x.Item().PaddingTop(20).Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
                                {
                                    column.Item().Text("OBSERVAÇÕES & TERMOS").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                                    column.Item().Text(budget.Observations).FontSize(10).Italic();
                                });
                            }
                        });

                    page.Footer()
                        .Column(col => 
                        {
                            col.Item().LineHorizontal(1).LineColor("#FFD700");
                            col.Item().PaddingTop(10).Row(row => 
                            {
                                row.RelativeItem().Text("Gerado por OrcaIzi").FontSize(9).FontColor(Colors.Grey.Medium);
                                row.RelativeItem().AlignRight().Text(x =>
                                {
                                    x.Span("Página ");
                                    x.CurrentPageNumber();
                                });
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold().FontSize(10).FontColor(Colors.White))
                            .Background(Colors.Black)
                            .PaddingVertical(8)
                            .PaddingHorizontal(5);
        }
    }
}
