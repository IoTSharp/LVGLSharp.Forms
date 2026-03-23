using System;
using LVGLSharp;
using LVGLSharp.Interop;

namespace LVGLSharp.Runtime.Remote
{
    /// <summary>
    /// RDP 远程视图，负责与 RdpTransport 协作，作为远程桌面会话的 View 层。
    /// </summary>
    public unsafe class RdpView : IView
    {
        public Rdp.RdpTransport Transport { get; }
        public lv_obj_t* Root { get; private set; }
        public lv_group_t* KeyInputGroup { get; private set; }
        public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback { get; private set; }

        public RdpView(Rdp.RdpTransport transport)
        {
            Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public void Open() { /* TODO: 实现打开逻辑 */ }
        public void HandleEvents() { /* TODO: 实现事件处理 */ }
        public void RunLoop(Action iteration) { /* TODO: 实现主循环 */ }
        public void Close() { /* TODO: 实现关闭逻辑 */ }
        public void RegisterTextInput(lv_obj_t* textArea) { /* TODO: 注册文本输入 */ }
        public void Dispose() { /* TODO: 释放资源 */ }
    }
}