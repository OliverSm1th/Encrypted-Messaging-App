using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App.Views
{


    public static class GlobalVariables
    {
        public static CUser CurrentUser;
        public static Dictionary<string, DiffieHellman> PendingRequests = new Dictionary<string, DiffieHellman>();
        public static bool DeveloperMode = true;
        public static Chat CurrentChat;
    }
}
