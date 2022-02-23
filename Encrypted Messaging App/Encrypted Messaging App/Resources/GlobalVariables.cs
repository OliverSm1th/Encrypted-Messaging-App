using System;
using System.Collections.Generic;
using System.Text;
using Encrypted_Messaging_App.Encryption;

namespace Encrypted_Messaging_App.Views
{


    public static class GlobalVariables
    {
        public static CUser CurrentUser;
        public static Dictionary<string, DiffieHellman> PendingRequests = new Dictionary<string, DiffieHellman>();
        public static bool DeveloperMode = true;
        public static Chat CurrentChat;
        public static int SecurityLevel = 192;   //128 or 192 or 256    -Length used in AES and DH

        public static string CurrentTheme = Functions.defaultThemeName;
    }
}
