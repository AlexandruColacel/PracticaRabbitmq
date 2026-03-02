using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogViewer
{
    public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string LogLevel { get; set; } //TIPO DE ERROR EL TOPIC
    public string Category { get; set; }
    public string Message { get; set; }
    public string Exception { get; set; }
}
}