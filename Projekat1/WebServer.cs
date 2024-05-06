using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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

            while(true)
            {
                ThreadPool.QueueUserWorkItem((context)=> OdgovoriNaZahtev((HttpListenerContext)context), listener.GetContext());    
            }
        }

        public void OdgovoriNaZahtev(HttpListenerContext zahtev)
        {
            try
            {
                if (zahtev.Request.HttpMethod != "GET")
                    throw new Exception("Metoda mora biti GET!");
                string Putanja = zahtev.Request.Url.ToString();
                string DozvoljenaPutanja = "http://localhost:8080/?q=";
                //Console.WriteLine(Putanja);
                //Console.WriteLine(DozvoljenaPutanja);
                string url;
                string parametarPretrage = Putanja.Substring(DozvoljenaPutanja.Length);
                if (Putanja.StartsWith(DozvoljenaPutanja))
                {
                    url = "https://collectionapi.metmuseum.org/public/collection/v1/search?q=" + parametarPretrage;
                    //Console.WriteLine(url);
                }
                else
                {
                    Console.WriteLine("Nepravilna putanja: " + Putanja);
                    zahtev.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                string rezultatString;
                if (kes.ImaKljuc(parametarPretrage))
                {
                    Stavka stavkaIzKesa = new Stavka();
                    stavkaIzKesa = kes.CitajIzKesa(parametarPretrage);
                    rezultatString = "{\"total\":" + stavkaIzKesa.Ukupno + ",\"objectIDs\":[" + stavkaIzKesa.Podaci + "]}";
                }
                else
                {
                    var rezultat = klijent.GetAsync(url).Result;
                    if (!rezultat.IsSuccessStatusCode)
                        throw new Exception("Nije uspela pretraga");
                    rezultatString = rezultat.Content.ReadAsStringAsync().Result;
                    JToken rezultatJSON = JToken.Parse(rezultatString);
                    int total = rezultatJSON["total"].Value<int>();
                    string objectIDs = string.Join(",", rezultatJSON["objectIDs"].Select(id => (int)id));
                    kes.DodajUKes(zahtev.Request.RawUrl.Substring(4), total, objectIDs, 10);
                }
                byte[] odgovorBytes = Encoding.UTF8.GetBytes(rezultatString);
                zahtev.Response.StatusCode = 200;
                zahtev.Response.ContentType = "text/plain";
                zahtev.Response.ContentLength64 = odgovorBytes.Length;
                zahtev.Response.OutputStream.Write(odgovorBytes, 0, odgovorBytes.Length);
                zahtev.Response.OutputStream.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
