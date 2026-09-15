using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using Windows.Win32.Foundation;

namespace RSoft.MacroPad.Infrastructure
{
    internal static class Extern
    {


        //[DllImport("user32.dll")]
        //public static extern ushort GetAsyncKeyState(Keys key);
        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern bool UnhookWindowsHookEx(IntPtr hook);
        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        //[DllImport("user32.dll")]
        //public static extern uint MapVirtualKeyW(uint uCode, [MarshalAs(UnmanagedType.U4)] MAPVK uMapType);
        //[DllImport("user32.dll")]
        //public static extern uint MapVirtualKeyExW(uint uCode, MAPVK uMapType, IntPtr hkl);
        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        //public static extern uint GetKeyNameTextW(uint lParam, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder str, int size);
        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr GetModuleHandle(string name);
        //[DllImport("user32.dll", CharSet = CharSet.Ansi)]
        //public static extern IntPtr LoadKeyboardLayoutW(/*[MarshalAs(UnmanagedType.LPStr)]*/ IntPtr klId, KLF flags);
        //[DllImport("user32.dll")]
        //public static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, KLF flags);
        //[DllImport("user32.dll")]
        //public static extern IntPtr GetKeyboardLayout(uint threadId = 0);


        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

        /// <summary>
        /// Pinta a barra de título com a cor do app (só funciona no Windows 11; no 10 a chamada é ignorada).
        /// </summary>
        public static void SetTitleBarColors(IntPtr windowHandle, Color background, Color text)
        {
            // Constantes DWMWA_CAPTION_COLOR e DWMWA_TEXT_COLOR da documentação do DwmSetWindowAttribute
            const int captionColorAttribute = 35;
            const int textColorAttribute = 36;

            var backgroundColorRef = ColorTranslator.ToWin32(background);
            var textColorRef = ColorTranslator.ToWin32(text);
            DwmSetWindowAttribute(windowHandle, captionColorAttribute, ref backgroundColorRef, sizeof(int));
            DwmSetWindowAttribute(windowHandle, textColorAttribute, ref textColorRef, sizeof(int));
        }

        public static PWSTR ToPWSTR(this string s)
        {
            unsafe
            {
                fixed (char* p = s)
                {
                    return new PWSTR(p);
                }
            }
        }
    }

    public enum MAPVK : uint
    {
        MAPVK_VK_TO_VSC = 0x00,
        MAPVK_VSC_TO_VK = 0x01,
        MAPVK_VK_TO_CHAR = 0x02,
        MAPVK_VSC_TO_VK_EX = 0x03,
        MAPVK_VK_TO_VSC_EX = 0x04
    }

    public enum KLF : uint
    {
        KLF_ACTIVATE = 0x01,
        KLF_SUBSTITUTE_OK = 0x02,
        KLF_REORDER = 0x08,
        KLF_REPLACELANG = 0x10,
        KLF_NOTELLSHELL = 0x80,
        KLF_SETFORPROCESS = 0x0100,
        KLF_SHIFTLOCK = 0x00010000,
        KLF_RESET = 0x40000000,

    }

    //System level functions to be used for hook and unhook keyboard input
    internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // Structure contain information about low-level keyboard input event
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyInfo
    {
        public Keys key;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr extra;

        public bool IsKeyRelease => (flags & 0x80) > 0;
        public bool IsKeyPress => !IsKeyRelease;

        public override string ToString()
        {
            var x = extra == IntPtr.Zero ? "" : "+";
            return $"{key}({(ushort)key}):{scanCode},{flags:X}({time}){x}";
        }
    }
}
