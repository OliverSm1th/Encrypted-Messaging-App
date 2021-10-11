using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Gms.Tasks;
using Firebase.Firestore;
using System.Reflection;
using Java.Util;
using static Encrypted_Messaging_App.Views.GlobalVariables;
using System.Numerics;
using Firebase.Auth;

namespace Encrypted_Messaging_App.Droid.Resources
{
    class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        private TaskCompletionSource<(bool, object)> _tcs;
        private string Type;
        private string Username;
        private CUser currentUser;

        ListenerHelper Helper = new ListenerHelper();


        public OnCompleteListener(TaskCompletionSource<(bool, object)> tcs, string type, string username = "")
        {
            _tcs = tcs;
            Type = type;
            Username = username;
            if(CurrentUser != null)
            {
                currentUser = CurrentUser;
            }
        }

        // When get() request is completed
        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (task.IsSuccessful)
            {
                var result = task.Result;
                // Documents:
                if (result is DocumentSnapshot doc)
                {
                    if(doc.Data == null)
                    {
                        Console.WriteLine($"No data found at doc for: {Type}");
                        _tcs.TrySetResult((false, "No data found"));
                        return;
                    }

                    if (Type == "CUser")
                    {
                        _tcs.TrySetResult(Helper.ParseCUser(doc));
                    }
                    else if (Type == "UserFromUsername")
                    {
                        _tcs.TrySetResult(Helper.ParseUser_Username(doc));
                    }
                    else if(Type == "UserFromId")
                    {
                        _tcs.TrySetResult(Helper.ParseUser_Id(doc));
                    }
                    else
                    {
                        Console.WriteLine("Invalid Type for Document provided");
                        _tcs.TrySetResult((false, "Invalid Type provided (document)"));
                    }
                }
                // Collections:
                else if(result is QuerySnapshot collection)
                {
                    DocumentSnapshot[] docs = collection.Documents.ToArray();
                    if (Type == "Requests")
                    {
                        _tcs.TrySetResult(Helper.ParseRequests(docs));
                    }
                    else
                    {
                        Console.WriteLine("Invalid Type for Collection provided");
                        _tcs.TrySetResult((false, "Invalid Type provided (collection)"));
                    }
                }
                else
                {
                    Console.WriteLine("result isn't a document snapshot");
                    _tcs.TrySetResult((false, "Path isn't valid"));
                }
            }
            else
            {
                Console.WriteLine("Task unsuccessful, setting default result");
                _tcs.TrySetResult((false, "Task Invalid"));
            }
        }
    }
}