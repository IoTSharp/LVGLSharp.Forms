using LVGLSharp.Runtime.Remote;
using LVGLSharp.Runtime.Remote.Rdp;
using System;

namespace RdpDemo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LVGLSharp RDP Demo 启动中...");
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
        Console.WriteLine($"RDP 服务已监听 {options.Host}:{options.Port}，用户名：{(string.IsNullOrEmpty(options.Username) ? "<无>" : options.Username)}，请用 RDP 客户端连接测试。");
        Console.WriteLine("支持参数：--host=IP --port=端口 --username=用户名");
        Console.WriteLine("按 Ctrl+C 退出。");
        while (true) { Thread.Sleep(1000); }
    }
}
