using System;
using System.IO;
using System.Text.RegularExpressions;

string dir = @"C:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\UserControls";
string[] files = Directory.GetFiles(dir, "*.cs");

foreach (var file in files)
{
    if (file.EndsWith(".Designer.cs"))
    {
        if (file.Contains("DashboardControl.Designer.cs"))
        {
            string content = File.ReadAllText(file);
            if (!content.Contains("ThemeManager.ThemeChanged -="))
            {
                content = content.Replace("if (disposing && (components != null))", "if (disposing)\n            {\n                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;\n            }\n            if (disposing && (components != null))");
                File.WriteAllText(file, content);
                Console.WriteLine("Patched DashboardControl.Designer.cs");
            }
        }
        continue;
    }

    string text = File.ReadAllText(file);
    if (text.Contains("ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;") && !text.Contains("protected override void Dispose(bool disposing)"))
    {
        // Find the last closing brace of the class
        int lastBrace = text.LastIndexOf('}');
        if (lastBrace > 0)
        {
            int secondLastBrace = text.LastIndexOf('}', lastBrace - 1);
            if (secondLastBrace > 0)
            {
                string disposeMethod = @"
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
";
                text = text.Insert(secondLastBrace, disposeMethod);
                File.WriteAllText(file, text);
                Console.WriteLine($"Patched {Path.GetFileName(file)}");
            }
        }
    }
}
