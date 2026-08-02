import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Forms/MainForm.cs', 'r', encoding='utf-8') as f:
    text = f.read()

cache_dict = """        private Dictionary<Type, Control> _controlCache = new Dictionary<Type, Control>();

        private async void LoadControl<T>(string placeholder) where T : Control, new()
        {
            Type type = typeof(T);
            
            if (CurrentSession.IsStaff && type != typeof(UserControls.StaffMyShiftsControl))
            {
                var shiftService = new BookStoreManagement.Services.ShiftService();
                if (shiftService.GetActiveShift() == null)
                {
                    MessageBox.Show("Vui lòng bắt đầu ca làm việc trước khi thực hiện các thao tác khác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    type = typeof(UserControls.StaffMyShiftsControl);
                    placeholder = "Ca của tôi";
                }
            }

            if (!_controlCache.ContainsKey(type))
            {
                Control newControl = (Control)Activator.CreateInstance(type);
                newControl.Dock = DockStyle.Fill;
                _controlCache[type] = newControl;
            }

            Control controlToLoad = _controlCache[type];

            // Don't dispose existing controls, just remove them from visual tree
            panelMain.Controls.Clear();
            panelMain.Controls.Add(controlToLoad);
            
            if (controlToLoad is BookStoreManagement.Interfaces.IRefreshable refreshable)
            {
                await refreshable.RefreshDataAsync();
            }
        }"""

text = re.sub(r'private void LoadControl\(Control newControl, string placeholder\).*?panelMain\.Controls\.Add\(newControl\);\s*\}', cache_dict, text, flags=re.DOTALL)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Forms/MainForm.cs', 'w', encoding='utf-8') as f:
    f.write(text)
