using System;
using System.Drawing;

namespace GeForce
{
    public static class test
    {
        static int Mainx(string[] args)
        {
            Console.Write("BOT\n");

            var handle = Interop.GetHandleByWindow("Star Citizen");
            var x = Interop.CursorPosition;
            Interop.BringToFront(handle);
            Interop.ClickMouseButton(handle, InteropMouseButton.Left, new Point(545, 300));
            //var pixel = Interop.GetScreenPixel(handle, new Point(1, 1));

            return 0;
        }
    }
}