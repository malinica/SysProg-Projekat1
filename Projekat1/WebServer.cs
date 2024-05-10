using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Projekat1
{
    internal class WebServer
    {
        private Kes kes;
        public HttpListener listener;
        private HttpClient klijent;
        private Thread osluskujNit;

        public WebServer(string url = "http://localhost:8080/")
        {
            listener = new HttpListener();
            klijent = new HttpClient();
            kes = new Kes();
            osluskujNit = new Thread(Osluskuj);
            listener.Prefixes
                    .Add(url);
        }

        public void Osluskuj()
        {

            try
            {
                while (listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem((ctx) => OdgovoriNaZahtev((HttpListenerContext)ctx), listener.GetContext());
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message + "Server je ugasen");
            }

        }

        public void Pokreni()
        {
            try
            {

                if (!listener.IsListening)
                {
                    listener.Start();
                    osluskujNit.Start();
                    Console.Write("Pokrenut server ");
                }
                else
                    Console.WriteLine("Server je vec pokrenut "); ;
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message); 
            }
        }

        public void Zaustavi()
        {
            try
            {

                Console.WriteLine("Pritisnite enter za kraj");
                while (Console.ReadKey().Key != ConsoleKey.Enter)
                    Thread.Sleep(1000);
                if (listener.IsListening)
                {
                    listener.Stop();
                    listener.Close();
                    osluskujNit.Join();
                    Console.WriteLine("Server je iskljucen ");
                }
                else
                    Console.WriteLine("Vec je iskljucen server ");
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message); 
            }
        }

        public void OdgovoriNaZahtev(HttpListenerContext zahtev)
        {
            try
            {
                if (zahtev.Request.HttpMethod != "GET")
                    Odgovori(400, null, zahtev);
                string Putanja = zahtev.Request.Url.ToString();
                string DozvoljenaPutanja = "http://localhost:8080/?";
                if (!Putanja.StartsWith(DozvoljenaPutanja))
                {
                    Odgovori(400, null, zahtev);
                    return;
                }
                //PARAMETRI SE IZDVAJAJU I STAVLJAJU U STRING PARAMETAR PRETRAGE STO PREDSTAAVLJA KLJUC 
                string parametarPretrage = izdvojiParametre(Putanja);
                if (parametarPretrage == null)
                    Odgovori(400, null, zahtev);
                //                Console.WriteLine("PARAMETAR JE " +parametarPretrage);



                string rezultatString;
                if (kes.ImaKljuc(parametarPretrage))
                {
                    Stavka stavkaIzKesa = new Stavka();
                    stavkaIzKesa = kes.CitajIzKesa(parametarPretrage);
                    rezultatString = "{\"total\":" + stavkaIzKesa.Ukupno + ",\"objectIDs\":[" + stavkaIzKesa.Podaci + "]}";
                }
                else
                {
                    var rezultat = klijent.GetAsync("https://collectionapi.metmuseum.org/public/collection/v1/search?" + parametarPretrage).Result;
                    if (!rezultat.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Nije uspela pretraga");
                        Odgovori(400, null, zahtev);
                        return;
                    }

                    rezultatString = rezultat.Content.ReadAsStringAsync().Result;
                    JToken rezultatJSON = JToken.Parse(rezultatString);
                    int total = rezultatJSON["total"].Value<int>();
                    string objectIDs = string.Join(",", rezultatJSON["objectIDs"].Select(id => (int)id));
                    kes.DodajUKes(parametarPretrage, total, objectIDs, 10);
                }
                Odgovori(200, rezultatString, zahtev);
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message); 
            }
        }

        public void Odgovori(int code, string rezultatString, HttpListenerContext zahtev)
        {
            try
            {

                if (code == 200)
                {
                    byte[] odgovorBytes = Encoding.UTF8.GetBytes(rezultatString);
                    zahtev.Response.StatusCode = 200;
                    zahtev.Response.ContentType = "text/plain";
                    zahtev.Response.ContentLength64 = odgovorBytes.Length;
                    zahtev.Response.OutputStream.Write(odgovorBytes, 0, odgovorBytes.Length);
                    zahtev.Response.OutputStream.Close();
                }
                else
                {
                    zahtev.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    zahtev.Response.OutputStream.Close();
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message); 
            }

        }

        public string izdvojiParametre(string Putanja)
        {
            try
            {
                string[] urlParts = Putanja.Split('?');
                if (urlParts.Length < 1)
                    return null;

                string queryParams = urlParts[1];
                string[] paramsList = queryParams.Split('&');
                int artistOrCulture = -1, isOnView = -1, isHighlight = -1, departmentId = -1;//-1 nije naveden,1 true,0 false
                string q = null, medium = null;
                foreach (var parameter in paramsList)
                {
                    var keyValue = parameter.Split('=');
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "artistOrCulture":
                                {
                                    if (keyValue[1] == "true")
                                        artistOrCulture = 1;
                                    else
                                        artistOrCulture = 0;

                                    break;
                                }
                            case "isOnView":
                                {
                                    if (keyValue[1] == "true")
                                        isOnView = 1;
                                    else
                                        isOnView = 0;

                                    break;
                                }
                            case "isHighlight":
                                {
                                    if (keyValue[1] == "true")
                                        isHighlight = 1;
                                    else
                                        isHighlight = 0;

                                    break;
                                }

                            case "q":
                                q = keyValue[1];
                                break;
                            case "medium":
                                medium = keyValue[1];
                                break;
                            case "departmentId":
                                int.TryParse(keyValue[1], out departmentId);
                                break;
                            default:
                                {
                                    return null;
                                }
                        }
                    }
                }
                string parametarPretrage = null;
                if (departmentId != -1)
                {
                    parametarPretrage += "departmentId=" + (departmentId.ToString()) + "&";
                }
                if (isOnView != -1)
                {
                    parametarPretrage += "isOnView=" + (isOnView == 1) + "&";
                }
                if (artistOrCulture != -1)
                {
                    parametarPretrage += "artistOrCulture=" + (artistOrCulture == 1) + "&";
                }
                if (isHighlight != -1)
                {
                    parametarPretrage += "isHighlight=" + (isHighlight == 1) + "&";
                }
                if (q != null)
                {
                    parametarPretrage += "q=" + q + "&";
                }
                if (medium != null)
                {
                    parametarPretrage += "medium=" + medium;
                }
                if (parametarPretrage.EndsWith("&"))
                {
                    parametarPretrage = parametarPretrage.Substring(0, parametarPretrage.Length - 1);
                }
                return parametarPretrage;
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message); 
            }
            return null;
        }
    }
}