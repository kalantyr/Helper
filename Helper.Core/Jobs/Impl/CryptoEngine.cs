using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Helper.Jobs;

namespace Helper.Core.Jobs.Impl
{
    internal class CryptoEngine: ICryptoEngine
    {
        private static readonly byte[] IV = { 15, 24, 33, 42, 51, 60, 79, 88, 97, 06, 15, 24, 33, 42, 51, 60 };

        private readonly byte[] _key;

        public CryptoEngine(string password)
        {
            _key = new byte[32];
            using var hashProvider = new SHA512CryptoServiceProvider();
            Buffer.BlockCopy(hashProvider.ComputeHash(Encoding.UTF8.GetBytes(password)), 0, _key, 0, _key.Length);
        }

        public byte[] Encrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = IV;
            using var encryptor = aes.CreateEncryptor();
            using var resultStream = new MemoryStream();
            using var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write);
            using var plainStream = new MemoryStream(data);
            plainStream.CopyTo(aesStream);
            aesStream.FlushFinalBlock();
            return resultStream.ToArray();
        }

        public byte[] Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = IV;
            using var decryptor = aes.CreateDecryptor();
            using var resultStream = new MemoryStream();
            using var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write);
            using var plainStream = new MemoryStream(data);
            plainStream.CopyTo(aesStream);
            aesStream.FlushFinalBlock();
            return resultStream.ToArray();
        }
    }
}
