using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Encrypted_Messaging_App
{
    public interface IAuthenticationService
    {
        bool isSignedIn();
        Task<(bool, string)> LogIn(string email, string password);
        bool LogOut();
        Task<(bool, string)> Register(string username, string email, string password);

        Task<bool> ForgotPassword(string email);
    }
}
