using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using string
namespace Projekat1
    
{
    internal class WebServer
    {
        private Kes kes;
        private HttpListener listener;

       public  WebServer(string url = "http://localhost:8080/")
        {
             listener = new HttpListener();
            listener.Prefixes
                    .Add(url);
            listener.Start();
        }

        public void OdgovoriNaZahtev( HttpListenerContext zahtev )
        {
            try
            {
                if (zahtev.Request.HttpMethod != "GET")
                    throw new Exception("Metoda mora biti GET!");
                string DozvoljenaPutanja = "https://collectionapi.metmuseum.org/public/collection/v1/search?q=";
                if (!(zahtev.Request.Url.ToString()).Contains(DozvoljenaPutanja))
                    throw new Exception("Lose navedena putanja");

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
