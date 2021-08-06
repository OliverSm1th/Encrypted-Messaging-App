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
using Encrypted_Messaging_App.Droid;
using System.Threading.Tasks;
using Encrypted_Messaging_App.Models;
using Firebase.Auth;
using Firebase.Firestore;
//using Google.Cloud.Firestore;
using Firebase.Firestore;
using Android.Gms.Extensions;
using Encrypted_Messaging_App.Droid.Resources;

[assembly: Dependency(typeof(Encrypted_Messaging_App.Droid.UserManager))]


namespace Encrypted_Messaging_App.Droid
{

    class UserManager : IManageUserService
    {
        public Task<CUser> GetUser()
        {
            var tcs = new TaskCompletionSource<CUser>();

            //https://www.youtube.com/watch?v=GNdRrV9Re4A
            
            
            FirebaseFirestore.Instance.Collection("users").Document(FirebaseAuth.Instance.CurrentUser.Uid).Get().AddOnCompleteListener(new OnCompleteListener(tcs, FirebaseAuth.Instance.CurrentUser.DisplayName));


            Task<CUser> result = tcs.Task; 
                

            return result;




            /*FirestoreDb db = FirestoreDb.Create("https://messaging-app-demo-348e5-default-rtdb.europe-west1.firebasedatabase.app/");
            CollectionReference userRef = db.Collection("users");
            Console.WriteLine("Awaiting user info");
            DocumentSnapshot document = await userRef.Document(FirebaseAuth.Instance.CurrentUser.Uid).GetSnapshotAsync();
            Console.WriteLine("Recieved information");
            User currentUser = new User(FirebaseAuth.Instance.CurrentUser.Uid);
            currentUser.Fill(document);
            return currentUser;*/
        }
    }
}