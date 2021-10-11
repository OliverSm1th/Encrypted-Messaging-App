using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Xamarin.Forms;
using Encrypted_Messaging_App.Droid;
using Android.Graphics;

[assembly: Xamarin.Forms.Dependency(typeof(MessageAndroid))]
namespace Encrypted_Messaging_App.Droid
{
    public class MessageAndroid : IToastMessage
    {
        public void LongAlert(string message)
        {
            Toast toastMessage = Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long);


            toastMessage.Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
    }
}