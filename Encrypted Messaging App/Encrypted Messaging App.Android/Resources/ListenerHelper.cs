using Android.Runtime;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Encrypted_Messaging_App.LoggerService;


namespace Encrypted_Messaging_App.Droid.Resources
{
     class ListenerHelper        // Converting:  Firebase Output > Classes
    {
        public class ParseStatus
        {   // Combines a success and error message for debugging
            public bool success = true;
            public string errorMessage;

            public ParseStatus(bool parseSuccess, string errorMsg) {
                success = parseSuccess; errorMessage = errorMsg;
            }

            public ParseStatus(bool parseSuccess) { success = parseSuccess; }
        }




        Dictionary<Type, string> returnTypes = new Dictionary<Type, string>();
        public ListenerHelper()
        {   // Populate returnTypes to be used in ParseObject   (Type:methodName)
            MethodInfo[] methods = this.GetType().GetMethods();  
            foreach (MethodInfo method in methods)
            {   Type currentType = method.ReturnType;
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(System.ValueTuple<,>))
                {   Type secondType = currentType.GetFields()[1].FieldType;
                    if (!returnTypes.ContainsKey(secondType))
                    {
                        returnTypes[secondType] = method.Name;
                    }
                }
            }
        }

        



        
        public (bool, object) ParseObject(object input, Type expectedType)
        {   // Find the appropirate Parse[Type] method to put the result through
            Debug($"Parsing: {expectedType.Name}", includeMethod: true, indentationLvl: 1);
            if (!returnTypes.ContainsKey(expectedType))
            {   Error($"Unexpected type {expectedType.Name}", indentationLvl: 2);
                return (false, $"Type to convert to {expectedType.Name} not in Helper Method (2)");
            }

            string methodName = returnTypes[expectedType];

            MethodInfo helperMethod = this.GetType().GetMethod(methodName);

            bool valid = true;
            if (helperMethod == null) {  // Error Handling
                Error($"Unexpected type {methodName} for Helper Method (not included)", 2); valid = false;
            } else if (helperMethod.GetParameters().Length == 0) {
                Error($"No parameters specified for target method ({methodName})", 2); valid = false;
            } else if (helperMethod.GetParameters()[0].ParameterType != input.GetType()) {
                Error($"Expected Parameter Type: {this.GetType().GetMethod(methodName).GetParameters()[0]}\nParameter Type Provided: {input.GetType()}  ({methodName})", 2); valid = false;
            }

            if (valid)
            {   Debug($"{expectedType.Name}- Invoking method: {helperMethod.Name}", 1);
                object resultFromMethod = helperMethod.Invoke(this, new object[] { input });

                (bool, object) result = CovertReturnedTuple((ITuple)resultFromMethod);
                Debug($"{expectedType.Name}- Received result from method: ({result.Item1}, {result.Item2})", 2);

                return result;
            }  else {
                return (false, "Invalid type/params given to ParseObject");
            }
        }



