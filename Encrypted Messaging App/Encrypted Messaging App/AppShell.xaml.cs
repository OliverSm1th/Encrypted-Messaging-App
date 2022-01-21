using Encrypted_Messaging_App.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Encrypted_Messaging_App
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute(nameof(MainMessagePage), typeof(MainMessagePage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(LoadingPage), typeof(LoadingPage));
            Routing.RegisterRoute(nameof(FriendRequestPage), typeof(FriendRequestPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));


        }

    }
}
