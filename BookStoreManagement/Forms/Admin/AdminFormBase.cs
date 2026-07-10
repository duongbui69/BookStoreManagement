using System;
using System.Drawing;
using System.Windows.Forms;

namespace BookStoreManagement.Forms.Admin
{
    public class AdminFormBase : Form
    {
        protected DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White
            };
            return grid;
        }

        protected TextBox CreateTextBox(Control parent, string label, int left, int top, int width = 220)
        {
            var lbl = new Label { Text = label, Left = left, Top = top + 4, Width = 120 };
            var txt = new TextBox { Left = left + 125, Top = top, Width = width };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        protected ComboBox CreateComboBox(Control parent, string label, int left, int top, int width = 220)
        {
            var lbl = new Label { Text = label, Left = left, Top = top + 4, Width = 120 };
            var cbo = new ComboBox { Left = left + 125, Top = top, Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            parent.Controls.Add(lbl);
            parent.Controls.Add(cbo);
            return cbo;
        }

        protected CheckBox CreateCheckBox(Control parent, string label, int left, int top)
        {
            var chk = new CheckBox { Text = label, Left = left, Top = top, Width = 160, Checked = true };
            parent.Controls.Add(chk);
            return chk;
        }

        protected Button CreateButton(Control parent, string text, int left, int top, EventHandler click, int width = 100)
        {
            var btn = new Button { Text = text, Left = left, Top = top, Width = width, Height = 30 };
            btn.Click += click;
            parent.Controls.Add(btn);
            return btn;
        }

        protected int GetSelectedId(DataGridView grid)
        {
            if (grid.CurrentRow == null) return 0;
            object? value = grid.CurrentRow.Cells["Id"].Value;
            return value == null ? 0 : Convert.ToInt32(value);
        }

        protected int ToInt(string text)
        {
            return int.TryParse(text, out int value) ? value : 0;
        }

        protected int? ToNullableInt(string text)
        {
            return int.TryParse(text, out int value) ? value : null;
        }

        protected decimal ToDecimal(string text)
        {
            return decimal.TryParse(text, out decimal value) ? value : 0;
        }

        protected decimal? ToNullableDecimal(string text)
        {
            return decimal.TryParse(text, out decimal value) ? value : null;
        }
    }
}