        private T ToObject<T>(JavaDictionary dict) where T : class, new()
        {   // Used to convert to simple objects  (object made of just String/Int/BigInt)
            var instance = new T();
            Type type = instance.GetType();

            IEnumerator keyEnumerator = dict.Keys.GetEnumerator();
            IEnumerator valueEnumerator = dict.Values.GetEnumerator();
            keyEnumerator.MoveNext();
            valueEnumerator.MoveNext();

            for (int i = 0; i < dict.Keys.Count; i++)
            {
                if (keyEnumerator.Current == null) { continue; }
                string propName = keyEnumerator.Current.ToString();
                if (propName != null)
                {
                    PropertyInfo prop = type.GetProperty(propName);
                    if (prop.PropertyType.IsArray && valueEnumerator.Current is JavaList valueList)
                    {
                        object[] result = ParseEnumerator(valueList.GetEnumerator(), valueList.Count);
                        if (prop.PropertyType == typeof(string[])) { prop.SetValue(instance, ConvertObjArr(result)); }
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
                    else
                    {
                        Error($"Null property: {prop.Name}");
                    }
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

        public (ParseStatus, Request[]) ParseRequests(DocumentSnapshot[] docs)
        {   List<Request> requests = new List<Request>();

            foreach (DocumentSnapshot doc in docs)
            {   JavaDictionary data = (JavaDictionary)doc.Data;
                Request request = ParseRequest(data);
                if (request != null) { requests.Add(request); }
            }
            return (new ParseStatus(true), requests.ToArray());
        }
        public (ParseStatus, Request) ParseRequest (DocumentSnapshot doc)
        {   JavaDictionary data = (JavaDictionary)doc.Data;
            Request request = ParseRequest(data);
            if(request != null) { return (new ParseStatus(true), request); }
            else { return (new ParseStatus(false), null); }
        }
        private Request ParseRequest(JavaDictionary data)
        {   if (data != null)
            {   if (data["EncryptionInfo"] is JavaDictionary encryptionDict && data["SourceUser"] is JavaDictionary userDict) {
                    KeyData encryptionInfo = ToObject<KeyData>(encryptionDict);
                    User user = ToObject<User>(userDict);
                    if (encryptionInfo == null || user == null) { return null; }
                    return new Request(encryptionInfo, user);
                } else {
                    Error("Invalid document passed");
                    return null;
                }
            }
            return null;
        }

        public (ParseStatus, AcceptedRequest[]) ParseAcceptRequests(DocumentSnapshot[] docs)
        {   List<AcceptedRequest> requests = new List<AcceptedRequest>();

            foreach (DocumentSnapshot doc in docs)
            {   JavaDictionary data = (JavaDictionary)doc.Data;
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
            {   AcceptedRequest request = new AcceptedRequest(chatID, ToObject<KeyData>(encryptionDict));
                request.requestUserID = requestUserID;
                return request;
            } else { 
                Error($"Invalid document passed");  return null; }
        }

        public (ParseStatus, Chat) ParseChat(DocumentSnapshot doc)
        {
            List<Message> messages = new List<Message>();
            List<User> users = new List<User>();
            List<string> userIDs = new List<string>();
            string title = null;
            KeyData encryptInfo = null;

            try
            {   if (doc.Get("userIDs") is JavaList usersObj)
                {   object[] userArr = ParseEnumerator(usersObj.GetEnumerator(), usersObj.Count);

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
                if (doc.Get("messages") is JavaList messagesObj)
                {   
                    for (int i = 0; i < messagesObj.Count; i++)
                    {   JavaDictionary messageObj = (JavaDictionary)messagesObj.Get(i);
                        Message messageResult = ParseMessage(messageObj, i);

                        if (messageResult != null) { messages.Add(messageResult); }
                        else { Error("Unable to parse message"); }
                    }
                }

                Chat chat = new Chat { messages = messages.ToArray(), userIDs = userIDs.ToArray(), title = title, encryptionInfo = encryptInfo };
                return (new ParseStatus(true), chat);
            }
            catch (Exception e)
            {   Error($"Failed settings values: {e.Message}");
                return (new ParseStatus(false, "Failed to get chats"), null);
            }
        }
        public Message ParseMessage(JavaDictionary data, int messageIndex)
        {
            if(data["author"] is JavaDictionary authorDict 
                && data["encryptedContent"].ToString() is string content
                && long.TryParse(data["createdTime"].ToString(), out long createdTime)) {
                User author = ToObject<User>(authorDict);
                return new Message { author = author, encryptedContent = content, createdTime = DateTime.FromBinary(createdTime), index= messageIndex };
            }
            return null;
        }

        public (ParseStatus, CUser) ParseCUser(DocumentSnapshot doc)
        {   // Private user info: chatsID
            CUser user = new CUser(doc.Id, FirebaseAuth.Instance.CurrentUser.DisplayName);
            try
            {   // Chats:
                if (doc.Get("chatsID") is JavaList chats)
                {   string[] chatsArr = ConvertObjArr(ParseEnumerator(chats.GetEnumerator(), chats.Count));

                    user.chatsID = chatsArr;
                }
                return (new ParseStatus(true), user);
            }
            catch (Exception e)
            {   Error($"Failed settings values: {e.Message}", 3);
                return (new ParseStatus(false, "Failed to get user"), null);  }
        }
        public (ParseStatus, User) ParseUser(DocumentSnapshot doc)
        {   // A user can be fetched from 2 different places: usersPublic/[username]/User   or  userPublicID/[id]/User
            User user = new User();
            
            if (doc.Get("Username") != null)  {
                user.Username = (string)doc.Get("Username");
                user.Id = doc.Id;
            }  else if(doc.Get("Id") != null)
            {   user.Id = (string)doc.Get("Id");
                user.Username = doc.Id;
            }  else {
                return (new ParseStatus(false, $"User object doesn't contain \'Username\' or \'Id\'"), null);
            }

            return (new ParseStatus(true), user);
        }
        public (ParseStatus, string[]) ParseStringArr(JavaList list)
        {
            try
            {
                object[] result = ParseEnumerator(list.GetEnumerator(), list.Count);
                return (new ParseStatus(true), ConvertObjArr(result));
            } catch(Exception e) {
                return (new ParseStatus(false, "Failed to ParseEnumerator"), null);
            }
        }


        // General methods used during conversion:
        public object[] ParseEnumerator(IEnumerator enumerator, int length)
        {   // Enumerator -> Object[]
            // Enumerators need to be looped through, using MoveNext to see the next value
            object[] array = new object[length];
            Debug("Parse Enumerator:");
            enumerator.MoveNext();
            for (int i = 0; i < length; i++)
            {   Debug($"{i}: {enumerator.Current}",1);
                if (enumerator.Current is JavaDictionary currentDict)
                {
                    array[i] = JavaDictToDict(currentDict);
                } else {
                    array[i] = enumerator.Current;
                    enumerator.MoveNext();
                }
            }
            return array;
        }
        private string[] ConvertObjArr(object[] input)
        {   // e.g object[] -> string[]
            return Array.ConvertAll(input, x => x.ToString());
        }
        
        private Dictionary<string, object> JavaDictToDict(JavaDictionary dict)
        {  // Used in ParseEnumerator

            string[] keyArr = (string[])ParseEnumerator(dict.Keys.GetEnumerator(), dict.Keys.Count);
            object[] valueArr = ParseEnumerator(dict.Values.GetEnumerator(), dict.Values.Count);

            Dictionary<string, object> newDict = new Dictionary<string, object>();
            for (int i = 0; i < keyArr.Length; i++) {
                newDict.Add(keyArr[i], valueArr[i]);
            }
            return newDict;
        }
        private (bool, object) CovertReturnedTuple(ITuple tuple)
        {   // Extracts the data from ParseStatus object
            if (tuple == null)
            {   Error("No tuple given");
                return (false, "No tuple given to tuple converter"); }

            ParseStatus status = (ParseStatus)tuple[0];
            if (status.success) {
                return (true, (object)tuple[1]);
            } else {
                return (false, status.errorMessage);
            }
        }
    }
}