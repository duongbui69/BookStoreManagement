import re

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Forms/MainForm.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Add cache dictionary and rewrite LoadControl
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

            if (lblPlaceholder != null)
            {
                lblPlaceholder.Text = placeholder;
            }
            
            if (controlToLoad is BookStoreManagement.Interfaces.IRefreshable refreshable)
            {
                await refreshable.RefreshDataAsync();
            }
        }"""

text = re.sub(r'private void LoadControl\(Control newControl, string placeholder\).*?lblPlaceholder\.Text = placeholder;\s*\}', cache_dict, text, flags=re.DOTALL)

# 2. Replace all instances of LoadControl(new UserControls.XYZControl(), "...") with LoadControl<UserControls.XYZControl>("...")
text = re.sub(r'LoadControl\(new (UserControls\.\w+)\(\), "(.*?)"\);', r'LoadControl<\1>("\2");', text)

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Forms/MainForm.cs', 'w', encoding='utf-8') as f:
    f.write(text)

