using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Gms.Tasks;
using System.Collections;
using System.Reflection;
using System.Numerics;

namespace Encrypted_Messaging_App.Droid.Resources
{
    class EventListener : Java.Lang.Object, IEventListener
    {
        string Type;
        DocumentChange.Type ChangeType;
        Action<object> Method;
        private bool firstTime = true;
        private bool IgnoreInitilal;

        ListenerHelper Helper = new ListenerHelper();    // Helper: Has Parse Functions which convert results into objects


        public EventListener(string type, Action<object> method, DocumentChange.Type changeType = null, bool ignoreInitial=false)
        {
            Type = type;
            Method = method;
            ChangeType = changeType;
            IgnoreInitilal = ignoreInitial;
        }


        public void OnEvent(Java.Lang.Object obj, FirebaseFirestoreException error)
        {
            if (firstTime) {
                if (IgnoreInitilal) { return; }
                Console.WriteLine($"EVENT INITILISED: {Type}"); 
            }
            else { Console.WriteLine($"EVENT TRIGGERED: {Type}"); }

            

            (bool success, object obj) response = (false, null);


            if (obj is DocumentSnapshot doc)
            {
                
                if(Type == "Chat")
                {
                    response = Helper.ParseChat(doc);
                }
                else if(Type == "ChatsID")
                {
                    response = Helper.ParseChatsID(doc);
                }
                else
                {
                    Console.WriteLine($"Invalid type {Type} for DocumentSnapshot");
                    return;
                }
            } 
            else if(obj is QuerySnapshot collection)
            {
                if (collection.IsEmpty)
                {
                    Console.WriteLine($"No docs found for: {Type}");
                }

                DocumentSnapshot[] docs = collection.Documents.ToArray();

                if (ChangeType != null && !firstTime) // Filter documents by changeType (added,modified,removed)
                {
                    List<DocumentSnapshot> newDocs = new List<DocumentSnapshot>();
                    foreach (DocumentChange change in collection.DocumentChanges)
                    {
                        if (change.GetType() == ChangeType) { newDocs.Add(change.Document); }
                    }

                    if (newDocs.Count == 0) { Console.WriteLine("No new documents"); }
                    docs = newDocs.ToArray();
                }

                if (Type == "Requests")
                {
                    response = Helper.ParseRequests(docs);
                }
                else if (Type == "AcceptRequests")
                {
                    response = Helper.ParceARequests(docs);
                }
                else { Console.WriteLine($"Invalid type {Type} for QuerySnapshot"); }                
            }
            else
            {
                Console.WriteLine("result isn't document or collection!!");
                return;
            }

            if (response.success)
            {
                Method(response.obj);
            }
            firstTime = false;
        }
    }
}