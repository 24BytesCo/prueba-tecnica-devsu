using Bancalite.Application.Config;
using Bancalite.Application.Interface;
using Bancalite.Application.Reportes.EstadoCuenta;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using System.Globalization;

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
            // Por defecto usar un azul corporativo solicitado (#095177) si no se configura
            _accent = string.IsNullOrWhiteSpace(options.Value.AccentColor) ? "#095177" : options.Value.AccentColor!;
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

                    // Determinar si el reporte es multi-cuenta
                    var cuentas = dto.Movimientos
                        .Select(m => m.NumeroCuenta)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList();
                    // Si se pidió por cliente pero solo hay una cuenta, tratamos como una sola cuenta
                    var numeroHeader = !string.IsNullOrWhiteSpace(dto.NumeroCuenta) ? dto.NumeroCuenta : (cuentas.Count == 1 ? cuentas[0] : null);
                    var esMultiCuenta = cuentas.Count > 1;

                    var drawWatermark = dto.ClienteActivo.HasValue && dto.ClienteActivo.Value == false;

                    // Encabezado estilo extracto
                    page.Header().Background(softHeader).Padding(18).Column(h =>
                    {
                        h.Item().AlignCenter().Text(esMultiCuenta ? "EXTRACTO DE CUENTAS" : "EXTRACTO DE LA CUENTA").FontSize(16);
                        // Más margen superior para la fila de datos
                        h.Item().PaddingTop(14).Row(r =>
                        {
                            r.RelativeItem().Column(col =>
                            {
                                var fechaEmision = DateTime.UtcNow.ToString("dd 'de' MMMM yyyy", culture).ToUpper(culture);
                                col.Item().PaddingBottom(4).Text(text => { text.Span("FECHA: "); text.Span(fechaEmision); });
                                if (!string.IsNullOrWhiteSpace(dto.ClienteNombre))
                                    col.Item().PaddingBottom(4).Text(text =>
                                    {
                                        text.Span("TITULAR: ");
                                        text.Span((dto.ClienteNombre ?? string.Empty).ToUpperInvariant());
                                    });
                                if (!string.IsNullOrWhiteSpace(dto.ClienteTipoDocumento))
                                    col.Item().PaddingBottom(4).Text(text => { text.Span("DOCUMENTO: "); text.Span((dto.ClienteTipoDocumento ?? string.Empty).ToUpper(culture)); });
                                if (!string.IsNullOrWhiteSpace(dto.ClienteNumeroDocumento))
                                    col.Item().PaddingBottom(4).Text(text => { text.Span("Nº: "); text.Span(dto.ClienteNumeroDocumento!); });
                                col.Item().PaddingBottom(4).Text(text => { text.Span("PERIODO: "); text.Span($"{dto.Desde:dd.MM.yyyy} - {dto.Hasta:dd.MM.yyyy}"); });
                            });
                            if (esMultiCuenta)
                            {
                                r.ConstantItem(240).Column(col => { col.Item().Element(BoxCuentas); });
                            }
                            else
                            {
                                r.ConstantItem(240).Column(col =>
                                {
                                    // Caja con desglose de código de cuenta
                                    col.Item().Element(BoxCuenta);
                                });
                            }
                        });
                        //Espacio de separación de 5px
                        h.Item().PaddingTop(5);
                        h.Item().AlignRight().Text(_brand).FontColor(lightText);
                    });

                    // Contenido principal: aplicar marca de agua sobre cada tabla cuando corresponda
                    page.Content().PaddingTop(12).Column(col =>
                    {
                        if (esMultiCuenta)
                        {
                            // Título consolidado
                            var preview = string.Join(", ", cuentas.Take(3));
                            var suffix = cuentas.Count > 3 ? ", …" : string.Empty;

                            //Espacio de separación de 20px
                            col.Item().PaddingTop(20);
                            col.Item().PaddingBottom(6)
                                .Text($"CONSOLIDADO — {cuentas.Count} CUENTAS ({preview}{suffix})");

                            // Tabla general (todas las cuentas)
                            col.Item().Element(c =>
                            {
                                if (drawWatermark)
                                {
                                    c.Layers(l =>
                                    {
                                        l.PrimaryLayer().Element(TablaGeneral);
                                        // sombra sutil para dar contorno
                                        // texto principal en rojo más tenue (más "transparente" visualmente)
                                        l.Layer().AlignCenter().AlignMiddle()
                                            .Text("USUARIO INACTIVO").FontSize(36).FontColor(Colors.Red.Lighten5);
                                    });
                                }
                                else
                                {
                                    c.Element(TablaGeneral);
                                }
                            });
                            // Totales generales inmediatamente debajo de la tabla general
                            col.Item().PaddingTop(6).Element(ResumenPie);
                            // Tablas por cuenta
                            foreach (var n in cuentas)
                            {
                                var movsCuenta = dto.Movimientos.Where(m => m.NumeroCuenta == n).ToList();
                                if (movsCuenta.Count == 0) continue;
                                col.Item().PaddingTop(28)
                                    .Text($"Cuenta {n}");
                                col.Item().Element(c =>
                                {
                                    if (drawWatermark)
                                    {
                                        c.Layers(l =>
                                        {
                                            l.PrimaryLayer().Element(cc => TablaPorCuenta(cc, movsCuenta));
                                            l.Layer().AlignCenter().AlignMiddle()
                                                .Text("USUARIO INACTIVO").FontSize(36).FontColor(Colors.Red.Lighten5);
                                        });
                                    }
                                    else
                                    {
                                        c.Element(cc => TablaPorCuenta(cc, movsCuenta));
                                    }
                                });
                                col.Item().PaddingTop(8).Element(c => SubResumenCuenta(c, movsCuenta));
                            }
                            // Totales generales ya se muestran debajo de la tabla general
                        }
                        else
                        {
                            // Tabla de movimientos con estilo de extracto
                            col.Item().Element(c =>
                            {
                                if (drawWatermark)
                                {
                                    c.Layers(l =>
                                    {
                                        l.PrimaryLayer().Element(TablaExtracto);
                                        l.Layer().AlignCenter().AlignMiddle()
                                            .Text("USUARIO INACTIVO").FontSize(36).FontColor(Colors.Red.Lighten5);
                                    });
                                }
                                else
                                {
                                    c.Element(TablaExtracto);
                                }
                            });
                            // Línea de totales y saldos al final
                            col.Item().PaddingTop(12).Element(ResumenPie);
                        }
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
                        var digits = string.IsNullOrWhiteSpace(numeroHeader) ? string.Empty : new string(numeroHeader!.Where(char.IsDigit).ToArray());
                        string GetPart(int start, int len, string def) => digits.Length >= start + len ? digits.Substring(start, len) : def;
                        var entidad = GetPart(0, 4, "0000");
                        var oficina = GetPart(4, 4, "0000");
                        var dc = GetPart(8, 2, "00");

                        container.Border(1).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text("CUENTA");
                            c.Item().Table(t =>
                            {
                                t.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(228);   // Núm. de Cuenta
                                });
                                // headers
                                t.Cell().Element(HCell).Text("Núm. de Cuenta");
                                // values

                                t.Cell().Element(VCell).Text(digits);

                                IContainer HCell(IContainer x) => x.Border(1).Padding(3).DefaultTextStyle(s => s.FontSize(8)).AlignCenter();
                                IContainer VCell(IContainer x) => x.Border(1).Padding(4).AlignCenter();
                            });
                        });
                    }

                    void BoxCuentas(IContainer container)
                    {
                        container.Border(1).Padding(6).Column(c =>
                        {
                            c.Item().AlignCenter().Text($"CUENTAS INCLUIDAS ({cuentas.Count})");
                            if (cuentas.Count == 0)
                            {
                                c.Item().AlignCenter().Text("—");
                            }
                            else
                            {
                                foreach (var n in cuentas.Take(12)) c.Item().Text(n);
                                if (cuentas.Count > 12) c.Item().Text($"… y {cuentas.Count - 12} más").FontColor(lightText);
                            }
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

                                IContainer HeaderCell(IContainer c) => c.Padding(6).Border(0)
                                    .DefaultTextStyle(x => x.FontSize(11));
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

                                    var mensaje = m.TipoCodigo?.ToUpperInvariant() == "DEB" ? "En Blanco - Débito" : "En Blanco - Crédito";

                                    var importe = (m.TipoCodigo?.ToUpperInvariant() == "DEB" ? -m.Monto : m.Monto);
                                    table.Cell().Element(c => CellBg(c, bg)).Text(m.Fecha.ToString("dd.MM.yyyy"));
                                    var concepto = string.IsNullOrWhiteSpace(m.Descripcion) ? mensaje : m.Descripcion;
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
                                    text.Span("Totales  ");
                                    text.Span($"Créditos: {F(dto.TotalCreditos)}    Débitos: {F(dto.TotalDebitos)}    ");
                                    text.Span($"Saldo Inicial: {F(dto.SaldoInicial)}    Saldo Final: {F(dto.SaldoFinal)}");
                                });
                            });
                        });
                    }

                    // Tabla general multi-cuenta (incluye columna de número de cuenta)
                    void TablaGeneral(IContainer container)
                    {
                        container.Border(1).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // Fecha
                                columns.RelativeColumn(1.4f);   // Número
                                columns.RelativeColumn(2.6f);   // Concepto
                                columns.RelativeColumn(1);   // Tipo
                                columns.RelativeColumn(1);   // Importe
                                columns.RelativeColumn(1);   // Saldo
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("FECHA");
                                header.Cell().Element(HeaderCell).Text("NÚMERO");
                                header.Cell().Element(HeaderCell).Text("CONCEPTO");
                                header.Cell().Element(HeaderCell).Text("TIPO");
                                header.Cell().Element(HeaderCell).AlignRight().Text("IMPORTE");
                                header.Cell().Element(HeaderCell).AlignRight().Text("SALDO");

                                IContainer HeaderCell(IContainer c) => c.Padding(6).Border(0)
                                    .DefaultTextStyle(x => x.FontSize(11));
                            });

                            IContainer CellPlain(IContainer c) => c.Padding(5).BorderBottom(0.5f);
                            IContainer CellBg(IContainer c, string background) => c.Background(background).Padding(5).BorderBottom(0.5f);

                            string Trunc(string? s, int len)
                            {
                                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                                var v = s.Trim();
                                return v.Length <= len ? v : v.Substring(0, len - 1) + "…";
                            }

                            var odd = true;
                            if (dto.Movimientos.Count == 0)
                            {
                                table.Cell().Element(CellPlain).Text("—");
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

                                var mensaje = m.TipoCodigo?.ToUpperInvariant() == "DEB" ? "En Blanco - Débito" : "En Blanco - Crédito";
                                var importe = (m.TipoCodigo?.ToUpperInvariant() == "DEB" ? -m.Monto : m.Monto);
                                table.Cell().Element(c => CellBg(c, bg)).Text(m.Fecha.ToString("dd.MM.yyyy"));
                                table.Cell().Element(c => CellBg(c, bg)).Text(m.NumeroCuenta ?? "");
                                var concepto = string.IsNullOrWhiteSpace(m.Descripcion) ? mensaje : m.Descripcion;
                                table.Cell().Element(c => CellBg(c, bg)).Text(Trunc(concepto, 60));
                                table.Cell().Element(c => CellBg(c, bg)).Text((m.TipoCodigo ?? string.Empty).ToUpperInvariant());
                                table.Cell().Element(c => CellBg(c, bg)).AlignRight().Text(F(importe)).FontColor(importe < 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3);
                                table.Cell().Element(c => CellBg(c, bg)).AlignRight().Text(F(m.SaldoPosterior));
                            }
                        });
                    }

                    // Tabla específica por cuenta (sin columna de número)
                    void TablaPorCuenta(IContainer container, List<EstadoCuentaItemDto> movimientos)
                    {
                        container.Border(1).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // Fecha
                                columns.RelativeColumn(2.8f);   // Concepto
                                columns.RelativeColumn(1);   // Tipo
                                columns.RelativeColumn(1);   // Importe
                                columns.RelativeColumn(1);   // Saldo
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("FECHA");
                                header.Cell().Element(HeaderCell).Text("CONCEPTO");
                                header.Cell().Element(HeaderCell).Text("TIPO");
                                header.Cell().Element(HeaderCell).AlignRight().Text("IMPORTE");
                                header.Cell().Element(HeaderCell).AlignRight().Text("SALDO");

                                IContainer HeaderCell(IContainer c) => c.Padding(6).Border(0)
                                    .DefaultTextStyle(x => x.FontSize(11));
                            });

                            IContainer CellPlain(IContainer c) => c.Padding(5).BorderBottom(0.5f);
                            IContainer CellBg(IContainer c, string background) => c.Background(background).Padding(5).BorderBottom(0.5f);

                            string Trunc(string? s, int len)
                            {
                                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                                var v = s.Trim();
                                return v.Length <= len ? v : v.Substring(0, len - 1) + "…";
                            }

                            var odd = true;
                            if (movimientos.Count == 0)
                            {
                                table.Cell().Element(CellPlain).Text("—");
                                table.Cell().Element(CellPlain).Text("Sin movimientos");
                                table.Cell().Element(CellPlain).Text("—");
                                table.Cell().Element(CellPlain).AlignRight().Text("0,00");
                                table.Cell().Element(CellPlain).AlignRight().Text("—");
                            }
                            else foreach (var m in movimientos)
                            {
                                var bg = odd ? Colors.Grey.Lighten5 : Colors.White;
                                odd = !odd;

                                var mensaje = m.TipoCodigo?.ToUpperInvariant() == "DEB" ? "En Blanco - Débito" : "En Blanco - Crédito";
                                var importe = (m.TipoCodigo?.ToUpperInvariant() == "DEB" ? -m.Monto : m.Monto);
                                table.Cell().Element(c => CellBg(c, bg)).Text(m.Fecha.ToString("dd.MM.yyyy"));
                                var concepto = string.IsNullOrWhiteSpace(m.Descripcion) ? mensaje : m.Descripcion;
                                table.Cell().Element(c => CellBg(c, bg)).Text(Trunc(concepto, 60));
                                table.Cell().Element(c => CellBg(c, bg)).Text((m.TipoCodigo ?? string.Empty).ToUpperInvariant());
                                table.Cell().Element(c => CellBg(c, bg)).AlignRight().Text(F(importe)).FontColor(importe < 0 ? Colors.Red.Darken2 : Colors.Grey.Darken3);
                                table.Cell().Element(c => CellBg(c, bg)).AlignRight().Text(F(m.SaldoPosterior));
                            }
                        });
                    }

                    // Subresumen por cuenta: créditos, débitos, saldo inicial y final
                    void SubResumenCuenta(IContainer container, List<EstadoCuentaItemDto> movimientos)
                    {
                        if (movimientos == null || movimientos.Count == 0)
                            return;
                        var totalCred = movimientos.Where(x => (x.TipoCodigo ?? string.Empty).ToUpperInvariant() == "CRE").Sum(x => x.Monto);
                        var totalDeb = movimientos.Where(x => (x.TipoCodigo ?? string.Empty).ToUpperInvariant() == "DEB").Sum(x => x.Monto);
                        var saldoIni = movimientos.First().SaldoPrevio;
                        var saldoFin = movimientos.Last().SaldoPosterior;

                        container.Row(row =>
                        {
                            row.RelativeItem().Background(softHeader).Padding(8).Column(c =>
                            {
                                c.Item().Text(text =>
                                {
                                    text.Span("Resumen cuenta  ");
                                    text.Span($"Créditos: {F(totalCred)}    Débitos: {F(totalDeb)}    ");
                                    text.Span($"Saldo Inicial: {F(saldoIni)}    Saldo Final: {F(saldoFin)}");
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
