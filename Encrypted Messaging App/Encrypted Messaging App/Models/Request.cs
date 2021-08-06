using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Encrypted_Messaging_App
{
    public class KeyData
    {
        public BigInteger prime { get; set; }
        public int global { get; set; }
        public BigInteger A_Key { get; set; }
        public BigInteger B_Key { get; set; }
        public KeyData(BigInteger p, int g, BigInteger secret, int userNum)
        {
            prime = p;
            global = g;
            if (userNum == 0) { A_Key = secret; }
            else { B_Key = secret; }
        }
        public String output()
        {
            return $"Prime: {prime}\nGlobal: {global}\nA Key: {A_Key}\nB Key: {B_Key}";
        }
    }




    public class Request
    {
        public string userID { get; set; }
        public User user;
        public KeyData EncryptionInfo;

        public Request()
        {

        }

        public void setSample()
        {
            user = new User("0000", "TestUsername");
            EncryptionInfo = new KeyData(500000, 3, 6969, 0);
            EncryptionInfo.B_Key = 42420;
        }
    }
}
