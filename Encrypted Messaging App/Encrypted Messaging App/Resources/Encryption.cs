using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;


namespace Encrypted_Messaging_App.Encryption
{

    public class DiffieHellman
    {
        private BigInteger prime;
        private int global;
        private BigInteger userKey;

        int user;
        bool debug;
        char p_char;

        private RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        private int[] identifiers = { 5, 14, 15, 16, 17, 18 };
        private string[] primes =  {
            "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA237327FFFFFFFFFFFFFFFF",
            "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AACAA68FFFFFFFFFFFFFFFF",
            "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6BF12FFA06D98A0864D87602733EC86A64521F2B18177B200CBBE117577A615D6C770988C0BAD946E208E24FA074E5AB3143DB5BFCE0FD108E4B82D120A93AD2CAFFFFFFFFFFFFFFFF",
            "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6BF12FFA06D98A0864D87602733EC86A64521F2B18177B200CBBE117577A615D6C770988C0BAD946E208E24FA074E5AB3143DB5BFCE0FD108E4B82D120A92108011A723C12A787E6D788719A10BDBA5B2699C327186AF4E23C1A946834B6150BDA2583E9CA2AD44CE8DBBBC2DB04DE8EF92E8EFC141FBECAA6287C59474E6BC05D99B2964FA090C3A2233BA186515BE7ED1F612970CEE2D7AFB81BDD762170481CD0069127D5B05AA993B4EA988D8FDDC186FFB7DC90A6C08F4DF435C934063199FFFFFFFFFFFFFFFF",
            "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6BF12FFA06D98A0864D87602733EC86A64521F2B18177B200CBBE117577A615D6C770988C0BAD946E208E24FA074E5AB3143DB5BFCE0FD108E4B82D120A92108011A723C12A787E6D788719A10BDBA5B2699C327186AF4E23C1A946834B6150BDA2583E9CA2AD44CE8DBBBC2DB04DE8EF92E8EFC141FBECAA6287C59474E6BC05D99B2964FA090C3A2233BA186515BE7ED1F612970CEE2D7AFB81BDD762170481CD0069127D5B05AA993B4EA988D8FDDC186FFB7DC90A6C08F4DF435C93402849236C3FAB4D27C7026C1D4DCB2602646DEC9751E763DBA37BDF8FF9406AD9E530EE5DB382F413001AEB06A53ED9027D831179727B0865A8918DA3EDBEBCF9B14ED44CE6CBACED4BB1BDB7F1447E6CC254B332051512BD7AF426FB8F401378CD2BF5983CA01C64B92ECF032EA15D1721D03F482D7CE6E74FEF6D55E702F46980C82B5A84031900B1C9E59E7C97FBEC7E8F323A97A7E36CC88BE0F1D45B7FF585AC54BD407B22B4154AACC8F6D7EBF48E1D814CC5ED20F8037E0A79715EEF29BE32806A1D58BB7C5DA76F550AA3D8A1FBFF0EB19CCB1A313D55CDA56C9EC2EF29632387FE8D76E3C0468043E8F663F4860EE12BF2D5B0B7474D6E694F91E6DCC4024FFFFFFFFFFFFFFFF",
            "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6BF12FFA06D98A0864D87602733EC86A64521F2B18177B200CBBE117577A615D6C770988C0BAD946E208E24FA074E5AB3143DB5BFCE0FD108E4B82D120A92108011A723C12A787E6D788719A10BDBA5B2699C327186AF4E23C1A946834B6150BDA2583E9CA2AD44CE8DBBBC2DB04DE8EF92E8EFC141FBECAA6287C59474E6BC05D99B2964FA090C3A2233BA186515BE7ED1F612970CEE2D7AFB81BDD762170481CD0069127D5B05AA993B4EA988D8FDDC186FFB7DC90A6C08F4DF435C93402849236C3FAB4D27C7026C1D4DCB2602646DEC9751E763DBA37BDF8FF9406AD9E530EE5DB382F413001AEB06A53ED9027D831179727B0865A8918DA3EDBEBCF9B14ED44CE6CBACED4BB1BDB7F1447E6CC254B332051512BD7AF426FB8F401378CD2BF5983CA01C64B92ECF032EA15D1721D03F482D7CE6E74FEF6D55E702F46980C82B5A84031900B1C9E59E7C97FBEC7E8F323A97A7E36CC88BE0F1D45B7FF585AC54BD407B22B4154AACC8F6D7EBF48E1D814CC5ED20F8037E0A79715EEF29BE32806A1D58BB7C5DA76F550AA3D8A1FBFF0EB19CCB1A313D55CDA56C9EC2EF29632387FE8D76E3C0468043E8F663F4860EE12BF2D5B0B7474D6E694F91E6DBE115974A3926F12FEE5E438777CB6A932DF8CD8BEC4D073B931BA3BC832B68D9DD300741FA7BF8AFC47ED2576F6936BA424663AAB639C5AE4F5683423B4742BF1C978238F16CBE39D652DE3FDB8BEFC848AD922222E04A4037C0713EB57A81A23F0C73473FC646CEA306B4BCBC8862F8385DDFA9D4B7FA2C087E879683303ED5BDD3A062B3CF5B3A278A66D2A13F83F44F82DDF310EE074AB6A364597E899A0255DC164F31CC50846851DF9AB48195DED7EA1B1D510BD7EE74D73FAF36BC31ECFA268359046F4EB879F924009438B481C6CD7889A002ED5EE382BC9190DA6FC026E479558E4475677E9AA9E3050E2765694DFC81F56E880B96E7160C980DD98EDD3DFFFFFFFFFFFFFFFFF"
        };

