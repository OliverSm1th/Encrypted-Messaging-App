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

namespace Encrypted_Messaging_App.Droid.Resources
{
    class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
    {
        private TaskCompletionSource<CUser> _tcs;
        private string Username;

        public OnCompleteListener(TaskCompletionSource<CUser> tcs, string username)
        {
            _tcs = tcs;
            Username = username;
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            Console.WriteLine("Started OnComplete");
            if (task.IsSuccessful)
            {
                var result = task.Result;
                if(result is DocumentSnapshot doc)
                {
                    Console.WriteLine("Result is Document Snapshot");
                    CUser user = new CUser(doc.Id, Username);

                    try
                    {
                        // Chats:
                        if(doc.Get("chats") != null)
                        {
                            JavaList chats = (JavaList) doc.Get("chats");
                            if(chats == null) { 
                                Console.WriteLine("chats is undefined");
                                return;
                            }
                            IEnumerator enumerator = chats.GetEnumerator();
                            string[] chatsArr = new string[chats.Count];

                            Console.WriteLine("--Chats--");
                            enumerator.MoveNext();
                            for(int i=0; i<chats.Count; i++)
                            {
                                Console.WriteLine($"{i}: {enumerator.Current}");
                                chatsArr[i] = (string) enumerator.Current;
                                enumerator.MoveNext();
                            }
                            user.SetChats(chatsArr);
                        }
                        /*if(doc.Get("requests") != null)
                        {
                            JavaList requests = (JavaList)doc.Get("requests");
                            if(requests == null) {
                                Console.WriteLine("requests is undefined");
                                return;
                            }
                            IEnumerator enumerator = requests.GetEnumerator();
                            string[] requestArr = new string[requests.Count];

                            Console.WriteLine("--Requests--");
                            enumerator.MoveNext();
                            for(int i=0; i< requests.Count; i++)
                            {
                                Console.WriteLine($"{i}: {enumerator.Current}");
                                requestArr[i] = (string)enumerator.Current;
                                enumerator.MoveNext();
                            }

                        }*/

                    } catch(Exception e)
                    {
                        Console.WriteLine($"Failed settings values: {e.Message}");
                    }
                    
                    _tcs.TrySetResult(user);
                    return;
                }
                else
                {
                    Console.WriteLine("Result isn't Document Snapshot");
                }
            }
            else
            {
                Console.WriteLine("Setting default result...");
                _tcs.TrySetResult((default(CUser)));
                Console.WriteLine("Default Result set");
            }
        }
    }
}