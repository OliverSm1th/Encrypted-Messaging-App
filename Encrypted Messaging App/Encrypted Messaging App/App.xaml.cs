using Encrypted_Messaging_App.Views;
using System;
using System.Collections.Generic;
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
        { Functions.setColour("Blue"); }
    }
}

