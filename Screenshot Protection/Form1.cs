using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Screenshot_Protection
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, int dwAffinity);

        private const int WDA_NONE = 0x00;
        private const int WDA_MONITOR = 0x01;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Check if the pressed key is "Insert" (key code 45)
                if ((Keys)vkCode == Keys.Insert)
                {
                    // Get the reference to form1
                    Form1 form1 = (Form1)Application.OpenForms[0];

                    // Check the form1's current transparency
                    bool isTransparent = form1.Opacity == 0;

                    // Toggle the transparency
                    form1.Opacity = isTransparent ? 1.0 : 0;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public Form1()
        {
            InitializeComponent();
        }

        private void checkBoxScreenshotProtection_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxScreenshotProtection.Checked)
            {
                // Enable screenshot protection
                SetWindowDisplayAffinity(this.Handle, WDA_MONITOR);
            }
            else
            {
                // Disable screenshot protection
                SetWindowDisplayAffinity(this.Handle, WDA_NONE);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            checkBoxScreenshotProtection.CheckedChanged += checkBoxScreenshotProtection_CheckedChanged;
            _hookID = SetHook(_proc);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            Application.Exit();
        }
    }
}
