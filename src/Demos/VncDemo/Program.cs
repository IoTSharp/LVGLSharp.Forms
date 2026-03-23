using LVGLSharp.Runtime.Remote;
using LVGLSharp.Runtime.Remote.Vnc;
using System;

namespace VncDemo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LVGLSharp VNC Demo 启动中...");
        string host = "0.0.0.0";
        int port = 5900;
        string? password = null;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--host=")) host = arg[7..];
            else if (arg.StartsWith("--port=")) int.TryParse(arg[7..], out port);
            else if (arg.StartsWith("--password=")) password = arg[11..];
        }
        var options = new VncSessionOptions
        {
            Host = host,
            Port = port,
            Password = password
        };
        var vnc = new VncTransportSkeleton(options);
        Console.WriteLine($"VNC 服务已监听 {options.Host}:{options.Port}，密码：{(string.IsNullOrEmpty(options.Password) ? "<无>" : options.Password)}，请用 VNC 客户端连接测试。");
        Console.WriteLine("支持参数：--host=IP --port=端口 --password=密码");
        Console.WriteLine("按 Ctrl+C 退出。");
        while (true) { Thread.Sleep(1000); }
    }
}
