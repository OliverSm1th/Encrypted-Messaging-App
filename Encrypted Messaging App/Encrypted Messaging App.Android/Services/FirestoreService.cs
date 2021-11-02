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
using Firebase.Auth;
using Firebase.Firestore;
//using Google.Cloud.Firestore;
using Firebase.Firestore;
using Android.Gms.Extensions;
using Encrypted_Messaging_App.Droid.Resources;
using Java.Util;
using System.Reflection;

[assembly: Dependency(typeof(Encrypted_Messaging_App.Droid.ManageFirestoreService))]


namespace Encrypted_Messaging_App.Droid
{
    class ManageFirestoreService : IManageFirestoreService
    {
        // Main manager for everything firestore
        private Dictionary<string, string> firestorePaths = new Dictionary<string, string>
        {
            {"Requests", $"requests/pending/[USERID]" },
            {"AcceptRequests", $"requests/accepted/[USERID]" },
            {"CUser", $"users/[USERID]" },
            {"UserFromUsername", $"usersPublic/<USERNAME>" },
            {"UserFromId", $"usersPublicID/<USERID>" },
            {"Chat", $"chats/<CHATID>" }
        };

        private (bool success, DocumentReference docRef, CollectionReference collectRef) GetReferenceFromPath(string[] pathLevels)
        {
            if(pathLevels.Length == 0) { return (false, null, null); }

            CollectionReference collection = FirebaseFirestore.Instance.Collection(pathLevels[0]);
            DocumentReference document = null;

            foreach(string currPathLevel in pathLevels.Skip(1))
            {
                if(currPathLevel.Length == 0) { break; }
                if(document is null)
                {
                    document = collection.Document(currPathLevel);
                    collection = null;
                }
                else
                {
                    collection = document.Collection(currPathLevel);
                    document = null;
                }
            }
            return (true, document, collection);
        }
        private (bool success, DocumentReference docRef, CollectionReference collectRef) GetReferenceFromPath(string path)
        {
            return GetReferenceFromPath(path.Split('/'));
        }


        public string GetPath(string type, params (string, string)[] arguments) //Dictionary<string, string> arguments = null
        {
            Dictionary<string, string> dictArgs = arguments.ToDictionary(arg => arg.Item1, arg => arg.Item2);
            if (!firestorePaths.ContainsKey(type) || firestorePaths[type] == null)
            {
                Console.WriteLine($"Invalid type passed: {type}");
                return null;
            }

            string[] pathLevels = firestorePaths[type].Split("/");
            for (int i=0; i<pathLevels.Length; i++)
            {
                pathLevels[i] = parseLevelArgument(pathLevels[i], dictArgs);
            }
            Console.WriteLine($"Path generated: {type} -> {string.Join("/", pathLevels)}");
            return string.Join("/", pathLevels);
        }
        private string parseLevelArgument(string levelName, Dictionary<string, string> arguments)
        {
            if (levelName.StartsWith("<") && levelName.EndsWith(">"))
            {
                levelName = levelName.TrimStart('<').TrimEnd('>');

                if (arguments == null) { Console.WriteLine($"No Arguments have been set (Expecting {levelName})"); return ""; }
                else if (arguments[levelName] == null) { Console.WriteLine($"Missing Argument: {levelName}"); return ""; }
                else
                {
                    levelName = arguments[levelName];
                }
            }
            else if (levelName.StartsWith("[") && levelName.EndsWith("]"))
            {
                levelName = levelName.TrimStart('[').TrimEnd(']');
                if (levelName == "USERID")
                {
                    levelName = FirebaseAuth.Instance.CurrentUser.Uid;
                }
                else
                {
                    Console.WriteLine($"Unrecognised automatic argument: {levelName}");
                }
            }
            return levelName;
        }



           //   GET:
        public Task<(bool, object)> FetchData(string type, params (string, string)[] arguments)
        {
            Console.WriteLine($"Fetching Data for {type}:");
            var tcs = new TaskCompletionSource<(bool, object)>();

            string path = GetPath(type, arguments);
            if (path is null)
            {
                tcs.TrySetResult((false, $"Invalid type passed: {type}"));
                return tcs.Task;
            }


            (bool success, DocumentReference document, CollectionReference collection) reference = GetReferenceFromPath(path);
            if (!reference.success)
            {
                tcs.TrySetResult((false, $"Invalid path: {path}"));
            }
            else
            {
                if (reference.collection == null) { reference.document.Get().AddOnCompleteListener(new OnCompleteListener(tcs, type));   }
                else                              { reference.collection.Get().AddOnCompleteListener(new OnCompleteListener(tcs, type)); }
            }
            return tcs.Task;

        }

