using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogViewer
{
    // NO incluimos LogEntry aquí porque ya lo tienes en LogEntry.cs

    public class Suscriber
    {
        private readonly string _hostname = "localhost";
        private readonly string _exchangeName = "logs";
        private readonly string _userName = "guest";
        private readonly string _password = "guest";
        private readonly int _port = 5672;

        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _topic;

        public Suscriber(string topic)
        {
            _topic = topic;
            var factory = new ConnectionFactory() { HostName = _hostname, UserName = _userName, Password = _password, Port = _port };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                // Declaramos Exchange TOPIC (Requisito Ejercicio 1)
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
                _queueName = _channel.QueueDeclare().QueueName;
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _topic);
            }
            catch (Exception)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" [CRITICAL] NO SE PUDO CONECTAR A RABBITMQ. ¿ESTÁ EL DOCKER ENCENDIDO? ");
                Console.ResetColor();
                throw;
            }
        }

        public void StartConsuming()
        {
            // CABECERA "PRO"
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 VISOR DE LOGS CORPORATIVO v2.0                     ║");
            Console.WriteLine($"║            Estado: CONECTADO   |   Topic: {_topic.PadRight(18)}   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("");

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var jsonString = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    // Usa tu clase LogEntry externa
                    var log = JsonSerializer.Deserialize<LogEntry>(jsonString, options);

                    if (log != null)
                    {
                        PrintPrettyLog(log, routingKey);
                    }
                }
                catch
                {
                    // Fallback para mensajes que no sean JSON
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($" [RAW] {Encoding.UTF8.GetString(ea.Body.ToArray())}");
                    Console.ResetColor();
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            System.Threading.Thread.Sleep(-1);
        }

        private void PrintPrettyLog(LogEntry log, string routingKey)
        {
            // 1. Configurar colores e iconos según nivel
            var (color, icon, bgColor) = GetEstilo(log.LogLevel);
            
            // 2. Formatear datos
            var hora = log.Timestamp.ToLocalTime().ToString("HH:mm:ss");
            // Usamos "System" si Category viene nulo
            var categoriaCorta = RecortarTexto(log.Category ?? "System", 25);
            
            // 3. IMPRESIÓN DE LA LÍNEA PRINCIPAL
            Console.Write($" {hora} "); 

            // Nivel con color de fondo si es grave
            if (bgColor != ConsoleColor.Black) Console.BackgroundColor = bgColor;
            Console.ForegroundColor = color;
            // PadRight alinea el texto del nivel
            Console.Write($" {icon} {(log.LogLevel ?? "INFO").ToUpper().PadRight(5)} "); 
            Console.ResetColor();

            // Categoría en gris
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" [{categoriaCorta}] ".PadRight(28));
            
            // Mensaje: Detectamos SQL o Error
            if (!string.IsNullOrEmpty(log.Message))
            {
                if (log.Message.Contains("Executed DbCommand") || log.Message.Contains("SELECT ") || log.Message.Contains("INSERT "))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (bgColor != ConsoleColor.Black) // Si era error grave
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else 
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine(log.Message);
            }
            else
            {
                Console.WriteLine("");
            }
            Console.ResetColor();

            // 4. SI HAY EXCEPCIÓN (Diseño de caja de error)
            if (!string.IsNullOrEmpty(log.Exception))
            {
                Console.WriteLine("     ┌──────────────────────────────────────────────────────────────┐");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"     │ EXCEPCIÓN DETECTADA ({routingKey})");
                
                var lineas = log.Exception.Split(Environment.NewLine);
                foreach (var linea in lineas)
                {
                    if(!string.IsNullOrWhiteSpace(linea))
                        Console.WriteLine($"     │ {linea}");
                }
                
                Console.ResetColor();
                Console.WriteLine("     └──────────────────────────────────────────────────────────────┘");
            }
        }

        private (ConsoleColor fg, string icon, ConsoleColor bg) GetEstilo(string level)
        {
            return level?.ToLower() switch
            {
                "critical" or "fatal" => (ConsoleColor.White, "💀", ConsoleColor.DarkRed),
                "error" or "fail"     => (ConsoleColor.Red,   "❌", ConsoleColor.Black),
                "warning" or "warn"   => (ConsoleColor.Yellow,"⚠️ ", ConsoleColor.Black),
                "information" or "info"=> (ConsoleColor.Green, "ℹ️ ", ConsoleColor.Black),
                "debug"               => (ConsoleColor.Gray,  "🐛", ConsoleColor.Black),
                _                     => (ConsoleColor.White, "📝", ConsoleColor.Black)
            };
        }

        private string RecortarTexto(string texto, int max)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            // Limpiamos namespaces comunes para ahorrar espacio
            var limpio = texto.Replace("AppForSEII2526.API.", "").Replace("Microsoft.AspNetCore.", "MS.");
            if (limpio.Length > max) return limpio.Substring(0, max - 3) + "...";
            return limpio;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}