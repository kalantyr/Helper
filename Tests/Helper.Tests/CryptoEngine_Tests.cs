using Helper.Core.Jobs.Impl;
using NUnit.Framework;

namespace Helper.Tests
{
    public class CryptoEngine_Tests
    {
        [Test]
        public void Encrypt_Decrypt_Test()
        {
            // Encoding.UTF8.GetBytes(text);

            var data1 = new byte[] { 1, 2, 3, 4, 5 };

            var cryptoEngine = new CryptoEngine("qwerty");
            var encrypted = cryptoEngine.Encrypt(data1);
            var data2 = cryptoEngine.Decrypt(encrypted);
            CollectionAssert.AreEqual(data1, data2);
        }
    }
}