        // Common:
        public async Task<User> UserFromId(string id) 
        {
            (bool success, object user) response = await FetchData("UserFromId", ("USERID", id));  //new Dictionary<string, string> { { "USERID", id } }

            return response.success ? (User)response.user : null;
        }
        public async Task<User> UserFromUsername(string username)
        {
            (bool success, object user) response = await FetchData("UserFromUsername", ("USERNAME", username)); //new Dictionary<string, string> { { "USERID", username } }

            return response.success ? (User)response.user : null;
        }


        // Listeners:
        private List<IListenerRegistration> Liseners = new List<IListenerRegistration>();
        public bool ListenData(string type, Action<object> action, string changeType = null, bool ignoreInitialEvent = false, params (string, string)[] arguments) //Dictionary<string, string> arguments = null
        {
            string path = GetPath(type, arguments);
            if(path is null) { return false; }
            DocumentChange.Type ChangeType = getDocChangeType(changeType);

            (bool success, DocumentReference document, CollectionReference collection) reference = GetReferenceFromPath(path);


            if (reference.success)
            {
                if (!(reference.document is null)) { 
                    Liseners.Add(reference.document.AddSnapshotListener(new EventListener(type, action, ChangeType, ignoreInitialEvent))); 
                } else { 
                    Liseners.Add(reference.collection.AddSnapshotListener(new EventListener(type, action, ChangeType, ignoreInitialEvent))); 
                } return true;
            } else 
            {
                Console.WriteLine($"Invalid path: {path}");
                return false;
            }
        }
        private DocumentChange.Type getDocChangeType(string changeType)
        {
            if (changeType == "added") { return DocumentChange.Type.Added; }
            else if (changeType == "modified") { return DocumentChange.Type.Modified; }
            else if (changeType == "removed") { return DocumentChange .Type.Removed; }
            return null;
        }

        public void RemoveListeners()
        {
            foreach(IListenerRegistration listener in Liseners)
            {
                listener.Remove();
            }
        }




           //   SET:
        public async Task<(bool, string)> WriteObject(object obj, string path)
        {
            (bool success, DocumentReference document, CollectionReference collection) reference = GetReferenceFromPath(path);
            if (!reference.success)
            {
                return (false, $"Invalid path given: {path}");
            } else if(reference.document is null)
            {
                return (false, "Invalid length of path given, must be odd to give document");
            }

            HashMap objHashMap = GetMap(obj);
            try
            {
                await reference.document.Set(objHashMap);
                return (true, "");
            }
            catch(Exception e)
            {
                return (false, e.Message);
            }

            
        }

