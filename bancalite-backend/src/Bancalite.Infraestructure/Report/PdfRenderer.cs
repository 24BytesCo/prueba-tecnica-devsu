using System.Text;
using Bancalite.Application.Interface;
using Bancalite.Application.Reportes.EstadoCuenta;

namespace Bancalite.Infraestructure.Report
{
    /// <summary>
    /// Renderizador PDF mínimo (plantilla simple) sin dependencias externas.
    /// - Cabecera por página
    /// - Tabla de movimientos paginada
    /// - Totales
    /// </summary>
    public class PdfRenderer : IPdfRenderer
    {
        public Task<byte[]> RenderEstadoCuentaAsync(object reporteDto, CancellationToken ct = default)
        {
            var dto = (EstadoCuentaDto)reporteDto;

            // Construir líneas de texto (cabecera + detalle + totales)
            var headerLines = new List<string>
            {
                "Reporte Estado de Cuenta",
                !string.IsNullOrWhiteSpace(dto.ClienteNombre) ? $"Cliente: {dto.ClienteNombre}" : null,
                !string.IsNullOrWhiteSpace(dto.NumeroCuenta) ? $"Cuenta: {dto.NumeroCuenta}" : null,
                $"Periodo: {dto.Desde:yyyy-MM-dd} a {dto.Hasta:yyyy-MM-dd}",
                $"Totales: Créditos {dto.TotalCreditos:N2} | Débitos {dto.TotalDebitos:N2}",
                $"Saldo Inicial: {dto.SaldoInicial:N2} | Saldo Final: {dto.SaldoFinal:N2}",
                string.Empty,
                "Fecha | Cuenta | Tipo | Monto | SaldoPrevio | SaldoPosterior | Desc"
            }.Where(s => s != null)!.ToList()!;

            var detailLines = dto.Movimientos
                .Select(m => $"{m.Fecha:yyyy-MM-dd} | {m.NumeroCuenta} | {m.TipoCodigo} | {m.Monto:N2} | {m.SaldoPrevio:N2} | {m.SaldoPosterior:N2} | {m.Descripcion}")
                .ToList();

            var pages = BuildPages(headerLines, detailLines, linesPerPage: 40);

            // Ensamblar PDF 1.4 con múltiples páginas (xref mínimo)
            var sbPdf = new StringBuilder();
            sbPdf.AppendLine("%PDF-1.4");

            var objIndex = 1;
            var catalogId = objIndex++; // 1
            var pagesId = objIndex++;   // 2

            // Reservar ids de page y contents
            var pageIds = new List<int>();
            var contentIds = new List<int>();
            foreach (var _ in pages)
            {
                pageIds.Add(objIndex++);     // page obj
                contentIds.Add(objIndex++);  // contents obj
            }
            var fontId = objIndex++; // font obj

            // Catalog
            sbPdf.AppendLine($"{catalogId} 0 obj <</Type /Catalog /Pages {pagesId} 0 R>> endobj");
            // Pages root
            var kids = string.Join(" ", pageIds.Select(id => $"{id} 0 R"));
            sbPdf.AppendLine($"{pagesId} 0 obj <</Type /Pages /Kids [{kids}] /Count {pages.Count}>> endobj");

            // Pages and contents
            for (int i = 0; i < pages.Count; i++)
            {
                var pageObj = pageIds[i];
                var contObj = contentIds[i];
                var content = BuildContentStream(pages[i]);
                var contentBytes = Encoding.ASCII.GetBytes(content);
                sbPdf.AppendLine($"{pageObj} 0 obj <</Type /Page /Parent {pagesId} 0 R /MediaBox [0 0 612 792] /Resources <</Font <</F1 {fontId} 0 R>>>> /Contents {contObj} 0 R>> endobj");
                sbPdf.AppendLine($"{contObj} 0 obj <</Length {contentBytes.Length}>> stream");
                sbPdf.Append(content);
                sbPdf.AppendLine("\nendstream endobj");
            }

            // Font (Helvetica)
            sbPdf.AppendLine($"{fontId} 0 obj <</Type /Font /Subtype /Type1 /BaseFont /Helvetica>> endobj");

            // xref mínimo (no offsets reales), suficiente para la mayoría de viewers en pruebas
            var final = sbPdf.ToString() + "\nxref\n0 1\n0000000000 65535 f \ntrailer <</Size 1/Root 1 0 R>>\nstartxref\n0\n%%EOF\n";
            return Task.FromResult(Encoding.ASCII.GetBytes(final));
        }

        private static string EscapePdf(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        private static List<List<string>> BuildPages(List<string> headerLines, List<string> detailLines, int linesPerPage)
        {
            var pages = new List<List<string>>();
            var headerCount = headerLines.Count;
            var detailCapacity = Math.Max(1, linesPerPage - headerCount);
            for (int i = 0; i < detailLines.Count; i += detailCapacity)
            {
                var page = new List<string>();
                page.AddRange(headerLines);
                page.AddRange(detailLines.Skip(i).Take(detailCapacity));
                pages.Add(page);
            }
            if (pages.Count == 0)
            {
                pages.Add(headerLines);
            }
            return pages;
        }

        private static string BuildContentStream(List<string> lines)
        {
            var sb = new StringBuilder();
            sb.Append("BT /F1 10 Tf 50 760 Td 14 TL\n"); // Begin text, set font, move start, leading
            foreach (var l in lines)
            {
                sb.Append($"({EscapePdf(l)}) Tj\nT*\n"); // Show text, next line
            }
            sb.Append("ET"); // End text
            return sb.ToString();
        }
    }
}
