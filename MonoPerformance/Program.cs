using System;
using System.IO;
using Microsoft.Owin.Hosting;
using MonoPerformance.Controllers;
using SQLite;

namespace MonoPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                Directory.CreateDirectory(dataDir);

                Storage.RecreateDbTables();
                SQLite.SQLite3.Config(SQLite3.ConfigOption.Serialized);

                var startOptions = new StartOptions();
                startOptions.Urls.Add("http://*:5000");

                using (WebApp.Start(startOptions, new ApiStartup().Configuration))
                {
                    Console.WriteLine("Running");
                    Console.ReadKey();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                Console.ReadKey();
            }
        }
    }
}
