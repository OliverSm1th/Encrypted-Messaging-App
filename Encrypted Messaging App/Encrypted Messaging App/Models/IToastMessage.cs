using System;
using System.Collections.Generic;
using System.Text;

namespace Encrypted_Messaging_App
{
    public interface IToastMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