        public DiffieHellman(int secret_size = 256, bool p_debug = false, char player_char = '\0')   // (Both users) Generates user secret
        {
            // Create user secret
            byte[] b_secret = new byte[secret_size];
            rngCsp.GetBytes(b_secret);
            userKey = BitConverter.ToUInt32(b_secret, 0);

            debug = p_debug; p_char = player_char;
            if (debug) { Console.WriteLine($"User {p_char} Secret Key: {userKey}"); }
        }
        public DiffieHellman(BigInteger privateKey)   // Setting DH from local storage
        {
            // Save user secret
            userKey = privateKey;
        }

        public KeyData Initilise(int p_id = 14, int g = 5)                                             // (First user) Initilise Prime and Global values  |  Calculate their public key- A
        {
            // Prime (p): 
            prime = getPrime(p_id);

            //Base (g):
            global = g;

            //A: Calculate public key
            BigInteger publicKey = getPublicKey();
            user = 0;

            if (debug)
            {
                Console.WriteLine($"\n---Initilise--     (User {p_char})");
                Console.WriteLine($"Prime(p): {prime.ToByteArray().Length} bytes");
                Console.WriteLine($"Base(g): {g}");
                Console.WriteLine($"User {p_char} Public Key: {publicKey.ToString("X")}");
            }

            return new KeyData(prime, g, publicKey, 0);
        }

        public KeyData Respond(KeyData data)                                                         // (Second user) Use values to calculate their public key- B
        {
            prime = data.prime;
            global = data.global;
            BigInteger publicKey = getPublicKey();
            user = 1;
            data.B_Key = publicKey;

            if (debug)
            {
                Console.WriteLine($"\n---Respond---   (User {p_char})");
                Console.WriteLine("Defined public variables");
                Console.WriteLine($"User {p_char} Public Key: {publicKey.ToString("X")}");
            }

            return data;
        }

        public BigInteger getSharedKey(KeyData data)                                                 // (Both users) Calculate Shared Master Key:  User 1: B^a  mod  p | User 2: A^b  mod  p
        {
            BigInteger request_secret;
            if (user == 0) { request_secret = data.B_Key; }
            else { request_secret = data.A_Key; }

            BigInteger sharedKey = BigInteger.ModPow(request_secret, userKey, data.prime);
            if (debug) { Console.WriteLine($"Shared Key calculated by {p_char}: {sharedKey}"); }

            return sharedKey;
        }

