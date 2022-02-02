using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private Type ReturnType;

        ListenerHelper Helper = new ListenerHelper();


        public OnCompleteListener(TaskCompletionSource<(bool, object)> tcs, Type returnType)
        {
            _tcs = tcs;
            ReturnType = returnType;
        }

        // When get() request is completed
        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (!task.IsSuccessful) { HandleError($"Unable to complete task   (Type={ReturnType.Name})");  return; }

            var result = task.Result;

            if (result is DocumentSnapshot doc)
            {
                
                if(doc.Data == null) { _tcs.TrySetResult((false, "Invalid document path")); }
                _tcs.TrySetResult(Helper.ParseObject(doc, ReturnType));
            } 
            else if (result is QuerySnapshot collection)
            {
                if (collection.IsEmpty) { HandleError($"No docs found for: {ReturnType.Name}"); return; }

                _tcs.TrySetResult(Helper.ParseObject(collection.Documents.ToArray(), ReturnType));
            }
            else { HandleError($"Result isn't a document or collection: objectType={result.GetType()}  Type={ReturnType.Name}"); return; }
        }

        private void HandleError(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            _tcs.TrySetResult((false, errorMsg));
        }



        /*public void OnComplete(Android.Gms.Tasks.Task task)
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
        }*/
    }
}