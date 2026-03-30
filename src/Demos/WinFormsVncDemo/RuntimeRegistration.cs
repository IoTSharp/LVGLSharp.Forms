using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace WinFormsVncDemo;

internal static class RuntimeRegistration
{
    private const string DefaultHost = "0.0.0.0";
    private const int DefaultPort = 5900;

    [ModuleInitializer]
    internal static void Register()
    {
        ApplicationConfiguration.RegisterWindowsRuntimeInitializer(RegisterRuntime);
        ApplicationConfiguration.RegisterLinuxRuntimeInitializer(RegisterRuntime);
        ApplicationConfiguration.RegisterMacOsRuntimeInitializer(RegisterRuntime);
    }

    private static void RegisterRuntime()
    {
        Application.UseRuntime(static options => new VncView(new VncSessionOptions
        {
            Host = DefaultHost,
            Port = DefaultPort,
            Width = options.Width,
            Height = options.Height,
        }));
        Image.RegisterFactory(static _ => throw new NotSupportedException("WinFormsVncDemo has not configured an image loading runtime yet."));
        WriteVncConsoleBanner();
    }

    private static void WriteVncConsoleBanner()
    {
        Console.WriteLine($"WinFormsVncDemo VNC listening on {DefaultHost}:{DefaultPort}");
        Console.WriteLine("Available endpoints:");

        foreach (var endpoint in GetSuggestedEndpoints())
        {
            Console.WriteLine($"  {endpoint}");
        }
    }

    private static IEnumerable<string> GetSuggestedEndpoints()
    {
        yield return $"127.0.0.1:{DefaultPort}";

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "127.0.0.1",
        };

        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            var properties = networkInterface.GetIPProperties();
            foreach (var unicastAddress in properties.UnicastAddresses)
            {
                var address = unicastAddress.Address;
                if (address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(address))
                {
                    continue;
                }

                var text = address.ToString();
                if (seen.Add(text))
                {
                    yield return $"{text}:{DefaultPort}";
                }
            }
        }
    }
}
