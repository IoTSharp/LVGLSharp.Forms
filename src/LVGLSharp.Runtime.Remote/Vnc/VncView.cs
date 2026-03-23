using System;
using LVGLSharp;
using LVGLSharp.Interop;

namespace LVGLSharp.Runtime.Remote
{
    /// <summary>
    /// VNC 远程视图，负责与 VncTransport 协作，作为远程桌面会话的 View 层。
    /// </summary>
    public unsafe class VncView : IView
    {
        public Vnc.VncTransport Transport { get; }
        public lv_obj_t* Root { get; private set; }
        public lv_group_t* KeyInputGroup { get; private set; }
        public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback { get; private set; }

        private static lv_indev_t* _keyboardIndev;
        private static lv_indev_t* _pointerIndev;
        private static bool _indevRegistered;

        public VncView(Vnc.VncTransport transport)
        {
            Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public void Open()
        {
            if (_indevRegistered) return;
            // 创建 LVGL 输入设备
            _keyboardIndev = LVGLSharp.Interop.Lvgl.lv_indev_create();
            LVGLSharp.Interop.Lvgl.lv_indev_set_type(_keyboardIndev, lv_indev_type_t.LV_INDEV_TYPE_KEYPAD);
            _pointerIndev = LVGLSharp.Interop.Lvgl.lv_indev_create();
            LVGLSharp.Interop.Lvgl.lv_indev_set_type(_pointerIndev, lv_indev_type_t.LV_INDEV_TYPE_POINTER);
            _indevRegistered = true;
        }

        public static lv_indev_t* GetKeyboardIndev() => _keyboardIndev;
        public static lv_indev_t* GetPointerIndev() => _pointerIndev;

        public void HandleEvents() { /* TODO: 实现事件处理 */ }
        public void RunLoop(Action iteration) { /* TODO: 实现主循环 */ }
        public void Close() { /* TODO: 实现关闭逻辑 */ }
        public void RegisterTextInput(lv_obj_t* textArea) { /* TODO: 注册文本输入 */ }
        public void Dispose() { /* TODO: 释放资源 */ }
    }
}
