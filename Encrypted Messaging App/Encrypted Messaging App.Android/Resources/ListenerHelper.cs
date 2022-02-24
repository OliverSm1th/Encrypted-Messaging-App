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
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using static Encrypted_Messaging_App.LoggerService;


namespace Encrypted_Messaging_App.Droid.Resources
{



    class ListenerHelper        // Converting:  Firebase Output > Classes
    {
        public class ParseStatus
        {
            public bool success = true;
            public string errorMessage;

            public ParseStatus(bool parseSuccess, string errorMsg) {
                success = parseSuccess; errorMessage = errorMsg;
            }

            public ParseStatus(bool parseSuccess) { success = parseSuccess; }
        }




        Dictionary<Type, string> returnTypes = new Dictionary<Type, string>();
        public ListenerHelper()
        {
            MethodInfo[] methods = this.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                Type currentType = method.ReturnType;
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(System.ValueTuple<,>))
                {
                    Type secondType = currentType.GetFields()[1].FieldType;
                    if (!returnTypes.ContainsKey(secondType))
                    {
                        returnTypes[secondType] = method.Name;
                    }
                }

            }
        }


        /*public (bool, object) ParseObject(string objectName, object input)
        {
            string methodName = "Parse" + objectName;
            MethodInfo helperMethod = this.GetType().GetMethod(methodName);
            if (helperMethod != null && helperMethod.GetParameters().Length > 0 && helperMethod.GetParameters()[0].ParameterType == input.GetType())
            {
                (bool, object) result = ((bool, object))helperMethod.Invoke(this, new object[] { input });
                return result;
            }
            else if (helperMethod == null)
            {
                Console.WriteLine($"Invalid type {methodName} for Helper Method");
            }
            else
            {
                if (this.GetType().GetMethod(methodName).GetGenericArguments().Length > 0)
                {
                    Console.WriteLine($"Expected Input Type: {this.GetType().GetMethod(methodName).GetGenericArguments()[0]}\nActual Input Type Provided: {input.GetType()}");
                }
                else
                {
                    Console.WriteLine($"Unexpected parameters for: {methodName} \nActual Input Type Provided: {input.GetType()}");
                }
            }
            return (false, "Invalid type/params given to ParseObject");
        }
        */

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
                if(keyEnumerator.Current == null) { continue; }
                string propName = keyEnumerator.Current.ToString();
                Console.WriteLine(keyEnumerator.Current);
                if (propName != null)
                {
                    PropertyInfo prop = type.GetProperty(propName);
                    if (prop.PropertyType.IsArray && valueEnumerator.Current is JavaList valueList)
                    {
                        object[] result = ParseEnumerator(valueList.GetEnumerator(), valueList.Count);
                        if(prop.PropertyType == typeof(string[])) { prop.SetValue(instance, ConvertObjArr(result)); }
                        else { Error($"Invalid array type: {prop.PropertyType.Name}"); }
                        continue;
                    }
                    
                    string initialValue = valueEnumerator.Current.ToString();

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
                    else { 
                        Error($"Null property: {prop.Name}"); }
                }
                else
                {
                    Error("Can't convert property name to string");
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



        private T[] ToObjectArr<T>(JavaList dict) where T : class, new()
        {
            List<T> result = new List<T>();
            

            for (int i = 0; i < dict.Count; i++)
            {
                JavaDictionary objectDict = (JavaDictionary)dict.Get(i);
                object convertedObject = ToObject<T>(objectDict);
                result.Add((T)convertedObject);
            }
            return result.ToArray();
        }

        // User
        /*
        public (bool, object) ParseCUser(DocumentSnapshot doc)
        {
            // Private user info: chatsID

            CUser user = new CUser(doc.Id, FirebaseAuth.Instance.CurrentUser.DisplayName);
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
                //Error($"Failed settings values: {e.Message}");
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
                Error($"Failed getting id: {e.Message}");
                return (false, "Failed to get chatsID");
            }
        }
        public (bool, object) ParseUserFromUsername(DocumentSnapshot doc)
        {
            // Public user info: username, id

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
                Error($"Failed settings values: {e.Message}");
                return (false, "Failed to get user");
            }
        }
        public (bool, object) ParseUserFromId(DocumentSnapshot doc)
        {
            // Public user info: username, id
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
                Error($"Failed settings values: {e.Message}");
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

                    foreach (object message in messageArr)
                    {
                        (bool success, object message) result = ParseMessage(message);
                        if (result.success)
                        {
                            messages.Add((Message)message);
                        }
                        else
                        {
                            Error($"Unable to parse message: {message.GetType()}");
                        }
                    }
                }
                if (doc.Get("users") != null)
                {
                    JavaList usersObj = (JavaList)doc.Get("users");

                    object[] userArr = ParseJavaList(usersObj);

                    foreach (object user in userArr)
                    {
                        //users.Add((string)user);
                    }



                }

                Chat chat = new Chat { messages = messages.ToArray(), users = users.ToArray() };

                return (false, "");
            }
            catch (Exception e)
            {
                Error($"Failed settings values: {e.Message}");
                return (false, "Failed to get chats");
            }
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
        */
        //-------------------//


        // Version 2.0

        public (bool, object) ParseObject(object input, Type expectedType)
        {
            Debug($"Parsing: {expectedType.Name}", includeMethod: true, indentationLvl: 1);
            if (!returnTypes.ContainsKey(expectedType))
            {
                Error($"Unexpected type {expectedType.Name}", indentationLvl: 2);
                return (false, $"Type to convert to {expectedType.Name} not in Helper Method (2)");
            }

            string methodName = returnTypes[expectedType];

            MethodInfo helperMethod = this.GetType().GetMethod(methodName);

            bool valid = true;
            if (helperMethod == null)
            {
                Error($"Unexpected type {methodName} for Helper Method (not included)", 2); valid = false;
            }
            else if (helperMethod.GetParameters().Length == 0)
            {
                Error($"No parameters specified for target method ({methodName})", 2); valid = false;
            }
            else if (helperMethod.GetParameters()[0].ParameterType != input.GetType())
            {
                Error($"Expected Parameter Type: {this.GetType().GetMethod(methodName).GetParameters()[0]}\nParameter Type Provided: {input.GetType()}  ({methodName})", 2); valid = false;
            }

            if (valid)
            {
                Debug($"{expectedType.Name}- Invoking method: {helperMethod.Name}", 1);
                object resultFromMethod = helperMethod.Invoke(this, new object[] { input });

                (bool, object) result = CovertReturnedTuple((ITuple)resultFromMethod);
                Debug($"{expectedType.Name}- Received result from method: ({result.Item1}, {result.Item2})", 2);

                return result;
            }
            else
            {
                return (false, "Invalid type/params given to ParseObject");
            }
        }


        public (ParseStatus, Request[]) ParseRequests(DocumentSnapshot[] docs)
        {
            List<Request> requests = new List<Request>();

            foreach (DocumentSnapshot doc in docs)
            {
                var t_data = doc.Data;

                JavaDictionary data = (JavaDictionary)t_data;
                Request request = ParseRequest(data);
                if (request != null) { requests.Add(request); }
            }
            return (new ParseStatus(true), requests.ToArray());
        }
        public (ParseStatus, Request) ParseRequest (DocumentSnapshot doc)
        {
            JavaDictionary data = (JavaDictionary)doc.Data;
            Request request = ParseRequest(data);
            if(request != null) { return (new ParseStatus(true), request); }
            else { return (new ParseStatus(false), null); }
        }
        private Request ParseRequest(JavaDictionary data)
        {
            if (data != null)
            {
                if (data["EncryptionInfo"] is JavaDictionary encryptionDict && data["SourceUser"] is JavaDictionary userDict)
                {
                    KeyData encryptionInfo = ToObject<KeyData>(encryptionDict);
                    User user = ToObject<User>(userDict);
                    if (encryptionInfo == null || user == null) { return null; }
                    return new Request(encryptionInfo, user);
                }
                else
                {
                    Error("Invalid document passed");
                    return null;
                }
            }
            return null;
        }

        

        public (ParseStatus, AcceptedRequest[]) ParseAcceptRequests(DocumentSnapshot[] docs)
        {
            List<AcceptedRequest> requests = new List<AcceptedRequest>();

            foreach (DocumentSnapshot doc in docs)
            {
                JavaDictionary data = (JavaDictionary)doc.Data;
                AcceptedRequest request = ParseARequest(data);
                if (request != null) { requests.Add(request); }
                doc.Reference.Delete();
            }
            return (new ParseStatus(true), requests.ToArray());
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
                Error($"Invalid document passed");
                return null;
            }
        }
        public (ParseStatus, Chat) ParseChat(DocumentSnapshot doc)
        {
            List<Message> messages = new List<Message>();
            List<User> users = new List<User>();
            List<string> userIDs = new List<string>();
            string title = null;
            KeyData encryptInfo = null;

            try
            {
                if (doc.Get("userIDs") != null)
                {
                    JavaList usersObj = (JavaList)doc.Get("userIDs");

                    object[] userArr = ParseJavaList(usersObj);

                    foreach (object user in userArr)
                    {
                        userIDs.Add((string)user);
                    }
                }
                if (doc.Get("title") != null)
                {
                    title = (string)doc.Get("title");
                }
                if(doc.Get("encryptionInfo") is JavaDictionary encryptionDict ){
                    encryptInfo = ToObject<KeyData>(encryptionDict);
                }
                if (doc.Get("messages") != null)
                {

                    JavaList messagesObj = (JavaList)doc.Get("messages");



                    for (int i = 0; i < messagesObj.Count; i++)
                    {
                        JavaDictionary messageObj = (JavaDictionary)messagesObj.Get(i);


                        Message messageResult = ParseMessage(messageObj);

                        if (messageResult != null) { messages.Add(messageResult); }
                        else { Error("Unable to parse message"); }

                        Log($"Done: {i}");

                    }

                    Log("Done!!");

                }

                Chat chat = new Chat { messages = messages.ToArray(), userIDs = userIDs.ToArray(), title = title, encryptionInfo = encryptInfo };

                return (new ParseStatus(true), chat);
            }
            catch (Exception e)
            {
                Error($"Failed settings values: {e.Message}");
                return (new ParseStatus(false, "Failed to get chats"), null);
            }
        }
        public Message ParseMessage(JavaDictionary data)
        {
            if(data["author"] is JavaDictionary authorDict && data["encryptedContent"].ToString() is string content && long.TryParse(data["createdTime"].ToString(), out long createdTime) &&
                long.TryParse(data["deliveredTime"].ToString(), out long deliveredTime) && long.TryParse(data["readTime"].ToString(), out long readTime) && data["pendingEvents"] is JavaList eventDict)
            {
                User author = ToObject<User>(authorDict);
                MessagePendingEvent[] pendingEvents = ToObjectArr<MessagePendingEvent>(eventDict);
                return new Message { author = author, encryptedContent = content, createdTime = DateTime.FromBinary(createdTime), deliveredTime = DateTime.FromBinary(deliveredTime), readTime = DateTime.FromBinary(readTime), pendingEvents = pendingEvents };
                
            }
            Log(data["encryptedContent"].ToString());
            return null;
        }

        public (ParseStatus, CUser) ParseCUser(DocumentSnapshot doc)
        {
            // Private user info: chatsID

            CUser user = new CUser(doc.Id, FirebaseAuth.Instance.CurrentUser.DisplayName);
            try
            {
                // Chats:
                if (doc.Get("chatsID") != null)
                {
                    JavaList chats = (JavaList)doc.Get("chatsID");

                    string[] chatsArr = ConvertObjArr(ParseEnumerator(chats.GetEnumerator(), chats.Count));

                    user.chatsID = chatsArr;
                    //user.SetChats(chatsArr);
                }
                return (new ParseStatus(true), user);
            }
            catch (Exception e)
            {
                Error($"Failed settings values: {e.Message}", 3);
                return (new ParseStatus(false, "Failed to get user"), null);
            }
        }
        public (ParseStatus, User) ParseUser(DocumentSnapshot doc)
        {
            User user = new User();
            

            if (doc.Get("Username") != null)
            {
                user.Username = (string)doc.Get("Username");
                user.Id = doc.Id;
            }
            else if(doc.Get("Id") != null)
            {
                user.Id = (string)doc.Get("Id");
                user.Username = doc.Id;
            }
            else
            {
                return (new ParseStatus(false, $"User object doesn't contain \'Username\' or \'Id\'"), null);
            }

            return (new ParseStatus(true), user);
        }
        public (ParseStatus, string[]) ParseStringArr(JavaList list)
        {
            Log("String Array sent to be parsed");
            try
            {
                object[] result = ParseEnumerator(list.GetEnumerator(), list.Count);
                return (new ParseStatus(true), ConvertObjArr(result));
            } catch(Exception e)
            {
                //Error($"Unable to parseEnumerator: {list.Count}");
                return (new ParseStatus(false, "Failed to ParseEnumerator"), null);
            }
        }

        // 
        public object[] ParseEnumerator(IEnumerator enumerator, int length)
        {
            // Enumerator -> Object[]
            object[] array = new object[length];
            Debug("Parse Enumerator:");
            enumerator.MoveNext();
            for (int i = 0; i < length; i++)
            {
                Debug($"{i}: {enumerator.Current}",1);
                //Debug($"{enumerator.Current.GetType()}",2);
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
            object[] newArray = (object[])array;
            return array;
        }
        private string[] ConvertObjArr(object[] input)
        {
            return Array.ConvertAll(input, x => x.ToString());
        }
        
        private Dictionary<string, object> JavaDictToDict(JavaDictionary dict)
        {
            // Used in: ParseEnumerator


            string[] keyArr = (string[])ParseEnumerator(dict.Keys.GetEnumerator(), dict.Keys.Count);
            object[] valueArr = ParseEnumerator(dict.Values.GetEnumerator(), dict.Values.Count);

            Dictionary<string, object> newDict = new Dictionary<string, object>();
            for (int i = 0; i < keyArr.Length; i++)
            {
                newDict.Add(keyArr[i], valueArr[i]);
            }
            return newDict;
        }
        private object[] ParseJavaList(JavaList list)
        {
            // Used in: ParseChat
            return ParseEnumerator(list.GetEnumerator(), list.Count);
        }

        

        



        // Test method  (Deprecated)
        //private Type targetType;
        /*
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
        }*/
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

        private (bool, object) CovertReturnedTuple(ITuple tuple)
        {
            if(tuple == null){
                Error("No tuple given");
                return (false, "No tuple given to tuple converter");
            }

            ParseStatus status = (ParseStatus)tuple[0];
            if (status.success)
            {
                return (true, (object)tuple[1]);
            }
            else
            {
                return (false, status.errorMessage);
            }
        }
    }
}