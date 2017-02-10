using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;


namespace OpenAgami.CardFoundation.MifareCardReaderWithSam.Tests
{
    [TestFixture]
    public class RSACypherer
    {
        [Test]
        public void Test()
        {
            //var keys = GenerateKeys(1024);

            //Console.WriteLine(DotNetUtilities.ToRSA(((RsaPrivateCrtKeyParameters)keys.Private)).ToXmlString(true));
            //Console.WriteLine();
            //Console.WriteLine(DotNetUtilities.ToRSA(((RsaPrivateCrtKeyParameters)keys.Private)).ToXmlString(false));

            var keys = GetRsaKeyPair();

            byte[] msg = Guid.NewGuid().ToByteArray();

            ISigner eng = new RsaDigestSigner(new Sha256Digest());

            eng.Init(true, keys.Private);

            eng.BlockUpdate(msg, 0, msg.Length);

            byte[] s = eng.GenerateSignature();

            eng = new RsaDigestSigner(new Sha256Digest());
            eng.Init(false, keys.Public);

            eng.BlockUpdate(msg, 0, msg.Length);

            Assert.IsTrue(eng.VerifySignature(s));
        }

        [Test]
        public void Test2()
        {
            byte[] msg = new byte[1224];

            new SecureRandom(new VmpcRandomGenerator()).NextBytes(msg);

            Assert.True(VerifySignature(msg, Sign(msg)));
        }

        [Test]
        public void Data_from_ms_crypto_service()
        {
            string document = @"{
    ""check_number"": 1,
    ""check_datetime"": ""2016-06-15T07:36:00"",
    ""shift_number"": 1,
    ""cashier"": ""Иванова А.И."",
    ""check_content"": [
    {
        ""good_code"": ""23"",
        ""price"": 5544,
        ""quantity"": 3.000,
        ""sum"": 1660,
        ""discount_sum"": 32
    },
    {
        ""good_code"": ""5223"",
        ""price"": 10000,
        ""quantity"": 7.000,
        ""sum"": 70000,
        ""discount_sum"": 0
    }
    ],
    ""payment_by_type"": [
    {
        ""payment_type"": 1,
        ""sum"": 1027
    },
    {
        ""payment_type"": 2,
        ""sum"": 500000
    }
    ]
}";

            var msg = Encoding.UTF8.GetBytes(document);

            var signature = ComputeSignature(msg);

            var bouncySig = Sign(msg);

            CollectionAssert.AreEqual(signature, bouncySig);

            var hash = new SHA256Managed().ComputeHash(msg);

            Assert.IsTrue(VerifySignature(msg, bouncySig));
            Assert.IsTrue(VerifySignature(msg, signature));
        }

        static string key = "<RSAKeyValue><Modulus>xYzorMTlvP5hu2biiWgljUf9VN9RRrH2qZH+dNSY6l+brrwOCBHmQAhsRrkn4NdVbEl5b9cfCbQng3pW92nIhI73NxTr8VCGc8iDDputb7psl+rErQpzVSHPXZkJyDgvkMsoWE0dHWHpQXgcBkzPn7BNlCHO0RWBP7ikyf/bCHs=</Modulus><Exponent>AQAB</Exponent><P>+6SYLqei2ztnfU7MJ4bNLnbLwPNCWUI1ArJZ6ImSqlsCiPFXX+Z6/SpMENZQevBHzf0nm0I6UuGGwCe/+iy+7w==</P><Q>yPiMugwg3nTGPz7kGEGdh4E99v8OsLKcluOuQATgUR3glyXgmp5NfP6i40/eLuixg2ZczWH0UOnylt7UKRCPNQ==</Q><DP>Tq/oOmaHAUCxGrjiE9YOIOJa0kn+zERsqRw2CwXBy+1LfKDi7oT2nmV0hatOXTL3cQ0hLmZmMHZ/GWUUndkSFw==</DP><DQ>lLYO9FCrNdF1LTGteSF8ntuM7at4xFm2s5TQyQCSuJOCMHZPyDohnr+R6uCbyVxYlqH1Q+ka75Dd+LP0jFp7jQ==</DQ><InverseQ>+g22Qw75/mC7WGKhGYYOX3ZljxvHP/rC8XBtzpimlqwmbj3S2oo65cxwfePbSJ07ZNLlrRLE41ulQbE+hDu8Gw==</InverseQ><D>U82SWNRQJyv8lqyvFh49q+DpqUrw5h4Rpt6dhL85PVegqe/xvd+l/uzzzc63CY0fmkfTAaxO6OVpS7+my98upbkkm4fxrvlJXb6t8MDn9whsT3Rkw42cHb04oTONxltynVqv8bfIEmCbuAVt0XO60Vn7Tf89hFrbg1Qe8vdrW8U=</D></RSAKeyValue>";
        static string pubKey = "<RSAKeyValue><Modulus>xYzorMTlvP5hu2biiWgljUf9VN9RRrH2qZH+dNSY6l+brrwOCBHmQAhsRrkn4NdVbEl5b9cfCbQng3pW92nIhI73NxTr8VCGc8iDDputb7psl+rErQpzVSHPXZkJyDgvkMsoWE0dHWHpQXgcBkzPn7BNlCHO0RWBP7ikyf/bCHs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private static byte[] ComputeSignature(byte[] msg)
        {
            var hash = new SHA256Managed().ComputeHash(msg);

            return OpenAgami.Foundation.Cryptography.RSA.SignSHA256Hash(key, hash);
        }

