using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Class1 
    {
        private string _baseUrl = "https://posgear";
        //private string _baseUrl = "http://localhost:5000";

        [Test, Repeat(3)]
        public void MultiPost()
        {
            var sw = Stopwatch.StartNew();

            int requestsCont = 50;
            int threads = 30;

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

            var signature = ComputeSignature(document);

            Action action = () =>
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);

                    for (int i = 0; i < requestsCont; i++)
                    {
                        var payload = new StringContent(document, Encoding.UTF8, "application/json");
                        payload.Headers.Add("X-Signature", signature);

                        var response = client.PostAsync("/api/v1/process_document22", payload).Result;
                        
                        response.EnsureSuccessStatusCode();

                        var content = response.Content.ReadAsStringAsync().Result;
                        //Console.WriteLine(content);
                    }
                }
            };

            var tasks = Enumerable.Range(0, threads).Select(x => Task.Run(action)).ToArray();

            Task.WaitAll(tasks);


            sw.Stop();

            Console.WriteLine($"{sw.ElapsedMilliseconds} ms, {(requestsCont * threads) / (sw.ElapsedMilliseconds / 1000m):F0} req/s");
        }

        [Test]
        public async Task Get()
        {
            using (var client = new HttpClient())
            {
                var sw = Stopwatch.StartNew();

                client.BaseAddress = new Uri(_baseUrl);
                var response = await client.GetAsync("/api/v1");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                sw.Stop();

                Console.WriteLine(content);
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }

        [Test]
        public async Task Post()
        {
            using (var client = new HttpClient())
            {
                var sw = Stopwatch.StartNew();

                client.BaseAddress = new Uri(_baseUrl);

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
                var response = client.PostAsync("/api/v1/process_document", new StringContent(document, Encoding.UTF8, "application/json")).Result;
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                sw.Stop();

                Console.WriteLine(content);
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }

        [Test]
        public async Task Post2()
        {
            using (var client = new HttpClient())
            {
                var sw = Stopwatch.StartNew();

                client.BaseAddress = new Uri(_baseUrl);

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
                var signature = ComputeSignature(document);

                var payload = new StringContent(document, Encoding.UTF8, "application/json");
                payload.Headers.Add("X-Signature", signature);

                var response = client.PostAsync("/api/v1/process_document22", payload).Result;
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                sw.Stop();

                Console.WriteLine(content);
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }

        private static string ComputeSignature(string document)
        {
            string key = "<RSAKeyValue><Modulus>t8nC/Eth8UabQbXu8pdro3v7NqUanV8Y+g92YgT7z1xqkBLRHXZ1guml3PxrqjNX9AvOmu8R+qaKOyHfJW0PcRDLzCoIUcHNAwpDO/E5j6WAaLIv7gAjTtyr9kJB9rfJaparViJNZu3RSUYGTvVznOmXMf7LTOTMR6HP/5H1TP5n1g4+BbLmC9EhjUf2eNFqwZBqPtzybBb6jaHBRaJ0XdE3lh2OeE9/OF0BtLwiYPDKsVTxIekbNf7l/DREy+YbUOxQLceeHXrvbYLiGWecP0a7CqHGj9ZNY1oJThK3AwrSd4yHa9Wnx/GaZUNtWud1BaP9g3sVX+sRV9xtnI96dw==</Modulus><Exponent>AQAB</Exponent><P>3WSb72a1erb6jcLkyZA2Y21VNIipGz+ta1RP+iacs3xnktFsxgTYgqWyt6SWZ2rStp0u4vb/IAHyKhgJPNTUSi2u0G44MOsRxMC/FWTF8zdyrDF4BjPBM4j84nAmE/FQYv5F8ldDkakc96zEPiTk5Fka3MPeN8mMk6/OA59JdF0=</P><Q>1IRVid5SsDrOwJQAEKkdT436XEb0sVWe9AcU8JyaCEEMj0NPzownNbIrebPofMYdDHikopQpr2XqxZYDbb7AneoHkhEV26TfpPVbN4wBJFXih3lAP2n5hqhgqHGp5Wq2Lu7jUS376Ruw3bhwW+MiWpXv1xhMTZ8AtDfnZFFNvOM=</Q><DP>Fo5KiNCJCtCbpFfH4XVM5UJdXPXTbNBHBdlYMJ9AddTl5IJrt50ExgLFu4oMPMsYXryS61LI2WT5XCqIvmbcnhYbambgWLOKYuZUUYSr2kS67So5FUCunWaGhTdx2bRLQVqwm6kiXDPDnMRAViiCHXWqk/VsrXheVymhLqNK440=</DP><DQ>mowSWMzhfV+G8+2tjnAt7KjnpSvEzyHhEr4DsGdybQZBR/4/j4nFCfukOkFnlTXN8j/aGpF9Lx0C+uX5YFoUYcLL9qGOL8lbCu+TgnXCbtY2gybeXj+HQzI3+MeQMlLEYqU/ks3KIOAOY2+55ljrpszbOqVk+B3luSnekMm/qtk=</DQ><InverseQ>aP5e5F1j6s82Pm7dCpH3mRZWnfZIKqoNQIq2BO8vA9/WrdFI2C27uNhxCp2ZDMulRdBZcoeHcwJjnyDzg4I4gBZ2nSKkVdlN1REoTjLBBdlHi8XKiXzxvpItc2wjNC2AKHaJqj/dnh3bbTAQD1iUAxPmmLJYYkhfZ2i1IrTVxZE=</InverseQ><D>PUfM+Aq6kZSVWAetsL3EajKAxOuwQCDhVx+ovW4j+DQ8Y+WiTEyfShNV9qVD0PBltz3omch1GjpFhQn6OaRvraeIDH9HXttb3FOjr2zzYG4yrrYbPSRWoYj63ZWiIP2O7zdl0caGQHezfNcYa2N0NTG99DGc3/q6EnhlvjWQsSbiEjmxcPx8fmV1i4DoflMQ383nsixAFapgrROUAtCgMvhWn1kSeoojKd+e4eKZxa/SNYulsBJWNFkmo1CZH4YTqlPM+IwYeDUOnOUGNxGurRZ3qQdWs2N2ZQhnrvlh+zpzurD2hwAz6gQXP7mxxMR1xHtAD8XQ+w4OiJK6VWjoIQ==</D></RSAKeyValue>";

            var bytes = Encoding.UTF8.GetBytes(document);
            var hash = new SHA256Managed().ComputeHash(bytes);

            return Convert.ToBase64String(OpenAgami.Foundation.Cryptography.RSA.SignSHA256Hash(key, hash));
        }

        [Test]
        public void GenerateKeyPair()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.KeySize = 2048;
                Console.WriteLine("Private key:");
                Console.WriteLine(rsa.ToXmlString(true));

                Console.WriteLine("Public key:");
                Console.WriteLine(rsa.ToXmlString(false));
            }
        }

        [Test]
        public async Task Post3()
        {
            using (var client = new HttpClient())
            {
                var sw = Stopwatch.StartNew();

                client.BaseAddress = new Uri(_baseUrl);

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
                var response = client.PostAsync("/api/v1/process_document3", new StringContent(document, Encoding.UTF8, "application/json")).Result;
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                sw.Stop();

                Console.WriteLine(content);
                Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
            }
        }

        public Class1()
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}
