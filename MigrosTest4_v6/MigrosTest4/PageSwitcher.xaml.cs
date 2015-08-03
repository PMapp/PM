using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Threading;
using Microsoft.Owin.Hosting;
using System.Resources;


namespace MigrosTest4
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PageSwitcher : Window
    {
        private IDisposable app = null;
        
        
        public PageSwitcher()
        {
            InitializeComponent();

            Switcher.pageSwitcher = this;
            Switcher.Switch(0);            
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }

        public void Navigate(UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;

            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! "
                  + nextPage.Name.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Web API'nin dışarıdan gelen istekleri de kabul etmesi için +:portNo şeklinde yazmak gerekiyor
            string baseUri = "http://+:1903/";

            app = WebApp.Start<Startup>(url: baseUri); // Web API başlatılıyor

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (app != null)
            {
                app.Dispose(); // Pencere kapanırken Web API'yi dispose ediyor
            }
        }
    }
}
