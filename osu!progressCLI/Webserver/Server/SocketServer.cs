namespace osu_progressCLI.Webserver.Server
{
    public class Socketserver
    {
        private static Socketserver instance;

        private Socketserver() { }

        public Socketserver Instance()
        {
            if (instance == null)
            {
                instance = new Socketserver();
            }
            return instance;
        }
    }
}
