using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using YooAsset;

namespace Wonderland.Utility
{
    public class BundleUnpackServices : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            // 如果没有流加密方式，可以返回空
            var outputStream = new MemoryStream();

            var secret = "Password";
            byte[] bKey = Encoding.UTF8.GetBytes(secret);
            Array.Reverse(bKey);

            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.PKCS7;

            using (var fileStream = new FileStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var cStream =
                    new CryptoStream(fileStream, aes.CreateDecryptor(bKey, bKey), CryptoStreamMode.Read))
                {
                    cStream.CopyTo(outputStream);
                }
            }
            
            // BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = outputStream;
            decryptResult.Result = AssetBundle.LoadFromStream(outputStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return decryptResult;
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            // 如果没有流加密方式，可以返回空
            var outputStream = new MemoryStream();

            var secret = "Password";
            byte[] bKey = Encoding.UTF8.GetBytes(secret);
            Array.Reverse(bKey);

            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.PKCS7;

            using (var fileStream = new FileStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var cStream =
                    new CryptoStream(fileStream, aes.CreateDecryptor(bKey, bKey), CryptoStreamMode.Read))
                {
                    cStream.CopyTo(outputStream);
                }
            }
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = outputStream;
            decryptResult.CreateRequest = AssetBundle.LoadFromStreamAsync(outputStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return decryptResult;
        }

        /// <summary>
        /// 后备方式获取解密的资源包对象
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleFallback(DecryptFileInfo fileInfo)
        {
            return new DecryptResult();
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        private uint GetManagedReadBufferSize()
        {
            return 1024;
        }
    }
}