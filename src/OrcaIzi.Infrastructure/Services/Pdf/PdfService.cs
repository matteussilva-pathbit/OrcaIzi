﻿namespace OrcaIzi.Infrastructure.Services.Pdf
{
    public class PdfService : OrcaIzi.Application.Interfaces.Services.IPdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateBudgetPdf(BudgetDto budget)
        {
            var culture = new CultureInfo("pt-BR");
            var accent = "#0D6EFD";
            var dark = "#111827";
            var muted = Colors.Grey.Darken2;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.6f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Inter"));

                    page.Header().Element(header =>
                    {
                        header
                            .Background(dark)
                            .PaddingVertical(14)
                            .PaddingHorizontal(16)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    var company = budget.CompanyName ?? "Sua Empresa";
                                    col.Item().Text(company).FontColor(Colors.White).FontSize(16).SemiBold();

                                    var companyLine = BuildCompanyLine(budget);
                                    if (!string.IsNullOrWhiteSpace(companyLine))
                                    {
                                        col.Item().Text(companyLine).FontColor(Colors.Grey.Lighten2).FontSize(9);
                                    }
                                });

                                row.ConstantItem(260).AlignRight().Column(col =>
                                {
                                    col.Item().Text($"ORÇAMENTO #{budget.Id.ToString()[..8].ToUpperInvariant()}").FontColor(Colors.White).FontSize(12).SemiBold();
                                    col.Item().Text($"Emissão: {budget.CreatedAt.ToString("dd/MM/yyyy", culture)}").FontColor(Colors.Grey.Lighten2).FontSize(9);
                                    col.Item().Text($"Validade: {budget.ExpirationDate.ToString("dd/MM/yyyy", culture)}").FontColor(Colors.Grey.Lighten2).FontSize(9);

                                    var status = StatusToPt(budget.Status);
                                    col.Item().PaddingTop(4).AlignRight().Element(badge =>
                                            badge.Background(GetStatusColor(budget.Status, accent))
                                                .PaddingVertical(4)
                                                .PaddingHorizontal(10))
                                        .Text(status)
                                        .FontSize(9)
                                        .FontColor(Colors.White);
                                });
                            });
                    });

                    page.Content()
                        .PaddingTop(14)
                        .Column(x =>
                        {
                            x.Spacing(14);

                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Element(container => SectionCard(container, "Cliente", accent, col =>
                                {
                                    col.Spacing(4);
                                    col.Item().Text(budget.CustomerName).FontSize(12).SemiBold().FontColor(Colors.Black);
                                    var customerLine = BuildCustomerLine(budget);
                                    if (!string.IsNullOrWhiteSpace(customerLine))
                                    {
                                        col.Item().Text(customerLine).FontSize(9).FontColor(muted);
                                    }
                                }));

                                row.ConstantItem(12);

                                row.RelativeItem().Element(container => SectionCard(container, "Resumo", accent, col =>
                                {
                                    col.Spacing(6);
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Total").FontColor(muted).FontSize(9);
                                        r.RelativeItem().AlignRight().Text(budget.TotalAmount.ToString("C2", culture)).FontSize(12).SemiBold().FontColor(Colors.Black);
                                    });
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Itens").FontColor(muted).FontSize(9);
                                        r.RelativeItem().AlignRight().Text((budget.Items?.Count ?? 0).ToString(culture)).FontSize(10);
                                    });
                                }));
                            });

                            x.Item().Element(container => SectionCard(container, "Escopo / Descrição", accent, col =>
                            {
                                col.Item().Text(string.IsNullOrWhiteSpace(budget.Description) ? "—" : budget.Description).FontSize(10).FontColor(Colors.Black);
                            }));

                            x.Item().Element(container => SectionCard(container, "Itens do orçamento", accent, col =>
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(5);
                                        columns.ConstantColumn(40);
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(85);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(t => HeaderCellStyle(t, dark)).Text("ITEM");
                                        header.Cell().Element(t => HeaderCellStyle(t, dark)).Text("DESCRIÇÃO");
                                        header.Cell().Element(t => HeaderCellStyle(t, dark)).AlignRight().Text("QTD");
                                        header.Cell().Element(t => HeaderCellStyle(t, dark)).AlignRight().Text("UNIT.");
                                        header.Cell().Element(t => HeaderCellStyle(t, dark)).AlignRight().Text("TOTAL");
                                    });

                                    var items = budget.Items ?? new List<BudgetItemDto>();
                                    for (var i = 0; i < items.Count; i++)
                                    {
                                        var item = items[i];
                                        var rowBg = i % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;

                                        table.Cell().Element(c => CellStyle(c, rowBg)).Text(item.Name).SemiBold();
                                        table.Cell().Element(c => CellStyle(c, rowBg)).Text(item.Description ?? string.Empty).FontColor(muted).FontSize(9);
                                        table.Cell().Element(c => CellStyle(c, rowBg)).AlignRight().Text(item.Quantity.ToString(culture));
                                        table.Cell().Element(c => CellStyle(c, rowBg)).AlignRight().Text(item.UnitPrice.ToString("C2", culture));
                                        table.Cell().Element(c => CellStyle(c, rowBg)).AlignRight().Text(item.TotalPrice.ToString("C2", culture)).SemiBold();
                                    }
                                });
                            }));

                            x.Item().Row(row =>
                            {
                                row.RelativeItem();
                                row.ConstantItem(260).Element(container => SummaryCard(container, accent, col =>
                                {
                                    col.Spacing(8);

                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Total geral").FontSize(10).FontColor(muted);
                                        r.RelativeItem().AlignRight().Text(budget.TotalAmount.ToString("C2", culture)).FontSize(14).SemiBold().FontColor(Colors.Black);
                                    });

                                    if (!string.IsNullOrWhiteSpace(budget.PaymentStatus))
                                    {
                                        col.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text("Pagamento").FontSize(10).FontColor(muted);
                                            r.RelativeItem().AlignRight().Text(StatusToPtPayment(budget.PaymentStatus)).FontSize(10).SemiBold().FontColor(Colors.Black);
                                        });
                                    }
                                }));
                            });

                            if (!string.IsNullOrWhiteSpace(budget.PaymentExternalId))
                            {
                                x.Item().Element(container => SectionCard(container, "Pagamento (Pix)", accent, col =>
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem().Column(col2 =>
                                        {
                                            col2.Spacing(6);
                                            col2.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Provedor").FontSize(9).FontColor(muted);
                                                r.RelativeItem().AlignRight().Text(budget.PaymentProvider ?? "—").FontSize(9);
                                            });
                                            col2.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Status").FontSize(9).FontColor(muted);
                                                r.RelativeItem().AlignRight().Text(StatusToPtPayment(budget.PaymentStatus)).FontSize(9).SemiBold();
                                            });
                                            col2.Item().Row(r =>
                                            {
                                                r.RelativeItem().Text("Link").FontSize(9).FontColor(muted);
                                                r.RelativeItem().AlignRight().Text(Shorten(budget.PaymentLink, 42)).FontSize(9).FontColor(Colors.Blue.Medium);
                                            });

                                            if (!string.IsNullOrWhiteSpace(budget.PaymentQrCode))
                                            {
                                                col2.Item().PaddingTop(8).Text("Copia e cola").FontSize(9).FontColor(muted);
                                                col2.Item().Background(Colors.Grey.Lighten4).Padding(10).Text(budget.PaymentQrCode).FontSize(8);
                                            }
                                        });

                                        row.ConstantItem(16);

                                        row.ConstantItem(180).AlignCenter()
                                            .Padding(8)
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Column(col3 =>
                                            {
                                                col3.Spacing(6);
                                                if (TryDecodeBase64(budget.PaymentQrCodeBase64, out var bytes))
                                                {
                                                    col3.Item().AlignCenter().Image(bytes).FitWidth();
                                                    col3.Item().AlignCenter().Text("Aponte a câmera para pagar").FontSize(8).FontColor(muted);
                                                }
                                                else
                                                {
                                                    col3.Item().AlignCenter().Text("QR Code indisponível").FontSize(9).FontColor(muted);
                                                }
                                            });
                                    });
                                }));
                            }

                            if (!string.IsNullOrWhiteSpace(budget.Observations))
                            {
                                x.Item().Element(container => SectionCard(container, "Observações / Condições", accent, col =>
                                {
                                    col.Item().Text(budget.Observations).FontSize(9).FontColor(Colors.Black);
                                }));
                            }
                        });

                    page.Footer()
                        .PaddingTop(10)
                        .Column(col =>
                        {
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            col.Item().PaddingTop(8).Row(row =>
                            {
                                var companyFooter = BuildCompanyFooterLine(budget);
                                row.RelativeItem().Text(companyFooter).FontSize(8).FontColor(Colors.Grey.Darken1);
                                row.RelativeItem().AlignRight().Text(t =>
                                {
                                    t.Span("Página ");
                                    t.CurrentPageNumber();
                                });
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container, string background)
        {
            return container
                .Background(background)
                .PaddingVertical(8)
                .PaddingHorizontal(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3);
        }

        private static IContainer HeaderCellStyle(IContainer container, string background)
        {
            return container.DefaultTextStyle(x => x.SemiBold().FontSize(10).FontColor(Colors.White))
                            .Background(background)
                            .PaddingVertical(8)
                            .PaddingHorizontal(8);
        }

        private static IContainer SectionCard(IContainer container, string title, string accent, Action<ColumnDescriptor> body)
        {
            var card = container
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(14);

            card.Column(col =>
            {
                col.Spacing(10);
                col.Item().Row(row =>
                {
                    row.ConstantItem(4).Height(14).Background(accent);
                    row.ConstantItem(8);
                    row.RelativeItem().Text(title).FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
                });
                body(col);
            });

            return card;
        }

        private static IContainer SummaryCard(IContainer container, string accent, Action<ColumnDescriptor> body)
        {
            var card = container
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(14);

            card.Column(col =>
            {
                col.Spacing(10);
                col.Item().Row(row =>
                {
                    row.ConstantItem(4).Height(14).Background(accent);
                    row.ConstantItem(8);
                    row.RelativeItem().Text("Totais").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
                });
                body(col);
            });

            return card;
        }

        private static string StatusToPt(string status)
        {
            return status switch
            {
                "Draft" => "Rascunho",
                "Sent" => "Enviado",
                "Approved" => "Aprovado",
                "Rejected" => "Rejeitado",
                "Cancelled" => "Cancelado",
                "Paid" => "Pago",
                _ => string.IsNullOrWhiteSpace(status) ? "—" : status
            };
        }

        private static string StatusToPtPayment(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "—";
            return status.ToLowerInvariant() switch
            {
                "approved" => "Aprovado",
                "accredited" => "Aprovado",
                "paid" => "Pago",
                "pending" => "Pendente",
                "in_process" => "Em processamento",
                "rejected" => "Rejeitado",
                "cancelled" => "Cancelado",
                "refunded" => "Estornado",
                "charged_back" => "Chargeback",
                _ => status
            };
        }

        private static string GetStatusColor(string status, string accent)
        {
            return status switch
            {
                "Paid" => Colors.Green.Darken2,
                "Approved" => accent,
                "Rejected" => Colors.Red.Darken2,
                "Sent" => Colors.Orange.Darken2,
                "Cancelled" => Colors.Grey.Darken3,
                _ => Colors.Grey.Darken3
            };
        }

        private static string? BuildCustomerLine(BudgetDto budget)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(budget.CustomerDocument)) parts.Add(budget.CustomerDocument);
            if (!string.IsNullOrWhiteSpace(budget.CustomerEmail)) parts.Add(budget.CustomerEmail);
            if (!string.IsNullOrWhiteSpace(budget.CustomerPhone)) parts.Add(budget.CustomerPhone);
            if (!string.IsNullOrWhiteSpace(budget.CustomerAddress)) parts.Add(budget.CustomerAddress);
            return parts.Count == 0 ? null : string.Join(" • ", parts);
        }

        private static string? BuildCompanyLine(BudgetDto budget)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(budget.CompanyCnpj)) parts.Add(budget.CompanyCnpj);
            if (!string.IsNullOrWhiteSpace(budget.CompanyEmail)) parts.Add(budget.CompanyEmail);
            if (!string.IsNullOrWhiteSpace(budget.CompanyPhone)) parts.Add(budget.CompanyPhone);
            return parts.Count == 0 ? null : string.Join(" • ", parts);
        }

        private static string BuildCompanyFooterLine(BudgetDto budget)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(budget.CompanyName)) parts.Add(budget.CompanyName);
            if (!string.IsNullOrWhiteSpace(budget.CompanyEmail)) parts.Add(budget.CompanyEmail);
            if (!string.IsNullOrWhiteSpace(budget.CompanyPhone)) parts.Add(budget.CompanyPhone);
            if (!string.IsNullOrWhiteSpace(budget.CompanyAddress)) parts.Add(budget.CompanyAddress);
            return parts.Count == 0 ? "Gerado por OrcaIzi" : string.Join(" • ", parts);
        }

        private static bool TryDecodeBase64(string? base64, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();
            if (string.IsNullOrWhiteSpace(base64)) return false;
            try
            {
                bytes = Convert.FromBase64String(base64);
                return bytes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string Shorten(string? text, int max)
        {
            if (string.IsNullOrWhiteSpace(text)) return "—";
            if (text.Length <= max) return text;
            return text.Substring(0, max - 1) + "…";
        }
    }
}



