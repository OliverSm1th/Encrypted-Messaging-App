using Encrypted_Messaging_App.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Encrypted_Messaging_App
{
    public partial class App : Application
    {
        public User CurrentUser;
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public interface IFirebaseAuthenticator
        {

        }
    }
}

