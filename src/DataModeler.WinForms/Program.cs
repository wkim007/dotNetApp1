using System;
using System.Windows.Forms;
using com.tomsawyer.licensing;

namespace DataModeler.WinForms
{
    internal static class Program
    {
        private const string LicenseProtocol = "https";
        private const string LicenseHost = "server.licensing.tomsawyer.com";
        private const int LicensePort = 443;
        private const string LicensePath = "WRUV5HNWN0TLGS53WJ4E00STO";
        private const string LicenseName = "Quest Software, Evaluation 9536, Tom Sawyer Version 9.2, Development Distribution";

        [STAThread]
        private static void Main()
        {
            InitializeTomSawyerLicense();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void InitializeTomSawyerLicense()
        {
            TSNLicenseManager.setUserName("Woo Kim");
            TSNLicenseManager.initTSSLicensing(
                LicenseProtocol,
                LicenseHost,
                LicensePort,
                LicensePath,
                LicenseName);
        }
    }
}
