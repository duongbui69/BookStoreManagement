using System.Reflection;
using System.Windows.Forms;

namespace BookStoreManagement.Helpers
{
    public static class DataGridViewExtensions
    {
        public static void SetDoubleBuffered(this DataGridView dgv, bool setting)
        {
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dgv,
                new object[] { setting });
        }
    }
}
