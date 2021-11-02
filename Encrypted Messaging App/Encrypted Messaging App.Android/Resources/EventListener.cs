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
        string EventType;
        DocumentChange.Type ChangeType;     // ADDED/MODIFIED/REMOVED
        Action<object> OnEventMethod;
        private bool IgnoreInitilal;

        ListenerHelper Helper = new ListenerHelper();    // Helper: Has Parse Functions which convert results into objects
        private bool FirstTime = true;

        public EventListener(string type, Action<object> method, DocumentChange.Type changeType = null, bool ignoreInitial=false)
        {
            EventType = type;
            OnEventMethod = method;
            ChangeType = changeType;
            IgnoreInitilal = ignoreInitial;
        }


        public void OnEvent(Java.Lang.Object obj, FirebaseFirestoreException error)
        {
            if (FirstTime && IgnoreInitilal) { 
                return;
            } else {
                Console.WriteLine($"EVENT TRIGGERED: {EventType}");
            }
            

            (bool success, object obj) response = (false, null);


            if (obj is DocumentSnapshot doc)
            {
                response = HandleDocument(doc);
                // Handles: Chat, ChatID
            } 
            else if(obj is QuerySnapshot collection)
            {
                if (collection.IsEmpty)
                {
                    Console.WriteLine($"No docs found for: {EventType}");
                }

                response = HandleCollection(collection);             
            }
            else
            {
                Console.WriteLine("result isn't document or collection!!");
                return;
            }

            if (response.success)
            {
                OnEventMethod(response.obj);
            }
            FirstTime = false;
        }

        public (bool, object) HandleDocument(DocumentSnapshot document)
        {
            /*
            if (EventType == "Chat")
            {
                //return Helper.ParseChat(doc);
                return Helper.GetType().GetMethod(EventType).Invoke(Helper, new object[]{ doc });
            }
            else if (EventType == "ChatsID")
            {
                return Helper.ParseChatsID(doc);
            }
            else
            {

            }*/
            return InvokeHelperMethod(EventType, document);
        }

        public (bool, object) HandleCollection(QuerySnapshot collection)
        {
            DocumentSnapshot[] docs = GetFilteredDocs(collection);

            return InvokeHelperMethod(EventType, docs);
            /*
            if (EventType == "Requests")
                {
                    response = Helper.ParseRequests(docs);
                }
                else if (EventType == "AcceptRequests")
                {
                    response = Helper.ParceARequests(docs);
                }
                else { Console.WriteLine($"Invalid type {EventType} for QuerySnapshot"); } 
            */
        }

        public DocumentSnapshot[] GetFilteredDocs(QuerySnapshot collection)
        {
            // No ChangeType
            if(ChangeType == null || FirstTime) {
                return collection.Documents.ToArray();
            }

            // ChangeType: ADDED/MODIFIED/REMOVED
            List<DocumentSnapshot> filteredDocs = new List<DocumentSnapshot>();

            foreach (DocumentChange change in collection.DocumentChanges) {
                if (change.GetType() == ChangeType) { 
                    filteredDocs.Add(change.Document); 
                }
            }

            if (filteredDocs.Count == 0) { Console.WriteLine("No documents after filter"); }
            return filteredDocs.ToArray();
        }


        public (bool, object) InvokeHelperMethod(string methodName, object argument)
        {
            methodName = "Parse" + methodName;
            MethodInfo helperMethod = Helper.GetType().GetMethod(methodName);
            if (helperMethod != null  &&  helperMethod.GetParameters().Length > 0  &&  helperMethod.GetParameters()[0].ParameterType == argument.GetType())
            {
                (bool, object)result = ((bool, object))helperMethod.Invoke(Helper, new object[] { argument });
                return result;
            }
            else
            {
                Console.WriteLine($"Invalid type {methodName} for Helper Method");
                //OutputMethods(Helper.GetType().GetMethods());
                if (helperMethod != null)
                {
                    Console.WriteLine($"Expected Type: {Helper.GetType().GetMethod(EventType).GetGenericArguments()[0]}\nActual Type: {argument.GetType()}");
                }
                
                return (false, "");
            }
        }

        private void OutputMethods(MethodInfo[] methods)
        {
            for(int i=0; i<methods.Length; i++)
            {
                Console.WriteLine(methods[i].Name);
            }
        }
    }
}