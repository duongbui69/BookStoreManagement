using System;
using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace BookStoreManagement.Services
{
    public class ExcelExportService
    {
        public void ExportDataTable(DataTable dataTable, string filePath, string sheetName = "Data")
        {
            if (dataTable == null)
            {
                throw new Exception("Không có dữ liệu để xuất Excel.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("Đường dẫn file Excel không hợp lệ.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).InsertTable(dataTable);
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        public void ExportDataGridView(DataGridView dataGridView, string filePath, string sheetName = "Data")
        {
            if (dataGridView == null)
            {
                throw new Exception("Không có bảng dữ liệu để xuất Excel.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("Đường dẫn file Excel không hợp lệ.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            int visibleColumnIndex = 1;
            for (int col = 0; col < dataGridView.Columns.Count; col++)
            {
                if (!dataGridView.Columns[col].Visible)
                {
                    continue;
                }

                worksheet.Cell(1, visibleColumnIndex).Value = dataGridView.Columns[col].HeaderText;
                visibleColumnIndex++;
            }

            int excelRow = 2;
            for (int row = 0; row < dataGridView.Rows.Count; row++)
            {
                if (dataGridView.Rows[row].IsNewRow)
                {
                    continue;
                }

                int excelCol = 1;
                for (int col = 0; col < dataGridView.Columns.Count; col++)
                {
                    if (!dataGridView.Columns[col].Visible)
                    {
                        continue;
                    }

                    worksheet.Cell(excelRow, excelCol).Value = dataGridView.Rows[row].Cells[col].Value?.ToString() ?? string.Empty;
                    excelCol++;
                }

                excelRow++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
    }
}
