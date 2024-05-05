using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Projekat1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WebServer server = new WebServer();

            Console.WriteLine("WebServer pokrenut, osluskuje");
            while (true)
            {
                var context = server.listener.GetContext();
                server.OdgovoriNaZahtev(context);
            }
        }
    }
}