        private BigInteger getPrime(int p_id)    // [Get required prime from array]
        {
            int prime_num = Array.IndexOf(identifiers, p_id);
            if (prime_num == -1) { prime_num = 1; }

            return BigInteger.Parse(primes[prime_num], System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        private BigInteger getPublicKey()        // [Calculate public key - g^(a/b) mod p]
        {
            BigInteger publicKey = BigInteger.ModPow(global, userKey, prime);
            return publicKey;
        }

        public BigInteger getPrivateKey()        // Get private key for storing locally on device
        {
            return userKey;
        }

        public void setPrivateKey(BigInteger newKey)  // TEST ONLY
        { userKey = newKey; }
    }


    public class AES
    {                   // Bytes:   16   24   32 
        public int[] lengths = { 128, 192, 256 };
        public int[] rounds = { 10, 12, 14 };
        public int keyIndex;     // 0,1,2
        public int keyLength;    // Length of key in bits: 128, 192, 256
        public int b_keyLength;  // Length of key in bytes: 16, 24, 32
        public int keyRounds;    // Number of rounds for key: 10, 12, 14
        public int wordsPerRound; // Number of words required for each round:  4, 6, 8

        private int[,] s_box = // Box used for g() in Key Schedule and SubBytes
        {   // 0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F
            {0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76}, // 0
            {0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0}, // 1
            {0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15}, // 2
            {0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75}, // 3
            {0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84}, // 4
            {0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf}, // 5
            {0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8}, // 6
            {0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2}, // 7
            {0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73}, // 8
            {0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb}, // 9
            {0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79}, // A
            {0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08}, // B
            {0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a}, // C
            {0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e}, // D
            {0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf}, // E
            {0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16}  // F
        };
        private int[,] inv_s_box =
        {   // 0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F
            {0x52, 0x09, 0x6a, 0xd5, 0x30, 0x36, 0xa5, 0x38, 0xbf, 0x40, 0xa3, 0x9e, 0x81, 0xf3, 0xd7, 0xfb}, // 0
            {0x7c, 0xe3, 0x39, 0x82, 0x9b, 0x2f, 0xff, 0x87, 0x34, 0x8e, 0x43, 0x44, 0xc4, 0xde, 0xe9, 0xcb}, // 1
            {0x54, 0x7b, 0x94, 0x32, 0xa6, 0xc2, 0x23, 0x3d, 0xee, 0x4c, 0x95, 0x0b, 0x42, 0xfa, 0xc3, 0x4e}, // 2
            {0x08, 0x2e, 0xa1, 0x66, 0x28, 0xd9, 0x24, 0xb2, 0x76, 0x5b, 0xa2, 0x49, 0x6d, 0x8b, 0xd1, 0x25}, // 3
            {0x72, 0xf8, 0xf6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xd4, 0xa4, 0x5c, 0xcc, 0x5d, 0x65, 0xb6, 0x92}, // 4
            {0x6c, 0x70, 0x48, 0x50, 0xfd, 0xed, 0xb9, 0xda, 0x5e, 0x15, 0x46, 0x57, 0xa7, 0x8d, 0x9d, 0x84}, // 5
            {0x90, 0xd8, 0xab, 0x00, 0x8c, 0xbc, 0xd3, 0x0a, 0xf7, 0xe4, 0x58, 0x05, 0xb8, 0xb3, 0x45, 0x06}, // 6
            {0xd0, 0x2c, 0x1e, 0x8f, 0xca, 0x3f, 0x0f, 0x02, 0xc1, 0xaf, 0xbd, 0x03, 0x01, 0x13, 0x8a, 0x6b}, // 7
            {0x3a, 0x91, 0x11, 0x41, 0x4f, 0x67, 0xdc, 0xea, 0x97, 0xf2, 0xcf, 0xce, 0xf0, 0xb4, 0xe6, 0x73}, // 8
            {0x96, 0xac, 0x74, 0x22, 0xe7, 0xad, 0x35, 0x85, 0xe2, 0xf9, 0x37, 0xe8, 0x1c, 0x75, 0xdf, 0x6e}, // 9
            {0x47, 0xf1, 0x1a, 0x71, 0x1d, 0x29, 0xc5, 0x89, 0x6f, 0xb7, 0x62, 0x0e, 0xaa, 0x18, 0xbe, 0x1b}, // A
            {0xfc, 0x56, 0x3e, 0x4b, 0xc6, 0xd2, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0x78, 0xcd, 0x5a, 0xf4}, // B
            {0x1f, 0xdd, 0xa8, 0x33, 0x88, 0x07, 0xc7, 0x31, 0xb1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xec, 0x5f}, // C
            {0x60, 0x51, 0x7f, 0xa9, 0x19, 0xb5, 0x4a, 0x0d, 0x2d, 0xe5, 0x7a, 0x9f, 0x93, 0xc9, 0x9c, 0xef}, // D
            {0xa0, 0xe0, 0x3b, 0x4d, 0xae, 0x2a, 0xf5, 0xb0, 0xc8, 0xeb, 0xbb, 0x3c, 0x83, 0x53, 0x99, 0x61}, // E
            {0x17, 0x2b, 0x04, 0x7e, 0xba, 0x77, 0xd6, 0x26, 0xe1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0c, 0x7d}  // F
        };

        public bool b_debug;
        public string b_debug_type; // 64, hex, bin
        public string[] debug_Blacklist = new string[] { "ScheduleKey", "ScheduleFunction", "MixColumns", "InvMixColumns", "MixMultiply" };
        //public string[] debug_Blacklist = new string[] { };




        public class WordArr
        {
            private Byte[,] Words;
            private AES current;

            public WordArr(Byte[,] p_Words, AES p_current)
            {
                Words = p_Words;
                current = p_current;
            }

            public Byte[] GetWord(int index) // Gets the bytes for word at index
            {
                Byte[] Word = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    Word[j] = Words[index, j];
                }
                return Word;
            }
            public Byte[] GetWords(int start, int end) // Gets the bytes for the words in the range
            {
                if (start < 0 | end >= Words.Length | end < start)
                {
                    Console.WriteLine($"Invalid start/end given: {start}, {end} for length {Words.Length}");
                    return new byte[(end - start) * 4];
                }
                Byte[] result = new byte[(end - start) * 4];
                for (int i = start; i < end; i++)
                {
                    byte[] currentWord = GetWord(i);
                    currentWord.CopyTo(result, 4 * (i - start));
                }
                return result;
            }
            public string Output(bool showEmpty = false)
            {
                string output = "";
                for (int i = 0; i < Words.GetLength(0); i++)
                {
                    Byte[] row = new byte[Words.GetLength(1)];
                    bool empty = true;
                    for (int j = 0; j < Words.GetLength(1); j++)
                    {
                        //Console.WriteLine($"i:{i}; j:{j}");
                        row[j] = Words[i, j];
                        if (Words[i, j].ToString() != "0")
                        {
                            empty = false;
                        }
                    }
                    if (!empty)
                    {
                        string s_row = current.ConvertByteArr(row);
                        output = output + $"w{i}: {s_row.Substring(0, s_row.Length)}" + "\n";
                    }
                    else if (showEmpty) { output += "-\n"; }
                }
                return output;
            }
            public int GetLength(int i)
            {
                return Words.GetLength(i);
            }
            public Byte[] SetWord(Byte[] Word, int index)
            {
                for (int i = 0; i < Words.GetLength(1); i++)
                {
                    Words[index, i] = Word[i];
                }
                return Word;
            }

            public Byte[,] value // Testing only
            {
                get { return Words; }
            }
        }


        public AES(int level = 192, bool p_debug = false, string debug_type = "64") // Constructor
        {
            keyIndex = Array.IndexOf(lengths, level);
            if (keyIndex == -1) { keyIndex = 1; }

            keyLength = lengths[keyIndex];
            b_keyLength = keyLength / 8;
            keyRounds = rounds[keyIndex];
            wordsPerRound = b_keyLength / 4; ;


            b_debug = p_debug;
            b_debug_type = debug_type;
            debug($"Initilised AES({level}):\n    Key Index = {keyIndex}\n    Key Length = {keyLength}\n    Byte Key Length = {b_keyLength}\n    Key Rounds = {keyRounds}\n--------\n");
        }

        // Main Encrypt Functions
        public byte[] Encrypt(byte[] sharedKey, byte[] b_message, bool decrypt = false)
        {
            //   --- KEY ---
            // Hex Key -> Byte Key


            // Resize key
            if (sharedKey.Length < b_keyLength)
            {
                debug("Error: Shared Key is too small");
                return new byte[0];
            }
            else if (sharedKey.Length > b_keyLength)
            {
                debug($"Decreased Shared Key size ({sharedKey.Length}->{b_keyLength})");
                sharedKey = SHA256.Create().ComputeHash(sharedKey);
                Array.Resize(ref sharedKey, b_keyLength);
                debug($"New Key:  {ConvertByteArr(sharedKey)}");
            }

            // Enlarge key
            WordArr l_sharedKey = ScheduleKey(sharedKey);


            // Resize message

            if (b_message.Length % 16 > 0)
            {
                Array.Resize(ref b_message, ((b_message.Length / 16) + 1) * 16);
            }


            //  --- MESSAGE ---

            // Split the message into 128-bit blocks (16-byte)
            byte[] cipher = new byte[Math.Max(b_message.Length, 16)];
            byte[] message_128 = new byte[16];
            for (int i = 0; i < b_message.Length; i++)
            {
                message_128[i % 16] = b_message[i];
                if (i % 16 == 15) // Reached end of 126-bit message
                {
                    debug($"Sub-Message {i / 16}:");
                    if (decrypt)
                    {
                        DecryptByte(l_sharedKey, message_128).CopyTo(cipher, i - 15);
                    }
                    else
                    {
                        EncryptByte(l_sharedKey, message_128).CopyTo(cipher, i - 15);
                    }


                    message_128 = new byte[16];
                }
                debug($"{i}");
            }
            debug($"All 128-bit segments added: {ConvertByteArr(cipher)}");
            /*if (b_message.Length % 16 > 0)
            {
                if (decrypt)
                {
                    DecryptByte(l_sharedKey, message_128).Take((b_message.Length % 16)).ToArray().CopyTo(cipher, cipher.Length - 16);
                }
                else
                {
                    EncryptByte(l_sharedKey, message_128).CopyTo(cipher, cipher.Length - 16);   //.Take((b_message.Length % 16)).ToArray()
                }

                debug($"Sub-Message {b_message.Length / 16}:");
            }*/


            return cipher;
        }

        private byte[] EncryptByte(WordArr sharedKey, byte[] message) // Main Encryption Method
        {
            //WordArr sharedKey = new WordArr(l_sharedKey, this);

            debug($"Encrypt Sub-Message:\n{OutputMessageArr(message)}");

            //byte[,] sharedKey = p_sharedKey;
            //tex: $$\text{msg}\  \otimes [w_0,w_1,w_2,w_3]\ \ \ \ \  \small(192\rightarrow w_5, 256\rightarrow w_7)$$
            byte[] currentKey = sharedKey.GetWords(0, 4);

            debug($"  FirstBytes: {ConvertByteArr(currentKey)}", true);
            message = ByteXOR(message, currentKey);
            debug($"  Message XOR {ConvertByteArr(currentKey)} -> \n{OutputMessageArr(message)}", true);

            for (int roundNum = 1; roundNum < (keyRounds + 1); roundNum++)
            {
                debug($"\n\n --- Round {roundNum} ---\n{OutputMessageArr(message)}", true);
                currentKey = sharedKey.GetWords(4 * roundNum, 4 * (roundNum + 1));

                SubBytes(ref message);
                debug($"  SubBytes:\n{OutputMessageArr(message)}");

                ShiftRows(ref message);
                debug($"  ShiftRows:\n{OutputMessageArr(message)}");

                if (roundNum != keyRounds)
                {
                    MixColumns(ref message);
                    debug($"  MixColumns:\n{OutputMessageArr(message)}");
                }
                message = ByteXOR(message, currentKey);
                debug($"  Round Key XOR:  {ByteToHex(currentKey)}");

            }
            return message;
        }

        // Main Decrypt Functions
        public byte[] Decrypt(byte[] sharedKey, byte[] cipher)
        {
            // As the decrypt function is so simular to the encrypt function just with a different function called at the end,
            // I call the Encrypt function, with the bool at the end signifying it's decrypt
            return Encrypt(sharedKey, cipher, true);
        }

        public byte[] DecryptOld(string s_key, byte[] cipher, bool hexCipher = false)
        {
            //   --- KEY ---
            // Hex Key -> Byte Key
            int x = 0;
            byte[] sharedKey = new byte[s_key.Length / 2];
            for (int i = 0; i < s_key.Length; i += 2)
            {
                int d_key = Convert.ToInt32(s_key.Substring(i, 2), 16);
                sharedKey[x] = Convert.ToByte(d_key);
                x++;
            }

            // Resize key
            if (sharedKey.Length < b_keyLength)
            {
                debug("Error: Shared Key is too small");
                return new byte[0];
            }
            else if (sharedKey.Length > b_keyLength)
            {
                debug($"Decreased Shared Key size ({sharedKey.Length}->{b_keyLength})");
                sharedKey = SHA256.Create().ComputeHash(sharedKey);
                Array.Resize(ref sharedKey, b_keyLength);
                debug($"New Key:  {ConvertByteArr(sharedKey)}");
            }

            // Enlarge key
            WordArr l_sharedKey = ScheduleKey(sharedKey);


            byte[] b_message = new byte[cipher.Length];
            byte[] cipher_128 = new byte[16];
            for (int i = 0; i < cipher.Length; i++)
            {
                cipher_128[i % 16] = cipher[i];
                if (i % 16 == 15) // Reached end of 126-bit message
                {
                    debug($"Sub-Message {i / 16}:");
                    DecryptByte(l_sharedKey, cipher_128).CopyTo(b_message, i - 15);


                    cipher_128 = new byte[16];
                }
            }
            if (b_message.Length % 16 > 0)
            {
                DecryptByte(l_sharedKey, cipher_128).Take(b_message.Length % 16).ToArray().CopyTo(b_message, b_message.Length - 15);
                debug($"Sub-Message {cipher.Length / 16}:");
            }




            return DecryptByte(l_sharedKey, cipher);
        }
        private byte[] DecryptByte(WordArr sharedKey, byte[] cipher)
        {
            debug($"Decrypt Sub-Message:\n{OutputMessageArr(cipher)}");

            //byte[,] sharedKey = p_sharedKey;
            //tex: $$\text{msg}\  \otimes [w_0,w_1,w_2,w_3]\ \ \ \ \  \small(192\rightarrow w_5, 256\rightarrow w_7)$$
            byte[] currentKey;


            for (int roundNum = 0; roundNum < (keyRounds); roundNum++)
            {
                debug($"\n\n --- Round {roundNum} ---\n{OutputMessageArr(cipher)}", true);
                currentKey = sharedKey.GetWords(sharedKey.GetLength(0) - 4 * (roundNum + 1), sharedKey.GetLength(0) - 4 * roundNum);

                cipher = ByteXOR(cipher, currentKey);
                debug($"  Round Key XOR:  {ByteToHex(currentKey)}\n{OutputMessageArr(cipher)}", true);


                if (roundNum > 0)
                {
                    InvMixColumns(ref cipher);
                    debug($"  InvMixColumns:\n{OutputMessageArr(cipher)}");
                }

                InvShiftRows(ref cipher);
                debug($"  InvShiftRows:\n{OutputMessageArr(cipher)}");

                InvSubBytes(ref cipher);
                debug($"  InvSubBytes:\n{OutputMessageArr(cipher)}");





                //debug("");
            }

            currentKey = sharedKey.GetWords(0, 4);
            debug($"  FirstBytes: {ConvertByteArr(currentKey)}", true);
            cipher = ByteXOR(cipher, currentKey);
            debug($"  Message XOR {ConvertByteArr(currentKey)} -> \n{OutputMessageArr(cipher)}", true);

            return cipher;
        }

        // --- Schedule ---
        public WordArr ScheduleKey(Byte[] sharedKey)                  // Calculates + splits the keys for the encryption ahead
        {
            int length = ((wordsPerRound) + (4 * keyRounds));
            if (keyLength == 192) { length -= 2; }
            else if (keyLength == 256) { length -= 4; }
            Byte[,] b_Words = new byte[length, 4];   // Length of words: [44, 54, 64] - (Note that not all are required)   
            WordArr Words = new WordArr(b_Words, this);
            debug($"\n   Schedule Key\n-Created Words array (length: {Words.GetLength(0)}x{Words.GetLength(1)})");

            //tex:   HeadWord = First Word of each 4-word set ($i_0,i_4,i_8$)
            Byte[] LastHeadWord = new byte[4];
            Byte[] PreviousWord = new byte[4];

            //tex: Key for Round 0: Split key into 4/6/8 words
            //$$\{w_0, w_1,w_2,w_3\}\ \ \text{  (for 128)}$$
            for (int i = 0; i < (wordsPerRound); i++)
            {
                Byte[] Word = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    Word[j] = sharedKey[(4 * i) + j];
                    //Words[i, j] = sharedKey[(4 * i) + j];
                }
                Words.SetWord(Word, i);
            }
            debug($"\n-Split key into {wordsPerRound} words for round 0:");
            debug(Words.Output(), true);

            LastHeadWord = Words.GetWord(3);
            PreviousWord = Words.GetWord(wordsPerRound - 1);

            int currentWordNum = wordsPerRound;

            // Keys for next Rounds: 4 words each
            for (int i = 1; i <= keyRounds; i++) //
            {
                debug($"\nKey Schedule Round {i} \nw{currentWordNum}-Generate headword (word 0)"); // Current Word = i*(wordsPerRound)
                // Generate next head word

                //tex:$$w_{i+4}=w_i\otimes g(w_{i+3})$$

                Byte[] g_Previous = ScheduleFunction((byte[])PreviousWord.Clone(), i); //  g(lastWord)

                Byte[] XOR_Result = ByteXOR(g_Previous, Words.GetWord(currentWordNum - wordsPerRound));
                debug($"    {ConvertByteArr(g_Previous)} XOR {ConvertByteArr(Words.GetWord(currentWordNum - wordsPerRound))} -> {ConvertByteArr(XOR_Result)}");

                PreviousWord = Words.SetWord(XOR_Result, i * wordsPerRound);
                currentWordNum++;
                if (Words.GetLength(0) == currentWordNum)
                {
                    Console.WriteLine("Reached end!!!");
                    break;
                }
                //else { Console.WriteLine(currentWordNum); }

                // Generate other 3/5/7 words
                //tex:$$w_{i+5}=w_{i+4}\otimes w_{i+1}\\w_{i+6}=w_{i+5}\otimes w_{i+2}\\w_{i+7}=w_{i+6}\otimes w_{i+3}$$
                for (int j = 1; j < (wordsPerRound); j++)
                {
                    debug($"w{currentWordNum}-Generate word {j}");

                    if (keyLength == 256 && j == 4)
                    {
                        //Console.WriteLine("")
                        byte[] OldWord = (byte[])PreviousWord.Clone();
                        SubBytes(ref PreviousWord);
                        debug($"    Substitute: {ConvertByteArr(OldWord)}->{ConvertByteArr(PreviousWord)}");
                    }

                    XOR_Result = ByteXOR(PreviousWord, Words.GetWord(((i - 1) * wordsPerRound) + j));
                    debug($"    {ConvertByteArr(PreviousWord)} XOR {ConvertByteArr(Words.GetWord(((i - 1) * (wordsPerRound)) + j))} -> {ConvertByteArr(XOR_Result)} (w{currentWordNum - 1} XOR w{currentWordNum - wordsPerRound})");

                    PreviousWord = Words.SetWord(XOR_Result, currentWordNum);
                    currentWordNum++;
                    if (Words.GetLength(0) == currentWordNum) { break; }
                    //else { Console.WriteLine(Words.GetLength(0) ); }
                }
                if (Words.GetLength(0) == currentWordNum) { break; }
            }

            debug(Words.Output(true), true);
            return Words;
        }
        private Byte[] ScheduleFunction(Byte[] Word, int RoundNum)   // return Byte
        {
            //tex:$w_{i+4}=w_i\otimes g(w_{i+3})$
            //$$g(w_{i+3})\rightarrow S\Big( LeftShift(w_{i+3})\Big)\otimes [RC(i), 00, 00, 00]$$
            Byte[] oldWord = (Byte[])Word.Clone();

            //tex: One-byte left circular roation
            //$$[b_0,b_1, b_2, b_3]\Rightarrow [b_1,b_2,b_3,b_0]$$
            LeftShift(ref Word);
            debug($"    Left Shift: {ConvertByteArr(oldWord)}->{ConvertByteArr(Word)}");
            oldWord = (Byte[])Word.Clone();

            //tex: Byte Substitution for each byte of the word using the S-Box:
            // $$[b_1,b_2,b_3,b_0] \Rightarrow [S(b_1),\ S(b_2),\ S(b_3)]$$
            SubBytes(ref Word);
            debug($"    Substitute: {ConvertByteArr(oldWord)}->{ConvertByteArr(Word)}");
            oldWord = (Byte[])Word.Clone();
            byte RoundConstant = RConst(RoundNum);
            Word = new byte[] { (byte)(RoundConstant ^ Word[0]), Word[1], Word[2], Word[3] };
            debug($"    {ConvertByteArr(oldWord)} XOR {ConvertByteArr(new byte[] { RoundConstant })} -> {ConvertByteArr(Word)} ");
            return Word;
        }
        public byte RConst(int num)
        {
            int result = 1;
            for (int i = 1; i < num; i++)
            {
                result = 2 * result;
                if (result > 0x80)
                {
                    result = result ^ 0x11B;
                }
            }
            //Console.WriteLine($"Round Const {num} = {result}");
            return (byte)result;
        }
        public byte[] LeftShift(ref byte[] Word)   // Circular left rotation
        {
            byte temp = Word[0];
            for (int i = 0; i < Word.Length - 1; i++)
            {
                Word[i] = Word[i + 1];
            }
            Word[Word.Length - 1] = temp;

            return Word;
        }

