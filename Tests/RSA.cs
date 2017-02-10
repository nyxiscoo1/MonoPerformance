using System.Security.Cryptography;

namespace OpenAgami.Foundation.Cryptography
{
    public static class RSA
    {
        public static string GenerateXmlKey(int size)
        {
            using (var rsa = new RSACryptoServiceProvider(size))
            {
                return rsa.ToXmlString(true);
            }
        }

        public static byte[] SignMD5Hash(string key, byte[] hash)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return rsa.SignHash(hash, CryptoConfig.MapNameToOID("MD5"));
            }
        }

        public static bool VerifyMD5Hash(string key, byte[] hash, byte[] signature)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("MD5"), signature);
            }
        }

        public static byte[] SignSHA1Hash(string key, byte[] hash)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
            }
        }

        public static bool VerifySHA1Hash(string key, byte[] hash, byte[] signature)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
            }
        }

        public static byte[] SignSHA256Hash(string key, byte[] hash)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            }
        }

        public static bool VerifySHA256Hash(string key, byte[] hash, byte[] signature)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signature);
            }
        }
    }
}