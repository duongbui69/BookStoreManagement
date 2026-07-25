using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace BookStoreManagement.Services
{
    public class ExcelExportService : ServiceBase
    {
        public void ExportDataTable(DataTable dataTable, string filePath, string sheetName = "Data")
        {
            PermissionService.RequireStaffOrAdmin();
            Require(dataTable != null, "No data to export to Excel.");
            Require(!string.IsNullOrWhiteSpace(filePath), "Invalid Excel file path.");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Data" : sheetName);
            worksheet.Cell(1, 1).InsertTable(dataTable);
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        public void ExportDataGridView(DataGridView dataGridView, string filePath, string sheetName = "Data")
        {
            PermissionService.RequireStaffOrAdmin();
            Require(dataGridView != null, "No data table to export to Excel.");
            Require(!string.IsNullOrWhiteSpace(filePath), "Invalid Excel file path.");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Data" : sheetName);

            int visibleColumnIndex = 1;
            for (int col = 0; col < dataGridView.Columns.Count; col++)
            {
                if (!dataGridView.Columns[col].Visible) continue;
                worksheet.Cell(1, visibleColumnIndex).Value = dataGridView.Columns[col].HeaderText;
                visibleColumnIndex++;
            }

            int excelRow = 2;
            for (int row = 0; row < dataGridView.Rows.Count; row++)
            {
                if (dataGridView.Rows[row].IsNewRow) continue;

                visibleColumnIndex = 1;
                for (int col = 0; col < dataGridView.Columns.Count; col++)
                {
                    if (!dataGridView.Columns[col].Visible) continue;
                    worksheet.Cell(excelRow, visibleColumnIndex).Value = dataGridView.Rows[row].Cells[col].Value?.ToString() ?? string.Empty;
                    visibleColumnIndex++;
                }
                excelRow++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
    }
}