        // --- Encrypt Sub-Functions ---
        public string OutputMessageArr(byte[] message)
        {
            //tex:Message 128:$$b_0\ \ |\ b_4\ \ |\ b_8\ \ |\ b_{12} \\b_1\ \ |\ b_5\ \ |\ b_9\ \ |\ b_{13}\\b_2\ \ |\ b_6\ \ |\ b_9\ \ |\ b_{14}\\b_3\ \ |\ b_7\ \ |\ b_{10}\ |\ b_{15}$$
            string output = "";

            for (int i = 0; i < message.Length / 4; i++)
            {
                if (i % 4 == 0) { debug(""); } // New Line
                byte num1 = message[i];
                byte num2 = new byte();
                byte num3 = new byte();
                byte num4 = new byte();
                if (i + 4 <= message.Length) { num2 = message[i + 4]; }
                if (i + 8 <= message.Length) { num3 = message[i + 8]; }
                if (i + 12 <= message.Length) { num4 = message[i + 12]; }
                output += $"{ByteToHex(new byte[] { num1, num2, num3, num4 }, " | ")}" + "\n";
            }
            return output.Substring(0, output.Length - 1);
        }

        private void SubBytes(ref Byte[] Word) // Substitute each byte of the word using the S-Box
        {
            for (int i = 0; i < Word.Length; i++)
            {
                int x = (Word[i] & 0xF0) >> 4;
                int y = Word[i] & 0x0F;
                //Console.WriteLine($"x: {x}   y: {y}");
                Word[i] = (byte)s_box[x, y];
            }
            //debug($"Substituted {ConvertByteArr(OldWord)} for {ConvertByteArr(Word)}");
        }
        private void ShiftRows(ref Byte[] Word) // Shifts each row a dfferent amount 0,1,2,3
        {
            int newIndex;
            Byte[] newWord = (Byte[])Word.Clone();

            for (int oldIndex = 0; oldIndex < Word.Length; oldIndex++)
            {
                newIndex = (oldIndex - ((oldIndex % 4) * 4));
                if (newIndex < 0) { newIndex = 16 + newIndex; }
                //debug($"{oldIndex} -> {newIndex}");
                newWord[newIndex] = Word[oldIndex];
            }
            Word = newWord;
        }
        private void MixColumns(ref Byte[] Word)
        {
            Byte[] oldWord = (Byte[])Word.Clone();

            byte[,] leftMatrix = new byte[4, 4] { { 2, 3, 1, 1 }, { 1, 2, 3, 1 }, { 1, 1, 2, 3 }, { 3, 1, 1, 2 } };

            for (int x = 0; x < 4; x++) // Columns
            {
                for (int y = 0; y < 4; y++)
                {
                    int currIndex = (x * 4);


                    byte multiplier1 = leftMatrix[y, 0];
                    byte multiplier2 = leftMatrix[y, 1];
                    byte multiplier3 = leftMatrix[y, 2];
                    byte multiplier4 = leftMatrix[y, 3];

                    Word[currIndex + y] = (byte)(MixMultiply(oldWord[currIndex], multiplier1) ^ MixMultiply(oldWord[currIndex + 1], multiplier2) ^ MixMultiply(oldWord[currIndex + 2], multiplier3) ^ MixMultiply(oldWord[currIndex + 3], multiplier4));
                    debug($"({ByteToHex(oldWord[currIndex])} * {multiplier1})  ^  ({ByteToHex(oldWord[currIndex + 1])} * {multiplier2})  ^  ({ByteToHex(oldWord[currIndex + 2])} * {multiplier3})  ^  ({ByteToHex(oldWord[currIndex + 3])} * {multiplier4})  ->  {ByteToHex(Word[currIndex + y])}");
                }
            }
        }

