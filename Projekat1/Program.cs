namespace Projekat1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WebServer server = new WebServer();
            server.Pokreni();
            server.Zaustavi();
        }
    }
}
