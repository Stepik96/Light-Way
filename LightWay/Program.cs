using System;
using System.Windows.Forms;

namespace LightWay
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаем главное меню — оно само откроет GameForm
            Application.Run(new MenuForm());
        }
    }
}
