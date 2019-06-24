using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace LSOmni.Common.Util
{
    //class to decrypte passwords in config file
    public static class DecryptConfigValue
    {
        //strings are encrypted with LSOmni.PasswordGenerator.exe 
        //const string password = "lsrinvor";
        //a bit more difficult to find 
        private static string theWord = new string(new char[] { 'l', 's', 'r', 'i', 'n', 'v', 'o', 'r' });

        public static string DecryptString(string cipherText)
        {
            //sometimes I label a password string with :encr: so I know it is an encrypted pwd
            if (cipherText.EndsWith(":encr:"))
            {
                cipherText = cipherText.Replace(":encr:", "");
            }

            SymmetricAlgorithm algorithm = getAlgorithm();
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.Close();
            return Encoding.Unicode.GetString(ms.ToArray());
        }

        public static string EncryptString(string clearText)
        {
            SymmetricAlgorithm algorithm = getAlgorithm();
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            return Convert.ToBase64String(ms.ToArray()) + ":encr:";
        }

        public static bool IsEncryptedPwd(string encryptedPwd)
        {
            //sometimes I label a password string with :encr: so I know it is an encrypted pwd
            if (encryptedPwd.EndsWith(":encr:"))
                return true;
            else
                return false;
        }

        private static SymmetricAlgorithm getAlgorithm(string password)
        {
            SymmetricAlgorithm algorithm = Rijndael.Create();
            //wince does not support Rfc2898DeriveBytes
            //Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(
            //    password, new byte[] {
            //0x53,0x6f,0x64,0x69,0x75,0x6d,0x20,             // salty goodness
            //0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65});
            //algorithm.Padding = PaddingMode.ISO10126;
            //algorithm.Key = rdb.GetBytes(32);
            //algorithm.IV = rdb.GetBytes(16);

            //works on wince - using our own bytes 
            byte[] byte16 = new byte[] {
            (byte)password[0], (byte)password[1],0x64,0x69,0x75,0x6d,0x20,
            0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x44,0x34
            };
            //
            byte[] byte32 = new byte[] {
            (byte)password[0], (byte)password[1],0x64,0x69,0x75,0x6d,0x20,0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65,
            0x53,0x66,0x62,0x69,0x45,0x66,0x78,0x47,0x78,0x6c,0x6f,0x73,0x64,0x67,0x63,0x62,0x61
            };

            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Key = byte32;
            algorithm.IV = byte16;

            return algorithm;
        }

        private static SymmetricAlgorithm getAlgorithm()
        {
            SymmetricAlgorithm algorithm = Rijndael.Create();
            //wince does not support Rfc2898DeriveBytes
            //Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(
            //    password, new byte[] {
            //0x53,0x6f,0x64,0x69,0x75,0x6d,0x20,             // salty goodness
            //0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65});
            //algorithm.Padding = PaddingMode.ISO10126;
            //algorithm.Key = rdb.GetBytes(32);
            //algorithm.IV = rdb.GetBytes(16);

            //works on wince - using our own bytes 
            byte[] byte16 = new byte[] {
            (byte)theWord[0], (byte)theWord[1],0x64,0x69,0x75,0x6d,0x20,
            0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x44,0x34
            };
            //
            byte[] byte32 = new byte[] {
            (byte)theWord[0], (byte)theWord[1],0x64,0x69,0x75,0x6d,0x20,0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65,
            0x53,0x66,0x62,0x69,0x45,0x66,0x78,0x47,0x78,0x6c,0x6f,0x73,0x64,0x67,0x63,0x62,0x61
            };

            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Key = byte32;
            algorithm.IV = byte16;

            return algorithm;
        }
    }

    public static class Security
    {
        //Get username and password from basic authentication request header
        public static bool BasicAuthHeader(string basicAuthHeader, out string username, out string password)
        {
            username = "";
            password = "";

            if (String.IsNullOrWhiteSpace(basicAuthHeader))
            {
                // No credentials; anonymous request, return false sends unauth to client
                return false;
            }
            else
            {
                basicAuthHeader = basicAuthHeader.Trim();
                if (basicAuthHeader.IndexOf("Basic", 0) != 0)
                {
                    // header is not correct.
                    return false; //sends unauth to client
                }
                string[] decoded = DecodeFrom64(basicAuthHeader.Replace("Basic ", String.Empty)).Split(':');
                //username:pwd  has the semicolumn, otherwise it is the security token
                if (decoded.Length != 2)
                {
                    return false; //sends unauth to client
                }
                username = decoded[0];
                password = decoded[1];
            }
            return true;
        }

        #region securityToken

        /// <summary>
        /// Create a new securitytoken
        /// </summary>
        /// <returns>A securityToken.</returns>
        public static string CreateSecurityToken()
        {
            //TODO - maybe encrypt the guid, add date to it? 
            //string returnValue = System.Guid.NewGuid().ToString().Replace("-", "").ToUpper();

            string returnValue = string.Format("{0}|{1}", Guid.NewGuid().ToString().Replace("-", "").ToUpper(), DateTime.Now.ToString("yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

            //ex A2FF4BFC152F4C41A680D94214764D11|2012-09-20 13:52:20
            //returnValue = ComputeHash(returnValue);
            //IsValidSecurityToken(returnValue);
            return returnValue;
        }

        /// <summary>
        /// Validate the securitytoken
        /// </summary>
        /// <returns>n</returns>
        public static bool IsValidSecurityToken(string securityToken)
        {
            if (string.IsNullOrWhiteSpace(securityToken))
                return false;

            if (securityToken.Length < 32)
                return false;

            return true;
        }

        #endregion securityToken

        /// <summary>
        /// The method to Decode your Base64 strings.
        /// </summary>
        /// <param name="encodedData">The String containing the characters to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }

        private static string ComputeHash(string token)
        {
            //HASHing a GUID may make it less unique
            token = token.Replace("-", "");
            Byte[] inputBytes = Encoding.UTF8.GetBytes(token);

            //the final lenght of string using SHA1 is 40 chars (hashing a GUID).  SHA256 was 64 char
            //http://www.obviex.com/samples/Code.aspx?Source=HashCS&Title=Hashing%20Data&Lang=C%23
            SHA1 shaM = new SHA1Managed(); //MD5CryptoServiceProvider

            Byte[] hashedBytes = shaM.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        //used by NAV to encrypt pwd
        public static string NAVHash(string password)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            password += "rosasaltaftanvid"; //add a salt to it
            SHA512Managed sha512Hasher = new SHA512Managed();
            byte[] hashedDataBytes = sha512Hasher.ComputeHash(encoder.GetBytes(password));
            return Convert.ToBase64String(hashedDataBytes, 0, hashedDataBytes.Length).ToUpper();
        }

        /// <summary>
        /// Gets the hash of the data as used in LS Nav 6.x
        /// </summary>
        /// <param name="input">Data to hash</param>
        /// <returns>An uppercase hexadecimal string</returns>
        public static string NAVMd5Hash(string input)
        {
            //JIJ - taken from .Net CE 3.5 
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                // Loop through each byte of the hashed data and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("X2")); //X2 creates in uppercase
                }
                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }

    //very simple encryption class
    public static class Encryption
    {
        public static string Encrypt(string input, string key)
        {
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string input, string key)
        {
            byte[] inputArray = Convert.FromBase64String(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }

    //used for resetCode in the forget password
    public static class StringCipher
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            byte[] saltStringBytes = Generate256BitsOfRandomEntropy();
            byte[]  ivStringBytes = Generate256BitsOfRandomEntropy();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                byte[] keyBytes = password.GetBytes(Keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                byte[] cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            byte[] cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                byte[] keyBytes = password.GetBytes(Keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            byte[] randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
