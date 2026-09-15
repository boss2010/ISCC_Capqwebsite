import { FileBlob, SpreadsheetFile } from "@oai/artifact-tool";
import fs from "node:fs/promises";

const inputPath = "C:/Users/mohamed.lotfy/Downloads/PrivatePayments_20260809_0925.xlsx";
const input = await FileBlob.load(inputPath);
const workbook = await SpreadsheetFile.importXlsx(input);
const summary = await workbook.inspect({
  kind: "workbook,sheet,table,region",
  maxChars: 20000,
  tableMaxRows: 15,
  tableMaxCols: 20,
  tableMaxCellChars: 120,
});
console.log(summary.ndjson);
const sheet = workbook.worksheets.getItem("المدفوعات الحكومية");
const rows = sheet.getRange("A4:O413").values;
await fs.writeFile("tmp/compare_payments/excel_rows.json", JSON.stringify(rows));
