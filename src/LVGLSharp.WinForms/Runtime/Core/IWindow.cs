using LVGLSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVGLSharp
{
    public unsafe interface IWindow
    {
        /// <summary>
        /// Gets the root LVGL object for the current host window.
        /// </summary>
        lv_obj_t* Root { get; }

        /// <summary>
        /// Gets the active LVGL keyboard input group for the current host window.
        /// </summary>
        lv_group_t* KeyInputGroup { get; }

        /// <summary>
        /// Gets the callback used to position IME or text input focus for text areas.
        /// </summary>
        delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback { get; }

        public void Init();
        /// <summary>
        /// Processes one iteration of host and LVGL events.
        /// </summary>
        public void ProcessEvents();
        public void StartLoop(Action handle);
        /// <summary>
        /// Stops the host window and message loop.
        /// </summary>
        public void Stop();
    }
}
