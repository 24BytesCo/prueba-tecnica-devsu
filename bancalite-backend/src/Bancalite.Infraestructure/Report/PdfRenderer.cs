using Bancalite.Application.Interface;
using Bancalite.Application.Config;
using Bancalite.Application.Reportes.EstadoCuenta;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Bancalite.Infraestructure.Report
{
    /// <summary>
    /// Renderizador PDF con QuestPDF: cabecera con branding, resumen de totales y tabla paginada.
    /// </summary>
    public class PdfRenderer : IPdfRenderer
    {
        private readonly string _brand;
        private readonly string _accent;

        public PdfRenderer(Microsoft.Extensions.Options.IOptions<ReportOptions> options)
        {
            _brand = string.IsNullOrWhiteSpace(options.Value.BrandName) ? "Bancalite" : options.Value.BrandName!;
            _accent = string.IsNullOrWhiteSpace(options.Value.AccentColor) ? Colors.Red.Medium : options.Value.AccentColor!;
        }

        public Task<byte[]> RenderEstadoCuentaAsync(object reporteDto, CancellationToken ct = default)
        {
            var dto = (EstadoCuentaDto)reporteDto;

            var accent = _accent; // estilo bancario
            var softHeader = Colors.Grey.Lighten4;
            var lightText = Colors.Grey.Darken2;
            var culture = new CultureInfo("es-ES");
            string F(decimal v) => v.ToString("N2", culture);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Encabezado estilo extracto
                    page.Header().Background(softHeader).Padding(14).Column(h =>
                    {
                        h.Item().AlignCenter().Text("EXTRACTO DE LA CUENTA").FontSize(16).SemiBold();
                        h.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().Column(col =>
                            {
                                var fechaEmision = DateTime.UtcNow.ToString("dd MMMM yyyy");
                                col.Item().Text(text => { text.Span("FECHA: ").SemiBold(); text.Span(fechaEmision); });
                                if (!string.IsNullOrWhiteSpace(dto.ClienteNombre))
                                    col.Item().Text(text => { text.Span("TITULAR: ").SemiBold(); text.Span(dto.ClienteNombre!); });
                                col.Item().Text(text => { text.Span("PERIODO: ").SemiBold(); text.Span($"{dto.Desde:dd.MM.yyyy} - {dto.Hasta:dd.MM.yyyy}"); });
                            });
                            r.ConstantItem(240).Column(col =>
                            {
                                // Caja con desglose de código de cuenta
                                col.Item().Element(BoxCuenta);
                            });
                        });
                        h.Item().AlignRight().Text(_brand).FontColor(lightText);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        // Tabla de movimientos con estilo de extracto
                        col.Item().Element(TablaExtracto);
                        // Línea de totales y saldos al final
                        col.Item().PaddingTop(8).Element(ResumenPie);
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Página ").FontColor(lightText);
                        t.CurrentPageNumber();
                        t.Span(" de ").FontColor(lightText);
                        t.TotalPages();
                    });

                    void BoxCuenta(IContainer container)
                    {
                        var digits = string.IsNullOrWhiteSpace(dto.NumeroCuenta) ? string.Empty : new string(dto.NumeroCuenta!.Where(char.IsDigit).ToArray());
                        string GetPart(int start, int len, string def) => digits.Length >= start + len ? digits.Substring(start, len) : def;
                        var entidad = GetPart(0, 4, "0000");
                        var oficina = GetPart(4, 4, "0000");
                        var dc = GetPart(8, 2, "00");
                        var num = digits.Length > 10 ? digits.Substring(10) : GetPart(10, Math.Max(0, digits.Length - 10), string.Empty);

                        container.Border(1).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("CUENTA").SemiBold();
                            c.Item().Table(t =>
                            {
                                t.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(228);   // Núm. de Cuenta
                                });
                                // headers
                                t.Cell().Element(HCell).Text("Núm. de Cuenta");
                                // values

                                t.Cell().Element(VCell).Text(string.IsNullOrWhiteSpace(num) ? digits : num);

                                IContainer HCell(IContainer x) => x.Border(1).Padding(3).DefaultTextStyle(s => s.SemiBold().FontSize(8)).AlignCenter();
                                IContainer VCell(IContainer x) => x.Border(1).Padding(4).AlignCenter();
                            });
                        });
                    }

                    void TablaExtracto(IContainer container)
                    {
                        container.Border(1).Table(table =>
                        {
                            // Columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // Fecha
                                columns.RelativeColumn(3);   // Concepto
                                columns.RelativeColumn(1);   // Tipo / Valor
                                columns.RelativeColumn(1);   // Importe
                                columns.RelativeColumn(1);   // Saldo
                            });

                            // Encabezado
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("FECHA");
                                header.Cell().Element(HeaderCell).Text("CONCEPTO");
                                header.Cell().Element(HeaderCell).Text("TIPO");
                                header.Cell().Element(HeaderCell).AlignRight().Text("IMPORTE");
                                header.Cell().Element(HeaderCell).AlignRight().Text("SALDO");

                                IContainer HeaderCell(IContainer c) => c.Background(accent).Padding(6).Border(0)
                                    .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                            });

                            // Helpers de celda
                            IContainer CellPlain(IContainer c) => c.Padding(5).BorderBottom(0.5f);
                            IContainer CellBg(IContainer c, string background) => c.Background(background).Padding(5).BorderBottom(0.5f);

                            // Filas
                            string Trunc(string? s, int len)
                            {
                                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                                var v = s.Trim();
                                return v.Length <= len ? v : v.Substring(0, len - 1) + "…";
                            }

                            var odd = true;
                            if (dto.Movimientos.Count == 0)
                            {
                                // Fila placeholder cuando no hay movimientos
                                table.Cell().Element(CellPlain).Text("—");
                                table.Cell().Element(CellPlain).Text("No hay movimientos en el periodo");
                                table.Cell().Element(CellPlain).Text("—");
                                table.Cell().Element(CellPlain).AlignRight().Text("0,00");
                                table.Cell().Element(CellPlain).AlignRight().Text(dto.SaldoFinal.ToString("N2"));
                            }
                            else foreach (var m in dto.Movimientos)
                                {
                                    var bg = odd ? Colors.Grey.Lighten5 : Colors.White;
                                    odd = !odd;

                                    var importe = (m.TipoCodigo?.ToUpperInvariant() == "DEB" ? -m.Monto : m.Monto);
                                    table.Cell().Element(c => CellBg(c, bg)).Text(m.Fecha.ToString("dd.MM.yyyy"));
                                    var concepto = string.IsNullOrWhiteSpace(m.Descripcion) ? m.NumeroCuenta : m.Descripcion;
                                    table.Cell().Element(c => CellBg(c, bg)).Text(Trunc(concepto, 60));
                                    table.Cell().Element(c => CellBg(c, bg)).Text((m.TipoCodigo ?? string.Empty).ToUpperInvariant());
                                    table.Cell().Element(c => CellBg(c, bg)).AlignRight().Text(F(importe)).FontColor(importe < 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3);
                                    table.Cell().Element(c => CellBg(c, bg)).AlignRight().Text(F(m.SaldoPosterior));
                                }
                        });
                    }

                    void ResumenPie(IContainer container)
                    {
                        container.Row(row =>
                        {
                            row.RelativeItem().Background(softHeader).Padding(8).Column(c =>
                            {
                                c.Item().Text(text =>
                                {
                                    text.Span("Totales  ").SemiBold();
                                    text.Span($"Créditos: {F(dto.TotalCreditos)}    Débitos: {F(dto.TotalDebitos)}    ");
                                    text.Span($"Saldo Inicial: {F(dto.SaldoInicial)}    Saldo Final: {F(dto.SaldoFinal)}");
                                });
                            });
                        });
                    }
                });
            });

            var bytes = document.GeneratePdf();
            return Task.FromResult(bytes);
        }
    }
}
