using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Parkano2018Bot.Models;

namespace Parkano2018Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9010/";

            using (var db= new ApplicationContext())
            {
                db.SaveChanges();
            }   

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("api is run on "+ baseAddress);
                // Create HttpCient and make a request to api/values 
                Console.ReadLine();
            }
        }
    }
}
