using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Firestore;
using Java.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace Encrypted_Messaging_App.Droid.Resources
{
    class ListenerHelper
    {
        public (bool, object) ParseObject(string objectName, object input)
        {
            string methodName = "Parse" + objectName;
            MethodInfo helperMethod = this.GetType().GetMethod(methodName);
            if (helperMethod != null && helperMethod.GetParameters().Length > 0 && helperMethod.GetParameters()[0].ParameterType == input.GetType())
            {
                (bool, object) result = ((bool, object))helperMethod.Invoke(this, new object[] { input });
                return result;
            }
            else
            {
                Console.WriteLine($"Invalid type {methodName} for Helper Method");
                //OutputMethods(Helper.GetType().GetMethods());
                if (helperMethod != null)
                {
                    Console.WriteLine($"Expected Input Type: {this.GetType().GetMethod(methodName).GetGenericArguments()[0]}\nActual Input Type Provided: {input.GetType()}");
                }

                return (false, "Invalid type given to ParseObject");
            }
        }


        // Convert Dictionary to the object of type: type
        private T ToObject<T>(JavaDictionary dict) where T : class, new() // WARNING: Also update EventListener Version
        {
            //var instance = Activator.CreateInstance(type);
            var instance = new T();
            Type type = instance.GetType();

            IEnumerator keyEnumerator = dict.Keys.GetEnumerator();
            IEnumerator valueEnumerator = dict.Values.GetEnumerator();
            keyEnumerator.MoveNext();
            valueEnumerator.MoveNext();

            for (int i = 0; i < dict.Keys.Count; i++)
            {
                string propName = keyEnumerator.Current as string;
                if (propName != null)
                {
                    PropertyInfo prop = type.GetProperty(propName);
                    string initialValue = (string)valueEnumerator.Current;

                    if (prop != null)
                    {
                        // Special Type Conversions:
                        if (prop.PropertyType == typeof(BigInteger))
                        {
                            prop.SetValue(instance, BigInteger.Parse(initialValue), null);
                        }
                        else if (prop.PropertyType == typeof(Int32))
                        {
                            prop.SetValue(instance, Int32.Parse(initialValue), null);
                        }
                        else
                        {
                            prop.SetValue(instance, initialValue, null);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Can't convert property name to string");
                }

                keyEnumerator.MoveNext();
                valueEnumerator.MoveNext();
            }


            //foreach (var item in dict)
            //{
            //    type.GetProperty(item.Key).SetValue(instance, item.Value, null);
            //}
            return instance;
        }
        private T ToObject<T>(Dictionary<string, Java.Lang.Object> dict) where T : class, new()
        {
            //var instance = Activator.CreateInstance(type);
            var instance = new T();
            Type type = instance.GetType();

            string[] keys = dict.Keys.ToArray();
            object[] values = dict.Values.ToArray();



            for (int i = 0; i < dict.Keys.Count; i++)
            {
                PropertyInfo prop = type.GetProperty(keys[i]);
                string initialValue = (string)values[i];

                // Special Type Conversions:
                if (prop.PropertyType == typeof(BigInteger))
                {
                    prop.SetValue(instance, BigInteger.Parse(initialValue), null);
                }
                else if (prop.PropertyType == typeof(Int32))
                {
                    prop.SetValue(instance, Int32.Parse(initialValue), null);
                }
                else
                {
                    prop.SetValue(instance, initialValue, null);
                }
            }
            return instance;
        }

        // Parse Objects:
        // User
        public (bool, object) ParseCUser(DocumentSnapshot doc) // Private user info: chatsID
        {
            //if (Username.Length == 0)
            //{
            //Console.WriteLine("No username given");
            //return (false, "No username given");
            //}
            CUser user = new CUser(doc.Id, FirebaseAuth.Instance.CurrentUser.DisplayName);
            //return (true, user);
            try
            {
                // Chats:
                if (doc.Get("chatsID") != null)
                {
                    JavaList chats = (JavaList)doc.Get("chatsID");

                    string[] chatsArr = (string[])ParseEnumerator(chats.GetEnumerator(), chats.Count);

                    user.chatsID = chatsArr;
                    //user.SetChats(chatsArr);
                }
                return (true, user);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed settings values: {e.Message}");
                return (false, "Failed to get user");
            }
        }
        public (bool, object) ParseChatsID(DocumentSnapshot doc)
        {
            try
            {
                // Chats:
                if (doc.Get("chatsID") != null)
                {
                    JavaList chats = (JavaList)doc.Get("chatsID");

                    string[] chatsArr = (string[])ParseEnumerator(chats.GetEnumerator(), chats.Count);

                    return (true, chatsArr);
                }
                return (false, "No chatID found");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed getting id: {e.Message}");
                return (false, "Failed to get chatsID");
            }
        }
        public (bool, object) ParseUser_Username(DocumentSnapshot doc)  // Public user info: username, id
        {
            User user = new User();
            user.Username = doc.Id;

            try
            {
                if (doc.Get("Id") != null)
                {
                    user.Id = (string)doc.Get("Id");
                }


                return (true, user);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed settings values: {e.Message}");
                return (false, "Failed to get user");
            }
        }
        public (bool, object) ParseUser_Id(DocumentSnapshot doc)  // Public user info: username, id
        {
            User user = new User();
            user.Id = doc.Id;

            try
            {
                if (doc.Get("Username") != null)
                {
                    user.Username = (string)doc.Get("Username");
                }

                return (true, user);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed settings values: {e.Message}");
                return (false, "Failed to get user");
            }
        }

        // Chat
        public (bool, object) ParseChat(DocumentSnapshot doc)
        {
            List<Message> messages = new List<Message>();
            List<User> users = new List<User>();

            try
            {
                if (doc.Get("messages") != null)
                {
                    JavaList messagesObj = (JavaList)doc.Get("messages");
                    

                    object[] messageArr = ParseEnumerator(messagesObj.GetEnumerator(), messagesObj.Count);

                    foreach(object message in messageArr)
                    {
                        (bool success, object message) result = ParseMessage(message);
                        if (result.success)
                        {
                            messages.Add((Message) message);
                        }
                        else
                        {
                            Console.WriteLine("Unable to parse message");
                        }
                    }
                }
                if (doc.Get("users") != null)
                {
                    JavaList usersObj = (JavaList)doc.Get("users");

                    object[] userArr = ParseJavaList(usersObj);

                    foreach(object user in userArr)
                    {
                        //users.Add((string)user);
                    }



                }

                Chat chat = new Chat { messages = messages, users = users.ToArray() };

                return (false, "");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed settings values: {e.Message}");
                return (false, "Failed to get chats");
            }
        }
        public (bool, object) ParseMessage(object messageObj)
        {
            // Content
            //if (messageObj.)
            //{

            //}
            // Times

            // Author

            // Recipient
            return (false, "");
        }

        // Request
        public (bool, object) ParseRequests(DocumentSnapshot[] docs)
        {
            List<Request> requests = new List<Request>();

            foreach (DocumentSnapshot doc in docs)
            {
                var t_data = doc.Data;

                JavaDictionary data = (JavaDictionary)t_data;
                Request request = ParseRequest(data);
                if (request != null) { requests.Add(request); }
            }
            return (true, requests.ToArray());
        }
        private Request ParseRequest(JavaDictionary data)
        {
            if (data != null)
            {
                if (data["EncryptionInfo"] is JavaDictionary encryptionDict && data["SourceUser"] is JavaDictionary userDict)
                {
                    KeyData encryptionInfo = ToObject<KeyData>(encryptionDict);
                    User user = ToObject<User>(userDict);
                    if(encryptionInfo == null || user == null) { return null; }
                    return new Request(encryptionInfo, user);
                }
                else
                {
                    Console.WriteLine("Invalid document passed");
                    return null;
                }
            }
            return null;
        }

        public (bool, object) ParseAcceptRequests(DocumentSnapshot[] docs)
        {
            List<AcceptedRequest> requests = new List<AcceptedRequest>();

            foreach (DocumentSnapshot doc in docs)
            {
                JavaDictionary data = (JavaDictionary)doc.Data;
                AcceptedRequest request = ParseARequest(data);
                if (request != null) { requests.Add(request); }
                doc.Reference.Delete();
            }
            return (true, requests.ToArray());
        }
        private AcceptedRequest ParseARequest(JavaDictionary data)
        {
            if (data == null) { return null; }

            if (data["newChatID"] is string chatID && data["requestUserID"] is string requestUserID && data["EncryptionInfo"] is JavaDictionary encryptionDict)
            {
                AcceptedRequest request = new AcceptedRequest(chatID, ToObject<KeyData>(encryptionDict));
                request.requestUserID = requestUserID;
                return request;
            }
            else
            {
                Console.WriteLine($"Invalid document passed");
                return null;
            }
        }
        //----------//


        // Useful Functions
        private Dictionary<string, object> JavaDictToDict(JavaDictionary dict)
        {
            var test = dict["test"];



            string[] keyArr = (string[])ParseEnumerator(dict.Keys.GetEnumerator(), dict.Keys.Count);
            object[] valueArr = ParseEnumerator(dict.Values.GetEnumerator(), dict.Values.Count);

            Dictionary<string, object> newDict = new Dictionary<string, object>();
            for (int i = 0; i < keyArr.Length; i++)
            {
                newDict.Add(keyArr[i], valueArr[i]);
            }
            return newDict;
        }
        private Dictionary<string, object> HashMapToDict(HashMap hash)       // HashMap -> Dictionary<string, object>
        {
            string[] keyArr = (string[])ParseEnumerator(hash.KeySet().GetEnumerator(), hash.KeySet().Count);
            object[] valueArr = ParseEnumerator(hash.Values().GetEnumerator(), hash.Values().Count);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0; i < keyArr.Length; i++)
            {
                dict.Add(keyArr[i], valueArr[i]);
            }
            return dict;
        }

        public object[] ParseEnumerator(IEnumerator enumerator, int length) // Enumerator -> Object[]
        {
            object[] array = new string[length];

            enumerator.MoveNext();
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine($"{i}: {enumerator.Current}");
                Console.WriteLine(enumerator.Current.GetType());
                if (enumerator.Current is JavaDictionary currentDict)
                {
                    array[i] = JavaDictToDict(currentDict);
                }
                else
                {
                    array[i] = enumerator.Current;
                    enumerator.MoveNext();
                }

            }
            return array;
        }

        public object[] ParseJavaList(JavaList list)
        {
            return ParseEnumerator( list.GetEnumerator(), list.Count);
        }



        // Test method
        private Type targetType;
        private object GetObject(DocumentSnapshot doc, Type type, string optionalExtra = "")
        {
            var instance = Activator.CreateInstance(type);
            if (optionalExtra.Length > 0)
            {
                var final = doc.Get(optionalExtra);
                Console.WriteLine("Got extra");
            }

            foreach (PropertyInfo prop in targetType.GetProperties())
            {
                var test = prop.GetValue(instance, null);
                Type childType = test.GetType();
                Console.WriteLine($"Type: {childType.ToString()}");


                if (!childType.Namespace.StartsWith("System"))
                {
                    Console.WriteLine($"{prop.Name}: ");
                    foreach (PropertyInfo testProp in test.GetType().GetProperties())
                    {
                        Console.WriteLine($"Inner: {testProp.Name}:{testProp.GetValue(prop, null).ToString()}");
                    }
                    //map.Put(prop.Name, GetMap(test));

                    Console.WriteLine("\n");
                }
                else
                {
                    Console.WriteLine($"{prop.Name}:{test.ToString()}");
                    //map.Put(prop.Name, test.ToString());
                }
            }
            return instance;
        }
    }
}