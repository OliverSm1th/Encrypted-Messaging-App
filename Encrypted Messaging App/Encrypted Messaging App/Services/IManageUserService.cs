using Encrypted_Messaging_App.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Encrypted_Messaging_App
{
    public interface IManageUserService
    {
        Task<CUser> GetUser();


    }
}
