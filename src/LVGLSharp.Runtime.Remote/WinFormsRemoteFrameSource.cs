using LVGLSharp.Forms;
using LVGLSharp.Runtime.Remote;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Remote
{
    /// <summary>
    /// 采集 WinForms 窗体像素为 RemoteFrame。
    /// </summary>
    public sealed class WinFormsRemoteFrameSource : IRemoteFrameSource
    {
        private readonly Form _form;
        public WinFormsRemoteFrameSource(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        public RemoteFrame CaptureFrame()
        {
            // 采集窗体内容为 BGRA32
            var bmp = new Bitmap(_form.Width, _form.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(_form.Left, _form.Top, 0, 0, bmp.Size);
            }
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            var bytes = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bmp.UnlockBits(data);
            bmp.Dispose();
            return new RemoteFrame(_form.Width, _form.Height, bytes);
        }

        public bool TryHandleInput(RemoteInputEvent inputEvent)
        {
            // TODO: 输入事件转发到窗体
            return false;
        }
    }
}
