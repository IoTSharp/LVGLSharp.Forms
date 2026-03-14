using System;
using System.Collections.Generic;

namespace LVGLSharp.Forms
{
    public static class Application
    {
        private static bool _messageLoopRunning;
        private static ApplicationStyleMode _styleMode = ApplicationStyleMode.WinForms;
        private static bool _compatibleTextRenderingDefault;
        private static HighDpiMode _highDpiMode = HighDpiMode.SystemAware;
        private static readonly List<Form> _openForms = new List<Form>();
        private static Form? _mainForm;

        public static FormCollection OpenForms { get; } = new FormCollection(_openForms);

        public static void Run(Form main)
        {
            ArgumentNullException.ThrowIfNull(main);

            if (_messageLoopRunning)
            {
                throw new InvalidOperationException();
            }

            _messageLoopRunning = true;
            _mainForm = main;

            try
            {
                main.Show();
                main.RunMessageLoop();
            }
            finally
            {
                _mainForm = null;
                _messageLoopRunning = false;
            }
        }

        /// <summary>
        /// Uses the WinForms-compatible control styling mode for newly created controls.
        /// </summary>
        public static void EnableVisualStyles()
        {
            _styleMode = ApplicationStyleMode.WinForms;
        }

        /// <summary>
        /// Uses the native LVGL control styling mode for newly created controls.
        /// </summary>
        public static void EnableLvglStyles()
        {
            _styleMode = ApplicationStyleMode.Lvgl;
        }

        public static void SetCompatibleTextRenderingDefault(bool value)
        {
            _compatibleTextRenderingDefault = value;
        }

        public static void SetHighDpiMode(HighDpiMode highDpiMode)
        {
            _highDpiMode = highDpiMode;
        }

        /// <summary>
        /// Processes all Windows messages currently in the message queue.
        /// </summary>
        public static void DoEvents()
        {
            _mainForm?.ProcessEventsCore();
        }

        /// <summary>
        /// Informs all message pumps that they must terminate, and then closes all application windows.
        /// </summary>
        public static void Exit()
        {
            foreach (var form in _openForms.ToArray())
            {
                form.Close();
            }
        }

        internal static void RegisterOpenForm(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);

            if (!_openForms.Contains(form))
            {
                _openForms.Add(form);
            }
        }

        internal static void UnregisterOpenForm(Form form)
        {
            ArgumentNullException.ThrowIfNull(form);

            _openForms.Remove(form);
        }

        internal static bool VisualStylesEnabled => _styleMode == ApplicationStyleMode.WinForms;

        internal static ApplicationStyleSet CurrentStyleSet => ApplicationStyleCatalog.Get(_styleMode);

        internal static ApplicationStyleSet GetStyleSet(bool useVisualStyles)
        {
            return ApplicationStyleCatalog.Get(useVisualStyles ? ApplicationStyleMode.WinForms : ApplicationStyleMode.Lvgl);
        }
    }
}
