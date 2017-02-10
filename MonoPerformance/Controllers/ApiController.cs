using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using ServiceStack.Redis;
using SQLite;
using StackExchange.Redis;

namespace MonoPerformance.Controllers
{
    public class ApiController : System.Web.Http.ApiController
    {
        private static Lazy<ConnectionMultiplexer> Redis =
            new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("127.0.0.1:6379"));

        private ConnectionMultiplexer redis
        {
            get { return Redis.Value; }
        }

        private static Lazy<IRedisClientsManager> Redis2 =
            new Lazy<IRedisClientsManager>(() => new PooledRedisClientManager("127.0.0.1:6379"));

        private IRedisClientsManager redis2
        {
            get { return Redis2.Value; }
        }

        [HttpGet]
        [ResponseType(typeof(string))]
        public IHttpActionResult about()
        {
            var message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent("MonoPerformanceTest v1", Encoding.UTF8);

            return ResponseMessage(message);
        }

        [HttpPost]
        public ResponseModel process_document([FromBody] Check check)
        {
            if (check == null)
            {
                return new ResponseModel
                {
                    ErrorMessage = "Check is null"
                };
            }

            //var doc = new Document
            //{
            //    DateAndTime = DateTime.Now,
            //    Type = 42,
            //    Data = check.JsonSerialize(true)
            //};

            string id = Guid.NewGuid().ToString();

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", id + ".json");

            File.WriteAllText(filePath, check.JsonSerialize(), Encoding.UTF8);

            //using (var db = Storage.Open())
            //{
            //    int retryNumber = 0;
            //    while (true)
            //    {
            //        try
            //        {
            //            lock (syncObj)
            //                db.Insert(doc);
            //            break;
            //        }
            //        catch (SQLiteException exc)
            //        {
            //            if (exc.Result == SQLite3.Result.Busy || exc.Result == SQLite3.Result.Locked)
            //            {
            //                retryNumber++;

            //                Console.WriteLine($"Retry #{retryNumber}");
            //                continue;
            //            }

            //            Console.WriteLine(exc);

            //            throw;
            //        }
            //        catch (Exception exc)
            //        {
            //            Console.WriteLine(exc);

            //            throw;
            //        }
            //    }
            //}

            return new ResponseModel
            {
                Id = id
            };
        }

        [HttpPost]
        public async Task<ResponseModel> process_document2([FromBody] Check check)
        {
            if (check == null)
            {
                return new ResponseModel
                {
                    ErrorMessage = "Check is null"
                };
            }

            try
            {
                string id = Guid.NewGuid().ToString();

                await redis.GetDatabase().StringSetAsync(id, check.JsonSerialize());

                return new ResponseModel
                {
                    Id = id
                };
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }
        }

        [HttpPost]
        public async Task<ResponseModel> process_document22([FromBody] Check check)
        {
            if (check == null)
            {
                return new ResponseModel
                {
                    ErrorMessage = "Check is null"
                };
            }

            try
            {
                string id = Guid.NewGuid().ToString();

                await redis.GetDatabase().StringSetAsync(id, check.JsonSerialize()).ConfigureAwait(false);

                return new ResponseModel
                {
                    Id = id
                };
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }
        }

        [HttpPost]
        public async Task<ResponseModel> process_document23([FromBody] Check check)
        {
            if (check == null)
            {
                return new ResponseModel
                {
                    ErrorMessage = "Check is null"
                };
            }

            try
            {
                string id = Guid.NewGuid().ToString();

                redis.GetDatabase().StringSet(id, check.JsonSerialize());

                return new ResponseModel
                {
                    Id = id
                };
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }
        }

        [HttpPost]
        public async Task<ResponseModel> process_document3([FromBody] Check check)
        {
            if (check == null)
            {
                return new ResponseModel
                {
                    ErrorMessage = "Check is null"
                };
            }

            try
            {
                string id = Guid.NewGuid().ToString();
                using (var client = redis2.GetClient())
                {
                    var checks = client.As<Document>();

                    checks.Store(new Document
                    {
                        Id = checks.GetNextSequence(),
                        Data = check.JsonSerialize(),
                        DateAndTime = DateTime.Now,
                        Type = 42
                    });

                    return new ResponseModel
                    {
                        Id = id
                    };
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }
        }
    }

    public class ResponseModel
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ErrorMessage { get; set; }
    }

    public class Check
    {
        public uint CheckNumber { get; set; }
        public DateTime CheckDatetime { get; set; }
        public uint ShiftNumber { get; set; }
        public string Cashier { get; set; }
        public CheckPosition[] CheckContent { get; set; }
        public CheckPayment[] PaymentByType { get; set; }
    }

    public class CheckPosition
    {
        public string GoodCode { get; set; }
        public string Name { get; set; }
        public ulong Price { get; set; }
        public decimal Quantity { get; set; }
        public ulong Sum { get; set; }
        public long DiscountSum { get; set; }
    }

    public class CheckPayment
    {
        public uint PaymentType { get; set; }
        public ulong Sum { get; set; }
    }

    public class Storage
    {
        private static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage.db");

        public static SQLiteConnection Open()
        {
            return new SQLiteConnection(path);
        }

        public static void RecreateDbTables()
        {
            using (var Db = new SQLiteConnection(path))
                RecreateDbTables(Db);
        }

        private static void RecreateDbTables(SQLiteConnection connection)
        {
            connection.CreateTable<Document>();
        }
    }

    [Table("Documents"), Serializable]
    public class Document
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public DateTime DateAndTime { get; set; }

        public int Type { get; set; }

        public string Data { get; set; }
    }

    public static class JsonSerializationExtensions
    {
        public static string JsonSerialize<T>(this T o, bool format = false)
        {
            return JsonConvert.SerializeObject(o, format ? Formatting.Indented : Formatting.None);
        }

        public static T JsonDeserialize<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}
