using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Encrypted_Messaging_App.Services
{ 
    public static class SQLiteService
    {
        // Pending Requests:
        public static class PendingRequests
        {
            private static string pendingDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "pendingRequests.db3");



            [Table("pendingRequests")]
            private class PendingRequest
            {
                [PrimaryKey]
                [Column("userID")]
                public string Id { get; set; }

                [Column("key")]
                public string privateKey
                { get; set; }
            }

            public static bool Set(string targetUserId, string encryptKey)
            {
                SQLiteConnection db = new SQLiteConnection(pendingDbPath);
                db.CreateTable<PendingRequest>();

                if(Get(targetUserId) != "")
                {
                    Delete(targetUserId);
                }

                PendingRequest request = new PendingRequest { Id = targetUserId, privateKey = encryptKey };
                try
                {
                    var results = db.Insert(request);
                    db.Close();
                    return true;
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Unable to set encryptKey for {targetUserId}: {e.Message}");
                    db.Close();
                    return false;
                }
            }
            public static bool Delete(string targetUserId)
            {
                SQLiteConnection db = new SQLiteConnection(pendingDbPath);
                db.CreateTable<PendingRequest>();
                try
                {
                   
                    Console.WriteLine("Created table");
                    db.Delete<PendingRequest>(targetUserId);
                    db.Close();
                    return true;
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Unable to delete {targetUserId}: {e.Message}");
                    db.Close();
                    return false;
                }
            }
            public static string Get(string targetUserId)
            {
                SQLiteConnection db = new SQLiteConnection(pendingDbPath);
                db.CreateTable<PendingRequest>();

                try
                {
                    List<PendingRequest> targetRequests = db.Table<PendingRequest>().Where(request => request.Id == targetUserId).ToList();
                    db.Close();
                    if (targetRequests.Count > 0) {
                        return targetRequests[0].privateKey;
                    }
                    else { 
                        return ""; 
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Unable to get encryptKey for {targetUserId}: {e.Message}");
                    db.Close();
                    return "";
                }
            }
        }


        // Chat Keys:
        public static class ChatKeys
        {
            private static string pendingDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "chatKeys.db3");

            [Table("chatKeys")]
            private class ChatKey
            {
                [PrimaryKey]
                [Column("chatID")]
                public string Id { get; set; }

                [Column("key")]
                public string privateKey
                { get; set; }
            }

            public static void Set(string chatId, string encryptKey)
            {
                SQLiteConnection db = new SQLiteConnection(pendingDbPath);
                db.CreateTable<ChatKey>();

                if (Get(chatId) != "")
                {
                    Delete(chatId);
                }

                ChatKey request = new ChatKey { Id = chatId, privateKey = encryptKey };
                db.Insert(request);

                db.Close();
            }
            public static void Delete(string targetUserId)
            {
                SQLiteConnection db = new SQLiteConnection(pendingDbPath);
                db.Delete<ChatKey>(targetUserId);
                db.Close();
            }
            public static string Get(string chatId)
            {
                SQLiteConnection db = new SQLiteConnection(pendingDbPath);
                db.CreateTable<ChatKey>();

                List<ChatKey> targetRequests = db.Table<ChatKey>().Where(chatKey => chatKey.Id == chatId).ToList();
                db.Close();
                if (targetRequests.Count > 0) {
                    return targetRequests[0].privateKey;
                }
                else {
                    return "";
                }
            }
        }

    }
}
