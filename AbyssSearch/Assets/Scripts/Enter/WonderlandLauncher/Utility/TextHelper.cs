// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/03/07/18:49
// Ver : 1.0.0
// Description : TextHelper.cs
// ChangeLog :
// **********************************************

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wonderland.Utility
{
    public class TextHelper
    {
        public static byte[] StringToByte(string inputString)
        {
            byte[] bKey = Encoding.UTF8.GetBytes("AesKey");
            byte[] bContent = Encoding.UTF8.GetBytes(inputString);

            Array.Reverse(bKey);

            var aes = new RijndaelManaged();
            ;
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.PKCS7;

            byte[] encrypt = null;

            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bKey),
                    CryptoStreamMode.Write))
                {
                    cStream.Write(bContent, 0, bContent.Length);
                    cStream.FlushFinalBlock();
                    encrypt = mStream.ToArray();
                }
            }

            return encrypt;
        }

        public static string ByteToString(byte[] inputBytes)
        {
            // 如果没有流加密方式，可以返回空
            var outputStream = new MemoryStream();

            var secret = "AesKey";
            byte[] bKey = Encoding.UTF8.GetBytes(secret);
            Array.Reverse(bKey);

            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.PKCS7;

            using (var memoryStream = new MemoryStream(inputBytes, 0, inputBytes.Length, false))
            {
                using (var cStream =
                    new CryptoStream(memoryStream, aes.CreateDecryptor(bKey, bKey), CryptoStreamMode.Read))
                {
                    cStream.CopyTo(outputStream);
                }
            }

            var content = outputStream.ToArray();

            return Encoding.UTF8.GetString(content);
        }
    }
}