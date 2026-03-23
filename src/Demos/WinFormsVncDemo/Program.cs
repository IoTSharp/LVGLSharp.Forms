using LVGLSharp.Forms;
using LVGLSharp.Runtime.Remote;
using LVGLSharp.Runtime.Remote.Vnc;
using System;
using System.Threading;

namespace WinFormsVncDemo;

internal class Program
{
    static void Main(string[] args)
    {
        // 创建 WinForms 风格窗体
        var form = new Form { Text = "LVGLSharp VNC Demo", Width = 800, Height = 600 };
        var button = new Button { Text = "Hello VNC", Left = 100, Top = 100, Width = 200, Height = 60 };
        form.Controls.Add(button);
        form.Show();

        // 创建 RemoteFrameSource
        var frameSource = new WinFormsRemoteFrameSource(form);
        // 创建 VNC 传输
        var vncOptions = new VncSessionOptions { Host = "0.0.0.0", Port = 5900 };
        var vncTransport = new VncTransport(vncOptions);
        vncTransport.Start();

        // 绑定 RemoteRuntimeSession
        var session = new RemoteRuntimeSession(frameSource, vncTransport, vncOptions);

        Console.WriteLine($"VNC 服务已启动，监听 {vncOptions.Host}:{vncOptions.Port}，请用 VNC 客户端连接。");
        Console.WriteLine("按 Ctrl+C 退出。");

        // 简单帧推送循环
        while (true)
        {
            session.SendFrameAsync().Wait();
            Thread.Sleep(100);
        }
    }
}
