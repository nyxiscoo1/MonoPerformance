using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace Service.Middleware.RequestLogger
{
    public class RequestLoggerMiddleware
    {
        public static readonly string key =
            "<RSAKeyValue><Modulus>t8nC/Eth8UabQbXu8pdro3v7NqUanV8Y+g92YgT7z1xqkBLRHXZ1guml3PxrqjNX9AvOmu8R+qaKOyHfJW0PcRDLzCoIUcHNAwpDO/E5j6WAaLIv7gAjTtyr9kJB9rfJaparViJNZu3RSUYGTvVznOmXMf7LTOTMR6HP/5H1TP5n1g4+BbLmC9EhjUf2eNFqwZBqPtzybBb6jaHBRaJ0XdE3lh2OeE9/OF0BtLwiYPDKsVTxIekbNf7l/DREy+YbUOxQLceeHXrvbYLiGWecP0a7CqHGj9ZNY1oJThK3AwrSd4yHa9Wnx/GaZUNtWud1BaP9g3sVX+sRV9xtnI96dw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public static Lazy<ICipherParameters> publicKey = new Lazy<ICipherParameters>(CreateKey);

        private static ICipherParameters CreateKey()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(key);
                return DotNetUtilities.GetRsaPublicKey(rsa);
            }
        }

        private Dictionary<int, RSACryptoServiceProvider> _pool = new Dictionary<int, RSACryptoServiceProvider>();

        public RequestLoggerMiddleware()
        {
            for (int i = 0; i < 256; i++)
            {
                var cp = new RSACryptoServiceProvider();
                cp.FromXmlString(key);
                _pool[i] = cp;
            }
        }

        public async Task Invoke(IOwinContext context, Func<Task> next)
        {
            try
            {
                var signature = context.Request.Headers.Get("X-Signature");

                if (string.IsNullOrEmpty(signature))
                {
                    context.Response.ReasonPhrase = "Unauthorized";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

                byte[] signatureBytes = Convert.FromBase64String(signature);

                //var isSignatureValid = ReadBody(context, signatureBytes);
                var isSignatureValid = VerifySha256Signature(context, signatureBytes);

                if (!isSignatureValid)
                {
                    context.Response.ReasonPhrase = "Unauthorized";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }

            await next();
        }

        private int counter = 0;

        private static Lazy<RSACryptoServiceProvider> _rsa = new Lazy<RSACryptoServiceProvider>(CreateRSA);

        private static RSACryptoServiceProvider CreateRSA()
        {
            var cp = new RSACryptoServiceProvider();
            cp.FromXmlString(key);
            return cp;
        }

        private bool VerifySha256Signature(IOwinContext context, byte[] signatureBytes)
        {
            Interlocked.Increment(ref counter);

            var ms = new MemoryStream();

            var sha256 = new SHA256Managed();

            var cryptoStream = new CryptoStream(ms, sha256, CryptoStreamMode.Write);

            context.Request.Body.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();


            ms.Seek(0, SeekOrigin.Begin);
            context.Request.Body = ms;

            var ras = _rsa.Value;//_pool[(counter%_pool.Count)];

            return ras.VerifyHash(sha256.Hash, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
        }

        private static bool ReadBody(IOwinContext context, byte[] signature)
        {
            var ms = new MemoryStream();

            RsaDigestSigner eng = new RsaDigestSigner(new Sha256Digest()); //new PssSigner(new RsaEngine(), digest);

            eng.Init(false, publicKey.Value);

            byte[] buffer = new byte[81920];

            int count;
            while ((count = context.Request.Body.Read(buffer, 0, buffer.Length)) != 0)
            {
                ms.Write(buffer, 0, count);
                eng.BlockUpdate(buffer, 0, count);
            }


            var sha256 = new SHA256Managed();

            var cs = new CryptoStream(ms, sha256, CryptoStreamMode.Write);

            context.Request.Body.CopyTo(cs);

            cs.FlushFinalBlock();

            ms.Seek(0, SeekOrigin.Begin);
            context.Request.Body = ms;

            return eng.VerifySignature(signature);
        }
    }
}
