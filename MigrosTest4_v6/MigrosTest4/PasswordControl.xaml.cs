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

using MigrosTest4.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;


namespace MigrosTest4
{
	/// <summary>
	/// Interaction logic for PasswordControl.xaml
	/// </summary>
    public partial class PasswordControl : UserControl, ISwitchable
	{
		/* PhoneControl'den tek farkı TextBox (PasswordBox) ile iletişimin 
		passwordTextBox.Text yerine passwordTextBox.Password ile sağlanmasıdır*/

        
		public PasswordControl()
		{  // public static ResourceManager ResFile = new ResourceManager("Resources",typeof(Switcher).Assembly);
			InitializeComponent();
		}
        

		private void key_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
            
			if (button.Content.ToString().Equals("Sil"))
			{
                if (passwordTextBox.Password.Length > 0)
                {
                    passwordTextBox.Password = passwordTextBox.Password.Remove(passwordTextBox.Password.Length - 1);

                }
                else
                {
                    Switcher.Switch(0); 
                   
                }
			}
			else
			{
				if (passwordTextBox.Password.Length < 10)
				{
					passwordTextBox.Password += button.Content.ToString();
				}

                if (passwordTextBox.Password.Length == 10)
                {

                    this.onayButton_Click(sender, e);
                    
                    
                }
			}
		}

		private async void onayButton_Click(object sender, RoutedEventArgs e)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("http://mobileservice.migros.com.tr/");
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				try
				{
					var req = new Request();

					/*PhoneControl'den farklı olarak PasswordBox'a girilen karakterler req objesinin
					password değişkenine atanmıştır.*/
					req.requestHeader = new RequestHeader() { userName = "Pavotek", password = passwordTextBox.Password, serviceName = "sifreKontrol" };
					req.requestBody = new Sifre.SifreRequestBody() { sifre = "ABCD1234" };
                    passwordTextBox.Password = "";
					var response = await client.PostAsJsonAsync("droppointpavoapi/api/genericmethod", req);

					if (response.IsSuccessStatusCode)
					{
						string data = await response.Content.ReadAsStringAsync();

						var results = JsonConvert.DeserializeObject<dynamic>(data);
						string apiResultCode = results.responseHeader.resultCode;						

						if (apiResultCode == "BASARILI")
						{

							this.Content = new Result();
						}

						else
						{
							MessageBox.Show("Hatalı Şifre!");
                            
                            
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}	
		}

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion
	}


}
