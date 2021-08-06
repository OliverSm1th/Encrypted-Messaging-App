using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMessagePage : ContentPage
    {
        public MainMessagePage()
        {
            InitializeComponent();
        }

        public void LogOut(object sender, EventArgs e)
        {
            IAuthenticationService authenticationService = DependencyService.Resolve<IAuthenticationService>();
            bool success = authenticationService.LogOut();
            if (success)
            {
                Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        }

    }
}