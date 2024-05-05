using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projekat1
{
    internal class WebServer
    {
        private Kes kes;
        public HttpListener listener;
        private HttpClient klijent;

        public WebServer(string url = "http://localhost:8080/")
        {
            listener = new HttpListener();
            klijent = new HttpClient();
            kes = new Kes();
            listener.Prefixes
                    .Add(url);
            listener.Start();
        }

        public void OdgovoriNaZahtev(HttpListenerContext zahtev)
        {
            try
            {
                if (zahtev.Request.HttpMethod != "GET")
                    throw new Exception("Metoda mora biti GET!");
                string Putanja = zahtev.Request.Url.ToString();
                string DozvoljenaPutanja = "http://localhost:8080/?q=";
                Console.WriteLine(Putanja);
                Console.WriteLine(DozvoljenaPutanja);
                string url;
                if (Putanja.StartsWith(DozvoljenaPutanja))
                {
                    url = "https://collectionapi.metmuseum.org/public/collection/v1/search?q=" + Putanja.Substring(DozvoljenaPutanja.Length);
                    Console.WriteLine(url);
                }
                else
                    throw new Exception("Lose navedena putanja");

                var rezultat = klijent.GetAsync(url).Result;
                if (!rezultat.IsSuccessStatusCode)
                {
                    throw new Exception("Nije uspela pretraga");
                }
                byte[] podaci = rezultat.Content.ReadAsByteArrayAsync().Result;
                kes.DodajUKes(zahtev.Request.RawUrl.Substring(4), podaci, 10);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