        public async Task<(bool, string)> InitiliseUser(string username)
        {
            HashMap privateMap = new HashMap();
            privateMap.Put("username", username);
            privateMap.Put("chatsID", new JavaList<string>());

            HashMap publicMap = new HashMap();
            publicMap.Put("Id", FirebaseAuth.Instance.CurrentUser.Uid);

            // Private User Data
            DocumentReference docRef = FirebaseFirestore.Instance.Collection("users").Document(FirebaseAuth.Instance.CurrentUser.Uid);
            // Public
            DocumentReference publicDocRef = FirebaseFirestore.Instance.Collection("usersPublic").Document(username);

            try
            {
                await docRef.Set(privateMap);
                await publicDocRef.Set(publicMap);
                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }

        /*
        public async Task<(bool, string)> InitiliseChat(Chat chat) //User[] users
        {
            HashMap map = GetMap(chat);


            CollectionReference collection = FirebaseFirestore.Instance.Collection("chats");
            try
            {
                DocumentReference doc = (DocumentReference) await collection.Add(map);
                return (true, doc.Id);
            }
            catch(Exception e)
            {
                return (false, e.Message);
            }
        }*/

        // Test to see if the WriteObject function works, will be implemented in the actual classes if works.
        public async Task<(bool, string)> InitiliseChat(Chat chat)
        {
            return await WriteObject(chat, GetPath("Chat", arguments:("CHATID", chat.Id)));
        }

        public async Task<(bool, string)> SendAcceptedRequest(string requestUserID, AcceptedRequest ARequest)
        {
            CollectionReference requestCollection = FirebaseFirestore.Instance.Collection("requests");
            DocumentReference acceptedRequests = requestCollection.Document("accepted");
            DocumentReference userAcceptedRequests = acceptedRequests.Collection(requestUserID).Document(FirebaseAuth.Instance.CurrentUser.Uid);

            DocumentReference pendingRequests = requestCollection.Document("pending");
            DocumentReference userPendingRequests = pendingRequests.Collection(FirebaseAuth.Instance.CurrentUser.Uid).Document(requestUserID);


            HashMap acceptMap = GetMap(ARequest);

            try
            {
                await userPendingRequests.Delete();
                Console.WriteLine($"Deleted Pending Request for id:{FirebaseAuth.Instance.CurrentUser.Uid}");

                await userAcceptedRequests.Set(acceptMap);
                Console.WriteLine($"Added Accepted Request for id:{requestUserID}");
                return (true, "");
            } catch(Exception e)
            {
                return (false, e.Message);
            }
        }

        public async Task<(bool, string)> SendRequest(Request request, string requestUserID)
        {
            CollectionReference requestCollection = FirebaseFirestore.Instance.Collection("requests");
            DocumentReference pendingRequests = requestCollection.Document("pending");
            CollectionReference userCollection = pendingRequests.Collection(requestUserID);


            HashMap pendingRequest = GetMap(request);

            try
            {
                await userCollection.Document(request.SourceUser.Id).Set(pendingRequest);
                Console.WriteLine("Added Pending Request");
                return (true, "");
            } catch(Exception e)
            {
                return (false, e.Message);
            }
        }

        public async Task<(bool, string)> AddChatIDToUser(string userID, string chatID)
        {
            DocumentReference doc = FirebaseFirestore.Instance.Collection("users").Document(userID);
            try
            {
                await doc.Update("chatsID", FieldValue.ArrayUnion(chatID));
                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }

        
        // Update Field:
        private async Task<(bool, string)> UpdateField(Java.Lang.Object obj, string path)
        {
            List<string> pathLevels = path.Split('/').ToList();
            string fieldLevel = PopFromList(ref pathLevels, -1);
            (bool invalidPath, DocumentReference document, CollectionReference collection) reference = GetReferenceFromPath(path);

            if (reference.invalidPath)
            {
                return (false, $"Invalid path given: {path}");
            }
            else if (reference.document == null)
            {
                return (false, "Invalid length of path given, must be odd");
            }
            else
            {
                try
                {
                    await reference.document.Update(fieldLevel, obj);
                    return (true, "");
                }
                catch (Exception e)
                {
                    return (false, e.Message);
                }
            }
        }

        public async Task<(bool, string)> AddToArray(string newItem, string path)
        {
            return await UpdateField(FieldValue.ArrayUnion(newItem), path);
        }

        public async Task<(bool, string)> RemoveFromArray(string oldItem, string path)
        {
            return await UpdateField(FieldValue.ArrayRemove(oldItem), path);
        }

        public async Task<(bool, string)> UpdateString(string newString, string path)
        {
            return await UpdateField(newString, path);
        }

        
        // Utility functions
        private HashMap GetMap(object obj) // Converts any object into a hashmap for saving to firestore
        {
            string parentMethodName = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
            string indent = "";
            if(parentMethodName == "GetMap") { indent = "     ";  }
            Console.WriteLine($"{indent}Converting {obj.GetType()} to HashMap...");
            
            HashMap map = new HashMap();
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                var propValue = prop.GetValue(obj, null);
                if(propValue == null) { Console.WriteLine($"{prop} is not defined in {obj.GetType()}"); }
                Type type = propValue.GetType();

                
                if (type.IsArray) // || type.GetGenericTypeDefinition() == typeof(List<>)
                {
                    if(type.GetGenericTypeDefinition() == typeof(List<>)) { propValue = ((List<object>)propValue).ToArray(); }
                    JavaList<HashMap> arrayMap = new JavaList<HashMap>();
                    int index = 0;
                    foreach (var item in (object[])propValue){
                        if (item is string str_item) {
                            Console.WriteLine($"{indent}{index}: {str_item}"); }
                        else {
                            Console.WriteLine($"{indent}{index}: {item.GetType()}"); }

                        arrayMap.Add(GetMap(item));
                        index++;
                    }
                    map.Put(prop.Name, arrayMap);
                }
                else if (!type.Namespace.StartsWith("System"))
                {
                    Console.WriteLine($"{indent}{prop.Name}: ");
                    //foreach (PropertyInfo testProp in prop.GetType().GetProperties())
                    //{
                    //    Console.WriteLine($"Inner: {testProp.Name}:{testProp.GetValue(prop, null).ToString()}");
                    //}
                    map.Put(prop.Name, GetMap(propValue));

                    Console.WriteLine("\n");
                }
                else
                {
                    Console.WriteLine($"{indent}{prop.Name}:{propValue.ToString()}");
                    map.Put(prop.Name, propValue.ToString());
                }
            }
            if(obj.GetType().GetProperties().Length == 0)
            {
                Console.WriteLine($"No properties found of object of type: {obj.GetType().ToString()}");
            }
            return map;
        }
        private string PopFromList(ref List<string> path, int index)
        {
            string fieldLevel = path[path.Count - 1];
            path.RemoveAt(path.Count - 1);
            return fieldLevel;
        }
    }
}