        // --- Decrypt Sub-Functions --
        private void InvSubBytes(ref Byte[] Word)
        {
            Byte[] OldWord = (Byte[])Word.Clone();
            for (int i = 0; i < Word.Length; i++)
            {
                int x = (Word[i] & 0xF0) >> 4;
                int y = Word[i] & 0x0F;
                //debug($"x: {x}   y: {y}");
                Word[i] = (byte)inv_s_box[x, y];
            }
            //debug($"Substituted {ConvertByteArr(OldWord)} for {ConvertByteArr(Word)}");
        }
        private void InvShiftRows(ref Byte[] Word)
        {
            int newIndex;
            Byte[] oldWord = (Byte[])Word.Clone();

            for (int oldIndex = 0; oldIndex < Word.Length; oldIndex++)
            {
                newIndex = (oldIndex + ((oldIndex % 4) * 4));
                if (newIndex > 15) { newIndex = newIndex - 16; }
                //debug($"{oldIndex}({ByteToHex(oldWord[oldIndex])}) -> {newIndex}({ByteToHex(oldWord[newIndex])})");
                Word[newIndex] = oldWord[oldIndex];
            }
        }
        private void InvMixColumns(ref Byte[] Word)
        {

            MixColumns(ref Word);
            debug("------------------------------------------");
            //byte[,] leftMatrix = new byte[4, 4] { { 14, 11, 13, 9 }, { 9, 14, 11, 13 }, { 13, 9, 14, 11 }, { 11, 13, 9, 14 } };

            Byte[] oldWord = (Byte[])Word.Clone();
            byte[,] leftMatrix = new byte[4, 4] { { 5, 0, 4, 0 }, { 0, 5, 0, 4 }, { 4, 0, 5, 0 }, { 0, 4, 0, 5 } };

            for (int x = 0; x < 4; x++) // Columns
            {
                for (int y = 0; y < 4; y++)
                {
                    int currIndex = (x * 4);


                    byte multiplier1 = leftMatrix[y, 0];
                    byte multiplier2 = leftMatrix[y, 1];
                    byte multiplier3 = leftMatrix[y, 2];
                    byte multiplier4 = leftMatrix[y, 3];

                    Word[currIndex + y] = (byte)(MixMultiply(oldWord[currIndex], multiplier1) ^ MixMultiply(oldWord[currIndex + 1], multiplier2) ^ MixMultiply(oldWord[currIndex + 2], multiplier3) ^ MixMultiply(oldWord[currIndex + 3], multiplier4));
                    debug($"({ByteToHex(oldWord[currIndex])} * {multiplier1})  ^  ({ByteToHex(oldWord[currIndex + 1])} * {multiplier2})  ^  ({ByteToHex(oldWord[currIndex + 2])} * {multiplier3})  ^  ({ByteToHex(oldWord[currIndex + 3])} * {multiplier4})  ->  {ByteToHex(Word[currIndex + y])}");
                }
            }


        }

