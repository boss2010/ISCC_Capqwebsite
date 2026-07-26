using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using ViewModels;

namespace Capqwebsite.Services;

internal static class PaymentsExcelExporter
{
    private const int ColumnCount = 14;

    public static byte[] Create(
        IReadOnlyCollection<SuccessfulPaymentVM> payments,
        string reportTitle)
    {
        using var output = new MemoryStream();

        using (var archive = new ZipArchive(
            output,
            ZipArchiveMode.Create,
            leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", WriteContentTypes);
            WriteEntry(archive, "_rels/.rels", WriteRootRelationships);
            WriteEntry(archive, "xl/workbook.xml", WriteWorkbook);
            WriteEntry(
                archive,
                "xl/_rels/workbook.xml.rels",
                WriteWorkbookRelationships);
            WriteEntry(archive, "xl/styles.xml", WriteStyles);
            WriteEntry(
                archive,
                "xl/worksheets/sheet1.xml",
                writer => WriteWorksheet(writer, payments, reportTitle));
        }

        return output.ToArray();
    }

    private static void WriteEntry(
        ZipArchive archive,
        string path,
        Action<XmlWriter> write)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, WriterSettings());
        write(writer);
    }

    private static XmlWriterSettings WriterSettings() => new()
    {
        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        Indent = false,
        CloseOutput = false
    };

    private static void WriteContentTypes(XmlWriter writer)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement(
            "Types",
            "http://schemas.openxmlformats.org/package/2006/content-types");
        WriteDefault(writer, "rels", "application/vnd.openxmlformats-package.relationships+xml");
        WriteDefault(writer, "xml", "application/xml");
        WriteOverride(writer, "/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
        WriteOverride(writer, "/xl/worksheets/sheet1.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
        WriteOverride(writer, "/xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteDefault(
        XmlWriter writer,
        string extension,
        string contentType)
    {
        writer.WriteStartElement("Default");
        writer.WriteAttributeString("Extension", extension);
        writer.WriteAttributeString("ContentType", contentType);
        writer.WriteEndElement();
    }

    private static void WriteOverride(
        XmlWriter writer,
        string partName,
        string contentType)
    {
        writer.WriteStartElement("Override");
        writer.WriteAttributeString("PartName", partName);
        writer.WriteAttributeString("ContentType", contentType);
        writer.WriteEndElement();
    }

    private static void WriteRootRelationships(XmlWriter writer)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement(
            "Relationships",
            "http://schemas.openxmlformats.org/package/2006/relationships");
        WriteRelationship(
            writer,
            "rId1",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument",
            "xl/workbook.xml");
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteWorkbook(XmlWriter writer)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement(
            "workbook",
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        writer.WriteAttributeString(
            "xmlns",
            "r",
            null,
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
        writer.WriteStartElement("sheets");
        writer.WriteStartElement("sheet");
        writer.WriteAttributeString("name", "المدفوعات الحكومية");
        writer.WriteAttributeString("sheetId", "1");
        writer.WriteAttributeString(
            "r",
            "id",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships",
            "rId1");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteWorkbookRelationships(XmlWriter writer)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement(
            "Relationships",
            "http://schemas.openxmlformats.org/package/2006/relationships");
        WriteRelationship(
            writer,
            "rId1",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet",
            "worksheets/sheet1.xml");
        WriteRelationship(
            writer,
            "rId2",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles",
            "styles.xml");
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteRelationship(
        XmlWriter writer,
        string id,
        string type,
        string target)
    {
        writer.WriteStartElement("Relationship");
        writer.WriteAttributeString("Id", id);
        writer.WriteAttributeString("Type", type);
        writer.WriteAttributeString("Target", target);
        writer.WriteEndElement();
    }

    private static void WriteStyles(XmlWriter writer)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement(
            "styleSheet",
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

        writer.WriteStartElement("numFmts");
        writer.WriteAttributeString("count", "2");
        WriteNumberFormat(writer, 164, "dd/mm/yyyy hh:mm");
        WriteNumberFormat(writer, 165, "#,##0.00");
        writer.WriteEndElement();

        writer.WriteStartElement("fonts");
        writer.WriteAttributeString("count", "3");
        WriteFont(writer, bold: false, color: null, size: 11);
        WriteFont(writer, bold: true, color: "FFFFFFFF", size: 11);
        WriteFont(writer, bold: true, color: "FFFFFFFF", size: 16);
        writer.WriteEndElement();

        writer.WriteStartElement("fills");
        writer.WriteAttributeString("count", "4");
        WriteFill(writer, "none");
        WriteFill(writer, "gray125");
        WriteFill(writer, "solid", "FF0C6B49");
        WriteFill(writer, "solid", "FF07563D");
        writer.WriteEndElement();

        writer.WriteStartElement("borders");
        writer.WriteAttributeString("count", "2");
        WriteBorder(writer, includeLines: false);
        WriteBorder(writer, includeLines: true);
        writer.WriteEndElement();

        writer.WriteStartElement("cellStyleXfs");
        writer.WriteAttributeString("count", "1");
        WriteXf(writer, 0, 0, 0, 0);
        writer.WriteEndElement();

        writer.WriteStartElement("cellXfs");
        writer.WriteAttributeString("count", "5");
        WriteXf(writer, 0, 0, 0, 0);
        WriteXf(writer, 0, 1, 3, 1, horizontal: "center");
        WriteXf(writer, 164, 0, 0, 1, horizontal: "center");
        WriteXf(writer, 165, 0, 0, 1, horizontal: "center");
        WriteXf(writer, 0, 2, 2, 0, horizontal: "center");
        writer.WriteEndElement();

        writer.WriteStartElement("cellStyles");
        writer.WriteAttributeString("count", "1");
        writer.WriteStartElement("cellStyle");
        writer.WriteAttributeString("name", "Normal");
        writer.WriteAttributeString("xfId", "0");
        writer.WriteAttributeString("builtinId", "0");
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteNumberFormat(
        XmlWriter writer,
        int id,
        string format)
    {
        writer.WriteStartElement("numFmt");
        writer.WriteAttributeString("numFmtId", id.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("formatCode", format);
        writer.WriteEndElement();
    }

    private static void WriteFont(
        XmlWriter writer,
        bool bold,
        string? color,
        int size)
    {
        writer.WriteStartElement("font");
        if (bold)
        {
            writer.WriteElementString("b", string.Empty);
        }
        writer.WriteStartElement("sz");
        writer.WriteAttributeString("val", size.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
        if (color != null)
        {
            writer.WriteStartElement("color");
            writer.WriteAttributeString("rgb", color);
            writer.WriteEndElement();
        }
        writer.WriteStartElement("name");
        writer.WriteAttributeString("val", "Arial");
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteFill(
        XmlWriter writer,
        string patternType,
        string? foreground = null)
    {
        writer.WriteStartElement("fill");
        writer.WriteStartElement("patternFill");
        writer.WriteAttributeString("patternType", patternType);
        if (foreground != null)
        {
            writer.WriteStartElement("fgColor");
            writer.WriteAttributeString("rgb", foreground);
            writer.WriteEndElement();
            writer.WriteStartElement("bgColor");
            writer.WriteAttributeString("indexed", "64");
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteBorder(XmlWriter writer, bool includeLines)
    {
        writer.WriteStartElement("border");
        foreach (var side in new[] { "left", "right", "top", "bottom" })
        {
            writer.WriteStartElement(side);
            if (includeLines)
            {
                writer.WriteAttributeString("style", "thin");
                writer.WriteStartElement("color");
                writer.WriteAttributeString("rgb", "FFDCE7E2");
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        writer.WriteElementString("diagonal", string.Empty);
        writer.WriteEndElement();
    }

    private static void WriteXf(
        XmlWriter writer,
        int numberFormatId,
        int fontId,
        int fillId,
        int borderId,
        string? horizontal = null)
    {
        writer.WriteStartElement("xf");
        writer.WriteAttributeString("numFmtId", numberFormatId.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("fontId", fontId.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("fillId", fillId.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("borderId", borderId.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("xfId", "0");
        if (numberFormatId != 0)
        {
            writer.WriteAttributeString("applyNumberFormat", "1");
        }
        if (horizontal != null)
        {
            writer.WriteAttributeString("applyAlignment", "1");
            writer.WriteStartElement("alignment");
            writer.WriteAttributeString("horizontal", horizontal);
            writer.WriteAttributeString("vertical", "center");
            writer.WriteAttributeString("wrapText", "1");
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteWorksheet(
        XmlWriter writer,
        IReadOnlyCollection<SuccessfulPaymentVM> payments,
        string reportTitle)
    {
        var lastRow = payments.Count + 3;

        writer.WriteStartDocument();
        writer.WriteStartElement(
            "worksheet",
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

        writer.WriteStartElement("sheetViews");
        writer.WriteStartElement("sheetView");
        writer.WriteAttributeString("workbookViewId", "0");
        writer.WriteAttributeString("rightToLeft", "1");
        writer.WriteStartElement("pane");
        writer.WriteAttributeString("ySplit", "3");
        writer.WriteAttributeString("topLeftCell", "A4");
        writer.WriteAttributeString("activePane", "bottomLeft");
        writer.WriteAttributeString("state", "frozen");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("sheetFormatPr");
        writer.WriteAttributeString("defaultRowHeight", "18");
        writer.WriteEndElement();

        WriteColumns(writer);

        writer.WriteStartElement("sheetData");
        WriteRow(writer, 1, height: 28, () =>
            WriteTextCell(writer, "A1", reportTitle, style: 4));
        WriteRow(writer, 2, height: 20, () =>
            WriteTextCell(
                writer,
                "A2",
                $"تاريخ التصدير: {DateTime.Now:dd/MM/yyyy hh:mm tt} — عدد العمليات: {payments.Count}",
                style: 0));

        var headers = new[]
        {
            "رقم العملية", "رقم الطلب", "تاريخ العملية", "تاريخ الدفع",
            "الاسم / الشركة", "الرقم القومي", "المكتب",
            "رقم الشهادة / الفحص", "السجل الضريبي", "السجل التجاري",
            "اسم المحطة", "تفاصيل الرسوم", "الإجمالي", "كود البنك"
        };
        WriteRow(writer, 3, height: 25, () =>
        {
            for (var column = 0; column < headers.Length; column++)
            {
                WriteTextCell(
                    writer,
                    $"{ColumnName(column + 1)}3",
                    headers[column],
                    style: 1);
            }
        });

        var rowNumber = 4;
        foreach (var payment in payments)
        {
            var currentRow = rowNumber++;
            WriteRow(writer, currentRow, height: 34, () =>
            {
                WriteNumberCell(writer, $"A{currentRow}", payment.ID, 0);
                WriteTextCell(writer, $"B{currentRow}", payment.OrderNumber, 0);
                WriteDateCell(writer, $"C{currentRow}", payment.CreationDate);
                WriteTextCell(
                    writer,
                    $"D{currentRow}",
                    payment.PaymentDate?.ToString("dd/MM/yyyy"),
                    0);
                WriteTextCell(writer, $"E{currentRow}", payment.Name, 0);
                WriteTextCell(writer, $"F{currentRow}", payment.NationalID, 0);
                WriteTextCell(writer, $"G{currentRow}", payment.Office, 0);
                WriteTextCell(writer, $"H{currentRow}", payment.CustomsCertificateNumber, 0);
                WriteTextCell(writer, $"I{currentRow}", payment.TaxRegistry, 0);
                WriteTextCell(writer, $"J{currentRow}", payment.CommercialRegister, 0);
                WriteTextCell(writer, $"K{currentRow}", payment.FarmName, 0);
                WriteTextCell(
                    writer,
                    $"L{currentRow}",
                    string.Join(
                        " | ",
                        payment.Details.Select(detail =>
                            $"{detail.FeesTypeName ?? "رسم"}: {detail.Amount ?? 0:N2} ج.م")),
                    0);
                WriteNumberCell(writer, $"M{currentRow}", payment.TotalAmount, 3);
                WriteTextCell(writer, $"N{currentRow}", payment.BankCode, 0);
            });
        }

        writer.WriteEndElement();

        writer.WriteStartElement("autoFilter");
        writer.WriteAttributeString("ref", $"A3:N{Math.Max(3, lastRow)}");
        writer.WriteEndElement();

        writer.WriteStartElement("mergeCells");
        writer.WriteAttributeString("count", "2");
        WriteMergedCell(writer, "A1:N1");
        WriteMergedCell(writer, "A2:N2");
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteColumns(XmlWriter writer)
    {
        var widths = new double[]
        {
            13, 18, 19, 14, 28, 18, 24, 23, 18, 18, 22, 48, 15, 12
        };
        writer.WriteStartElement("cols");
        for (var index = 0; index < widths.Length; index++)
        {
            writer.WriteStartElement("col");
            writer.WriteAttributeString("min", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("max", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", widths[index].ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("customWidth", "1");
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteRow(
        XmlWriter writer,
        int rowNumber,
        double height,
        Action writeCells)
    {
        writer.WriteStartElement("row");
        writer.WriteAttributeString("r", rowNumber.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("ht", height.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("customHeight", "1");
        writeCells();
        writer.WriteEndElement();
    }

    private static void WriteTextCell(
        XmlWriter writer,
        string reference,
        string? value,
        int style)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("s", style.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("t", "inlineStr");
        writer.WriteStartElement("is");
        writer.WriteStartElement("t");
        writer.WriteAttributeString("xml", "space", null, "preserve");
        writer.WriteString(value ?? string.Empty);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteNumberCell(
        XmlWriter writer,
        string reference,
        decimal value,
        int style)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("s", style.ToString(CultureInfo.InvariantCulture));
        writer.WriteElementString("v", value.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
    }

    private static void WriteDateCell(
        XmlWriter writer,
        string reference,
        DateTime value)
    {
        writer.WriteStartElement("c");
        writer.WriteAttributeString("r", reference);
        writer.WriteAttributeString("s", "2");
        writer.WriteElementString(
            "v",
            value.ToOADate().ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
    }

    private static void WriteMergedCell(XmlWriter writer, string reference)
    {
        writer.WriteStartElement("mergeCell");
        writer.WriteAttributeString("ref", reference);
        writer.WriteEndElement();
    }

    private static string ColumnName(int column)
    {
        var name = string.Empty;
        while (column > 0)
        {
            column--;
            name = (char)('A' + column % 26) + name;
            column /= 26;
        }
        return name;
    }
}
