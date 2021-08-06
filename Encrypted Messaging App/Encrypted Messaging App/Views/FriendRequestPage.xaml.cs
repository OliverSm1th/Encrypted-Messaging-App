using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.ComponentModel;

using static Encrypted_Messaging_App.Views.GlobalVariables;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendRequestPage : ContentPage
    {
        public FriendRequestPage()
        {
            InitializeComponent();
            if (CurrentUser.FriendRequestsUpdated)
            {
                DisplayRequests(CurrentUser.FriendRequests);
            }
        }
        public void DisplayRequests(Request[] requests)
        {
            Console.WriteLine("Displaying Requests");
            //DataTemplate template = (DataTemplate)Content.FindByName("userTemplate");
            StackLayout userStack = (StackLayout) Content.FindByName("UserStack");
            if (requests.Length != 0) // No requests, delete grid
            {
                //for (int i = 0; i < requests.Length; i++)
                //{
                //    User user = requests[i].user;
                //    if (i != 0)
                //    {

                //    }
                //}

                ListView users = new ListView { ItemTemplate = (DataTemplate) Resources["userTemplate"] };
                users.ItemsSource = new string[] { "User 1", "User 2", "User 3" };
                Console.WriteLine("Set Item Source");
                userStack = new StackLayout
                {
                    Children = { users }
                };
            }
            
        }

    }
}