using System.Windows;
using com.tomsawyer.licensing;

namespace DataModeler.Wpf
{
    public partial class App : Application
    {
        private const string LicenseProtocol = "https";
        private const string LicenseHost = "server.licensing.tomsawyer.com";
        private const int LicensePort = 443;
        private const string LicensePath = "WRUV5HNWN0TLGS53WJ4E00STO";
        private const string LicenseName = "Quest Software, Evaluation 9536, Tom Sawyer Version 9.2, Development Distribution";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeTomSawyerLicense();

            MainWindow window = new MainWindow();
            MainWindow = window;
            window.Show();
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
