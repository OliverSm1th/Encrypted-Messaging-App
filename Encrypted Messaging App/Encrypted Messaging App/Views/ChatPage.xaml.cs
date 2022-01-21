using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Encrypted_Messaging_App.Views.GlobalVariables;

namespace Encrypted_Messaging_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public ObservableCollection<Message> MessagesCollection { get; set; } = new ObservableCollection<Message>();


        public ChatPage()
        {
            InitializeComponent();
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine($"~~ ChatPage: {CurrentChat.id} ~~");
            Title = CurrentChat.title;
            foreach (Message message in CurrentChat.messages){
                MessagesCollection.Add(message);
            }
        }

        public void MessageTapped(object sender, EventArgs e)
        {
            Console.WriteLine("Tapped message");
        }
    }
}