        public static bool VerifySignature(byte[] data, byte[] modulus, byte[] exponent, byte[] signature)
        {
            //RSAParameters p = new RSAParameters();
            //var publicKey = DotNetUtilities.GetRsaPublicKey(p);

            //var cs = new RSACryptoServiceProvider(1024);
            //var publicKey = DotNetUtilities.GetRsaPublicKey(cs);

            var publicKey = new RsaKeyParameters(false, new BigInteger(1, modulus), new BigInteger(1, exponent));

            ISigner eng = new RsaDigestSigner(new Sha256Digest()); //new PssSigner(new RsaEngine(), digest);


            eng.Init(false, publicKey);
            eng.BlockUpdate(data, 0, data.Length);

            return eng.VerifySignature(signature);
        }

        public static bool VerifySignature(byte[] data, byte[] signature)
        {
            //RSAParameters p = new RSAParameters();
            //var publicKey = DotNetUtilities.GetRsaPublicKey(p);

            //var cs = new RSACryptoServiceProvider(1024);
            //var publicKey = DotNetUtilities.GetRsaPublicKey(cs);

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pubKey);
            var parameters = rsa.ExportParameters(false);

            var publicKey = DotNetUtilities.GetRsaPublicKey(parameters);

            //ISigner eng = new RsaDigestSigner(new Sha256Digest()); //new PssSigner(new RsaEngine(), digest);
            RsaDigestSigner eng = new RsaDigestSigner(new Sha256Digest()); //new PssSigner(new RsaEngine(), digest);

           
            eng.Init(false, publicKey);
            eng.BlockUpdate(data, 0, data.Length);

            return eng.VerifySignature(signature);
        }

        public static byte[] Sign(byte[] data)
        {
            //RSAParameters p = new RSAParameters();
            //var publicKey = DotNetUtilities.GetRsaPublicKey(p);

            //var cs = new RSACryptoServiceProvider(1024);
            //var publicKey = DotNetUtilities.GetRsaPublicKey(cs);

            var publicKey = GetRsaKeyPair();

            ISigner eng = new RsaDigestSigner(new Sha256Digest()); //new PssSigner(new RsaEngine(), digest);


            eng.Init(true, publicKey.Private);
            eng.BlockUpdate(data, 0, data.Length);

            return eng.GenerateSignature();
        }

        private static AsymmetricCipherKeyPair GetRsaKeyPair()
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            var parameters = rsa.ExportParameters(true);

            var publicKey = DotNetUtilities.GetRsaKeyPair(parameters);
            return publicKey;
        }

        //public static bool VerifySignature(byte[] data, BigInteger modulus, BigInteger exponent, byte[] signature, IDigest digest)
        //{
        //    var key = new RsaKeyParameters(false, modulus, exponent);

        //    PssSigner eng = new PssSigner(new RsaEngine(), digest);

        //    eng.Init(false, key);
        //    eng.BlockUpdate(data, 0, data.Length);

        //    return eng.VerifySignature(signature);
        //}

        public AsymmetricCipherKeyPair GenerateKeys(int keySizeInBits)
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(),
                                               keySizeInBits));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();

            return keys;
        }

    }
}