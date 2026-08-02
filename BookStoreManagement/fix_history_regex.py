import os
import re

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls\StaffMyShiftsControl.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

pattern = r"private void RenderHistoryPage\(\)[\s\S]*?(?=\s+private void ThemeManager_ThemeChanged|\s+private void DgvHistory_CellPainting|\s+\})"

new_render = """private void RenderHistoryPage()
        {
            try
            {
                if (dgvHistory.Columns.Count == 0)
                {
                    dgvHistory.Columns.Add("Id", "ID");
                    dgvHistory.Columns["Id"].Width = 80;
                    dgvHistory.Columns.Add("Ngày", "Ngày");
                    dgvHistory.Columns.Add("ShiftName", "Ca");
                    dgvHistory.Columns.Add("Thời gian", "Bắt đầu - Kết thúc");
                    dgvHistory.Columns.Add("Revenue", "Doanh số (đ)");
                    dgvHistory.Columns["Revenue"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                    dgvHistory.Columns.Add("Trạng thái", "Trạng thái");
                    dgvHistory.Columns["Trạng thái"].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                    
                    var actionCol = new System.Windows.Forms.DataGridViewTextBoxColumn
                    {
                        Name = "Thao tác",
                        HeaderText = "Thao tác",
                        Width = 100,
                        DefaultCellStyle = { Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter }
                    };
                    dgvHistory.Columns.Add(actionCol);
                }

                dgvHistory.Rows.Clear();
                if (_shiftHistory == null || _shiftHistory.Count == 0)
                    return;

                foreach (var item in pageItems)
                {
                    string timeStr = $"{item.StartTime:HH:mm} - {(item.EndTime.HasValue ? item.EndTime.Value.ToString("HH:mm") : "Đang mở")}";
                    
                    int rowIndex = dgvHistory.Rows.Add(
                        "#" + item.Id,
                        item.StartTime.ToString("dd/MM/yyyy"),
                        item.ShiftName,
                        timeStr,
                        item.Revenue.ToString("N0") + " đ",
                        item.Status == "Đã đóng" ? "Đã đóng" : "Đang mở",
                        "Chi tiết"
                    );
                    dgvHistory.Rows[rowIndex].Tag = item;
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Lỗi RenderHistoryPage", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }"""

match = re.search(pattern, repo_content)
if match:
    repo_content = repo_content[:match.start()] + new_render + repo_content[match.end():]
    with open(repo_path, 'w', encoding='utf-8') as f:
        f.write(repo_content)
    print("Replaced RenderHistoryPage in StaffMyShiftsControl")
else:
    print("Could not match RenderHistoryPage")
