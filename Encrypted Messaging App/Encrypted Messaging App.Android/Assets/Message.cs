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
using Xamarin.Forms;
using Encrypted_Messaging_App.Droid.Assets;

[assembly: Dependency(typeof(MessageAndroid))]
namespace Encrypted_Messaging_App.Droid.Assets
{
    public class MessageAndroid : IToastMessage
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long);
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long);
        }
    }
}