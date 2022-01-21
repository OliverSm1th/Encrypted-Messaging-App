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
using static Encrypted_Messaging_App.LoggerService;

namespace Encrypted_Messaging_App.Droid.Resources
{

    class OnEventListener : Java.Lang.Object, IEventListener
    {
        Type EventType;
        DocumentChange.Type ChangeType;     // ADDED/MODIFIED/REMOVED
        Action<object> OnEventMethod;
        private string FieldName;

        ListenerHelper Helper = new ListenerHelper();    // Helper: Has Parse Functions which convert results into objects

        public OnEventListener(Type returnType, Action<object> method, DocumentChange.Type changeType = null, string optionalFieldName=null)
        {
            EventType = returnType;
            OnEventMethod = method;
            ChangeType = changeType;
            FieldName = optionalFieldName;
        }


        public void OnEvent(Java.Lang.Object obj, FirebaseFirestoreException error)
        {

            Debug($"Event Triggered: {EventType}");


            (bool success, object obj) response = (false, null);


            if (obj is DocumentSnapshot doc)          // Chat/ChatID
            {
                if(doc.Data == null) { OnEventMethod(null); return; }
                if(FieldName == null) {
                    response = Helper.ParseObject(doc, EventType);
                }
                else
                {
                    object field = doc.Get(FieldName);
                    response = Helper.ParseObject(field, EventType);
                }
            } 
            else if(obj is QuerySnapshot collection)
            {
                if (collection.IsEmpty) { 
                    Debug($"No docs found for: {EventType}", indentationLvl:1); OnEventMethod(null); return;
                }
                DocumentSnapshot[] docs = GetFilteredDocs(collection);
                
                response = Helper.ParseObject(docs, EventType);    
            }
            else
            {
                Error("result isn't document or collection!!", indentationLvl: 1);
                return;
            }

            if (response.success)
            {
                OnEventMethod(response.obj);
            }
        }

        private DocumentSnapshot[] GetFilteredDocs(QuerySnapshot collection)
        {
            // No ChangeType
            if(ChangeType == null) {
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


        public (bool, object) InvokeListenerHelperMethod(string methodName, object argument)
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
                    //Console.WriteLine($"Expected Type: {Helper.GetType().GetMethod(EventType).GetGenericArguments()[0]}\nActual Type: {argument.GetType()}");
                }
                
                return (false, "");
            }
        }

        private void OutputMethods(MethodInfo[] methods)  // Debugging Method
        {
            for(int i=0; i<methods.Length; i++)
            {
                Console.WriteLine(methods[i].Name);
            }
        }
    }
}