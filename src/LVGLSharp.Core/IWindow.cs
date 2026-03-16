using LVGLSharp.Interop;

namespace LVGLSharp
{
    public unsafe interface IWindow
    {
        lv_obj_t* Root { get; }

        lv_group_t* KeyInputGroup { get; }

        delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback { get; }

        void Init();

        void ProcessEvents();

        void StartLoop(Action handle);

        void Stop();

        void AttachTextInput(lv_obj_t* textArea);
    }
}
