using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Windows
{
    public class Win32Api
    {
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WHEEL_DELTA = 120;


        public const int WS_OVERLAPPEDWINDOW = 0xcf0000;
        public const int WM_DESTROY = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowExW(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern int SetDIBitsToDevice(
            IntPtr hdc,
            int x, int y,
            uint dwWidth, uint dwHeight,
            int XSrc, int YSrc,
            uint uStartScan, uint cScanLines,
            IntPtr lpvBits,
            [In] ref BITMAPINFO lpbmi,
            uint fuColorUse
        );

        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW", CharSet = CharSet.Unicode)]
        public static extern int DefWindowProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int GCS_RESULTSTR = 0x0800;

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);

        [DllImport("imm32.dll")]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);


        [DllImport("imm32.dll")]
        public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COMPOSITIONFORM
        {
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public const int CFS_POINT = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int pt_x;
            public int pt_y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] bmiColors;
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;

        public static float GetDPI()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
            int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
            ReleaseDC(IntPtr.Zero, hdc);

            return dpiX;
        }
    }
}
