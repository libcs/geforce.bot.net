using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GeForce
{
    public enum InteropMouseButton : byte
    {
        Left,
        Middle,
        Right
    }

    //https://www.ownedcore.com/forums/diablo-3/diablo-3-bots-programs/353201-source-how-bot-c.html
    //https://social.msdn.microsoft.com/Forums/vstudio/en-US/960411ab-d7c1-4f8f-99ac-dc273f50ace2/emulate-mouse-clicks-and-key-typing?forum=csharpgeneral
    public static class Interop
    {
        #region Interop

        // MSG PUMP

        [Serializable]
        public struct MSG
        {
            public IntPtr hwnd;
            public IntPtr lParam;
            public int message;
            public int pt_x;
            public int pt_y;
            public int time;
            public IntPtr wParam;
        }

        [DllImport("user32.dll")]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        // CURSOR

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
            public static implicit operator Point(POINT point) => new Point(point.X, point.Y);
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        // MOUSE HOOK

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT P;
            public int Wnd;
            public int HitTestCode;
            public int ExtraInfo;
        }

        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
        const int WH_MOUSE = 7;

        //This is the Import for the SetWindowsHookEx function.
        //Use this function to install a thread-specific hook.
        [DllImport("user32.dll")] //, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //This is the Import for the UnhookWindowsHookEx function.
        //Call this function to uninstall the hook.
        [DllImport("user32.dll")] //, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool UnhookWindowsHookEx(int idHook);

        //This is the Import for the CallNextHookEx function.
        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll")] //, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        // MOUSE

        [Flags]
        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        //static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        // KEYBOARD

        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const int KEYEVENTF_KEYUP = 0x2;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // GDI

        [DllImport("user32.dll")]
        static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

        [DllImport("gdi32.dll")]
        static extern int BitBlt
        (
            IntPtr hdcDest,     // handle to destination DC (device context)
            int nXDest,         // x-coord of destination upper-left corner
            int nYDest,         // y-coord of destination upper-left corner
            int nWidth,         // width of destination rectangle
            int nHeight,        // height of destination rectangle
            IntPtr hdcSrc,      // handle to source DC
            int nXSrc,          // x-coordinate of source upper-left corner
            int nYSrc,          // y-coordinate of source upper-left corner
            int dwRop           // raster operation code
        );

        // HANDLE

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        static readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());

        // MSG PUMP

        static int MsgPumpThreadId;
        public static void MsgPump() => Task.Run(() =>
        {
            MsgPumpThreadId = Thread.CurrentThread.ManagedThreadId;
            while (!GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        });

        // HANDLE

        public static IntPtr GetHandleByWindow(string windowName, string className = null) => FindWindow(className, windowName);

        public static IntPtr GetHandleByProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 1)
                throw new Exception($"Too many Processes named {processName}!");
            if (processes.Length == 0)
                throw new Exception($"{processName} not found!");
            return processes[0].MainWindowHandle;
        }

        public static void BringToFront(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return;
            SetForegroundWindow(handle);
        }

        // CURSOR

        public static Point CursorPosition
        {
            get { GetCursorPos(out var lpPoint); return lpPoint; }
            set => SetCursorPos(value.X, value.Y);
        }

        // MOUSEHOOK

        static int hHook = 0;

        public static bool MouseHook()
        {
            if (hHook != 0)
                throw new InvalidOperationException("Already hooked");
            var proc = new HookProc(MouseHookProc);
            hHook = SetWindowsHookEx(WH_MOUSE, proc, IntPtr.Zero, MsgPumpThreadId); //AppDomain.GetCurrentThreadId()
            return hHook != 0;
        }

        public static bool MouseUnhook()
        {
            if (!UnhookWindowsHookEx(hHook))
                return false;
            hHook = 0;
            return true;
        }

        static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            if (nCode < 0)
                return CallNextHookEx(hHook, nCode, wParam, lParam);
            Console.Write($"x = {MyMouseHookStruct.P.X.ToString("d")}  y = {MyMouseHookStruct.P.Y.ToString("d")}");
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        // MOUSE

        public static void ClickMouseButton(IntPtr handle, InteropMouseButton button, Point point, bool pause = false) =>
            ClickMouseButton(button, ConvertToScreenPixel(handle, point), pause);
        public static void ClickMouseButton(InteropMouseButton button, Point point, bool pause = false)
        {
            uint down, up;
            switch (button)
            {
                case InteropMouseButton.Left:
                    down = (uint)MouseEventFlags.LEFTDOWN;
                    up = (uint)MouseEventFlags.LEFTUP;
                    break;
                case InteropMouseButton.Middle:
                    down = (uint)MouseEventFlags.MIDDLEDOWN;
                    up = (uint)MouseEventFlags.MIDDLEUP;
                    break;
                case InteropMouseButton.Right:
                    down = (uint)MouseEventFlags.RIGHTDOWN;
                    up = (uint)MouseEventFlags.RIGHTUP;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(button));
            }
            var tmp = CursorPosition;
            mouse_event(up, 0, 0, 0, 0);
            CursorPosition = point;
            if (pause) Thread.Sleep(Rnd.Next(45, 50));
            mouse_event(down, 0, 0, 0, 0);
            mouse_event(up, 0, 0, 0, 0);
            CursorPosition = tmp;
        }

        // KEYBOARD

        public static void PressKey(byte key, bool pause = false)
        {
            keybd_event(key, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            if (pause) Thread.Sleep(Rnd.Next(5, 10));
            keybd_event(key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
        }

        // GDI

        public static Point ConvertToScreenPixel(IntPtr handle, Point point)
        {
            GetWindowRect(handle, out var rect);
            return new Point
            {
                X = rect.Location.X + point.X,
                Y = rect.Location.Y + point.Y
            };
        }

        public static Image CopyScreen(Point point, int width, int height, Image toImage)
        {
            using (var gdest = Graphics.FromImage(toImage))
            using (var gsrc = Graphics.FromHwnd(IntPtr.Zero))
            {
                var hSrcDC = gsrc.GetHdc();
                var hDC = gdest.GetHdc();
                var retval = BitBlt(hDC, 0, 0, width, height, hSrcDC, point.X, point.Y, (int)CopyPixelOperation.SourceCopy);
                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();
            }
            return toImage;
        }

        static Bitmap ScreenPixel = new Bitmap(1, 1);

        public static Color GetScreenPixel(IntPtr handle, Point point) =>
            GetScreenPixel(ConvertToScreenPixel(handle, point));
        public static Color GetScreenPixel(Point point) =>
            ((Bitmap)CopyScreen(point, 1, 1, ScreenPixel)).GetPixel(0, 0);

        //static bool IsInGame()
        //{
        //    var pixel = ConvertToScreenPixel(new Point(125, 598));
        //    var c = GetColorAt(pixel);
        //    return c.Name == "ff5d574c";
        //}
    }
}
