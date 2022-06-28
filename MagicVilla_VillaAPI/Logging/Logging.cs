namespace MagicVilla_VillaAPI.Logging
{
    public class Logging : ILogging
    {
        public void Log(string message, string type)
        {
            if (type == "error")
            {
                Console.WriteLine("ERORR - " + message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