        // General Functions
        private Byte[] ByteXOR(Byte[] Word1, Byte[] Word2)
        {
            if (Word1.Length != Word2.Length) { debug($"Invalid bytes given {Word1.Length} vs {Word2.Length}"); return new byte[4]; }
            Byte[] result = new byte[Word1.Length];
            for (int i = 0; i < Word1.Length; i++)
            {
                result[i] = (byte)(Word1[i] ^ Word2[i]);
            }
            return result;
        }
        private void debug(string message, bool ignoreConcat = false, [CallerMemberName] string method = null)
        {
            // string method = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
            if (b_debug && !debug_Blacklist.Contains(method))
            {
                if (false && message.Length > 80 && !ignoreConcat)
                {
                    message = message.Remove(30 - 3) + "..."; ;
                }
                Console.WriteLine(message);
            }
        }
        private string ConvertByteArr(Byte[] data)
        {
            string text = "";
            if (b_debug_type == "64")
            {
                text = Convert.ToBase64String(data);
            }
            else if (b_debug_type.StartsWith("hex"))  // Hex
            {
                if (b_debug_type == "hex")
                {
                    text = ByteToHex(data);
                }
                else
                {
                    text = ByteToHex(data, b_debug_type.Substring(3));
                }
                //BigInteger num = BitConverter.ToInt32(data);
                //text = num.ToString("X");
                //text = BitConverter.ToString(data).Replace('-', '\0');
                //text = String.Join(String.Empty, BitConverter.ToString(data).Split('-').Reverse());
            }
            else
            {
                foreach (byte b_data in data)
                {
                    text = text + Convert.ToString(b_data, 2).PadLeft(8, '0') + " ";
                }
            }
            return text;
        }

        private string ByteToHex(byte[] input, string seperator = "")
        {
            return BitConverter.ToString(input).Replace("-", seperator);
        }
        private string ByteToHex(byte input, string seperator = "")
        {
            byte[] b_input = new byte[] { input };
            return BitConverter.ToString(b_input).Replace("-", seperator);
        }

        // Functions for MixColumns
        private byte MixMultiply(byte large, byte small) // small=0/1/2/3/4/5
        {
            int result = large;
            switch (small)
            {
                case 0:
                    result = 0;
                    break;
                case 1:
                    break;
                case 2:
                    result = Multiply2(large); // Multiply 2 function checks the result is within range each time. 
                    break;
                case 3:
                    result = Multiply2(large) ^ large;
                    break;
                case 4:
                    result = Multiply2(Multiply2(large));
                    break;
                case 5:
                    result = Multiply2(Multiply2(large)) ^ large;
                    break;
            }
            debug($"{ByteToHex(large)} * {small} -> {ByteToHex((byte)result)}");

            return (byte)result;
        }
        private byte Multiply2(byte large)
        {
            int result = large << 1;
            if (Convert.ToBoolean(large & 0x80))
            {
                result = (result ^ 0x1B);
            }
            return (byte)result;
        }
    }
}
