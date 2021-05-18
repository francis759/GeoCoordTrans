using GeoCoordTrans_v2._1.Properties;
using System;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    static class Program
    {
        internal static readonly string ProductKey = "Y2UC0-49BD1-ADCDE-445CE-A3MM7";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Secure scr = new Secure();
            bool Logic = scr.Algorithm(ProductKey, Settings.Default.Protection);
            if (Logic == true)
                Application.Run(new mainform());
        }
    }
}
