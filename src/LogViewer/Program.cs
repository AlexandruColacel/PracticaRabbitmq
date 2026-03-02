using System;

namespace LogViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            string topic;

            // Si le pasas argumentos (ej: dotnet run error) lo coge directo
            if (args.Length > 0)
            {
                topic = args[0];
            }
            else
            {
                // Si no, pregunta
                Console.WriteLine("Introduce el topic a escuchar:");
                Console.WriteLine("  error        -> Solo fallos");
                Console.WriteLine("  information  -> Solo info");
                Console.WriteLine("  #            -> TODO");
                Console.Write("> ");
                topic = Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(topic)) topic = "#";

            try
            {
                var suscriber = new Suscriber(topic);
                suscriber.StartConsuming();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar: " + ex.Message);
            }
        }
    }
}