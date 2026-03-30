using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace WinFormsRdpDemo;

internal static class RuntimeRegistration
{
    private const string DefaultHost = "0.0.0.0";
    private const int DefaultPort = 3389;

    [ModuleInitializer]
    internal static void Register()
    {
        ApplicationConfiguration.RegisterWindowsRuntimeInitializer(RegisterRuntime);
        ApplicationConfiguration.RegisterLinuxRuntimeInitializer(RegisterRuntime);
        ApplicationConfiguration.RegisterMacOsRuntimeInitializer(RegisterRuntime);
    }

    private static void RegisterRuntime()
    {
        Application.UseRuntime(static options => new RdpView(new RdpSessionOptions
        {
            Host = DefaultHost,
            Port = DefaultPort,
            Width = options.Width,
            Height = options.Height,
        }));
        Image.RegisterFactory(static _ => throw new NotSupportedException("WinFormsRdpDemo has not configured an image loading runtime yet."));
        WriteRdpConsoleBanner();
    }

    private static void WriteRdpConsoleBanner()
    {
        Console.WriteLine($"WinFormsRdpDemo RDP placeholder listening on {DefaultHost}:{DefaultPort}");
        Console.WriteLine("Available endpoints:");

        foreach (var endpoint in GetSuggestedEndpoints())
        {
            Console.WriteLine($"  {endpoint}");
        }

        Console.WriteLine("Note: TCP listening is ready, but the RDP handshake is still under implementation.");
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
