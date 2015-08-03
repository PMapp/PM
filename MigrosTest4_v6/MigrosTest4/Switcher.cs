using System.Windows.Controls;
using System.Globalization;
using System.Threading;


using Microsoft.Owin.Hosting;
namespace MigrosTest4
{
  	public static class Switcher
  	{
    	public static PageSwitcher pageSwitcher;
        public static PasswordControl PasswordControlIns = new PasswordControl();
        public static MainWindow MainWindowIns = new MainWindow();

        
            
        public static void Switch(int Index)
        {

            switch (Index)
            {
                 
                

                case 0: pageSwitcher.Navigate(MainWindowIns); break;
                case 1: pageSwitcher.Navigate(PasswordControlIns); break;

            }
            
        }
        public static void Switch(UserControl newPage)
    	{
      		pageSwitcher.Navigate(newPage);
    	}

    	public static void Switch(UserControl newPage, object state)
    	{
      		pageSwitcher.Navigate(newPage, state);
    	}
  	}
}
