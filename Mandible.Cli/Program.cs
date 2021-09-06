using Mandible.Pack2;

namespace Mandible.Cli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: <packFilePath> <outputFolder>");
                return;
            }

            using Pack2Reader reader = new(args[0]);
        }
    }
}
