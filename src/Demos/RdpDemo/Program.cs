using System;

namespace RdpDemo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LVGLSharp RDP Demo is starting...");
        string host = "0.0.0.0";
        int port = 3389;
        string? username = null;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--host=")) host = arg[7..];
            else if (arg.StartsWith("--port=")) int.TryParse(arg[7..], out port);
            else if (arg.StartsWith("--username=")) username = arg[11..];
        }
        var options = new RdpSessionOptions
        {
            Host = host,
            Port = port,
            Username = username
        };
        var rdp = new RdpTransportSkeleton(options);
        Console.WriteLine($"RDP listener is running on {options.Host}:{options.Port}; username: {(string.IsNullOrEmpty(options.Username) ? "<none>" : options.Username)}.");
        Console.WriteLine("Supported arguments: --host=IP --port=PORT --username=NAME");
        Console.WriteLine("Press Ctrl+C to exit.");
        while (true) { Thread.Sleep(1000); }
    }
}
