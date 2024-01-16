using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Radon
{
    public partial class MainForm : Form
    {
        private Point[] clickPoints;
        private int currentPointIndex;
        private int loopCount;
        private System.Threading.Timer clickTimer;
        private TextBox txtPoints;
        private TextBox txtInterval;
        private TextBox txtLoopCount;
        private KeyboardHook keyboardHook;

        public MainForm()
        {
            InitializeComponent();

            clickPoints = new Point[0];
            currentPointIndex = 0;
            loopCount = 0;

            clickTimer = new System.Threading.Timer(PerformMouseClick, null, Timeout.Infinite, Timeout.Infinite);
            keyboardHook = new KeyboardHook();
            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupUI();
            RegisterHotKey();
        }

        private void SetupUI()
        {
            keyboardHook.Start();
            this.Text = "Radon";
            this.Size = new Size(450,320);
            this.StartPosition = FormStartPosition.CenterScreen;

            Button btnAddPoint = new Button();
            btnAddPoint.Text = "Add Point";
            btnAddPoint.Size = new Size(80, 30);
            btnAddPoint.Location = new Point(20, 20);
            btnAddPoint.Click += btnAddPoint_Click;
            this.Controls.Add(btnAddPoint);

            txtPoints = new TextBox();
            txtPoints.Multiline = true;
            txtPoints.ScrollBars = ScrollBars.Vertical;
            txtPoints.Size = new Size(200, 200);
            txtPoints.Location = new Point(20, 60);
            txtPoints.ReadOnly = true;
            this.Controls.Add(txtPoints);

            Button btnStart = new Button();
            btnStart.Text = "Start";
            btnStart.Size = new Size(80, 30);
            btnStart.Location = new Point(250, 20);
            btnStart.Click += btnStart_Click;
            this.Controls.Add(btnStart);

            Label lblInterval = new Label();
            lblInterval.Text = "Interval (ms):";
            lblInterval.Size = new Size(80, 20);
            lblInterval.Location = new Point(250, 70);
            this.Controls.Add(lblInterval);

            txtInterval = new TextBox();
            txtInterval.Size = new Size(80, 20);
            txtInterval.Location = new Point(330, 70);
            this.Controls.Add(txtInterval);

            Label lblLoopCount = new Label();
            lblLoopCount.Text = "Loop Count:";
            lblLoopCount.Size = new Size(80, 20);
            lblLoopCount.Location = new Point(250, 100);
            this.Controls.Add(lblLoopCount);

            txtLoopCount = new TextBox();
            txtLoopCount.Size = new Size(80, 20);
            txtLoopCount.Location = new Point(330, 100);
            this.Controls.Add(txtLoopCount);
        }

        private void RegisterHotKey()
        {
            RegisterHotKey(this.Handle, 1, (uint)KeyModifiers.CtrlAlt, (uint)Keys.F10);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                btnAddPoint_Click(this, EventArgs.Empty);
            }
        }

        private void btnAddPoint_Click(object sender, EventArgs e)
        {
            Array.Resize(ref clickPoints, clickPoints.Length + 1);
            clickPoints[clickPoints.Length - 1] = Cursor.Position;

            txtPoints.Text += $"Point {clickPoints.Length}: X={clickPoints[clickPoints.Length - 1].X}, Y={clickPoints[clickPoints.Length - 1].Y}{Environment.NewLine}";

            loopCount = 0;
        }

        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == KeyModifiers.CtrlAlt && e.Key == Keys.F10)
            {
                btnAddPoint_Click(sender, e);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            keyboardHook.Stop();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (clickPoints.Length == 0)
            {
                MessageBox.Show("Please add at least one point before starting the auto clicker.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int interval;
            if (int.TryParse(txtInterval.Text, out interval) && interval > 0 &&
                int.TryParse(txtLoopCount.Text, out loopCount) && loopCount > 0)
            {
                clickTimer.Change(0, interval);
                currentPointIndex = 0;
                int totalClicks = clickPoints.Length * loopCount;
                MessageBox.Show($"Auto clicker started. Total clicks: {totalClicks}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid values for interval and loop count.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformMouseClick(object state)
        {
            if (currentPointIndex < clickPoints.Length)
            {
                MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown | MouseOperations.MouseEventFlags.LeftUp, clickPoints[currentPointIndex].X, clickPoints[currentPointIndex].Y);

                currentPointIndex++;

                if (currentPointIndex == clickPoints.Length)
                {
                    currentPointIndex = 0;
                    loopCount--;

                    if (loopCount == 0)
                    {
                        clickTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        MessageBox.Show("Auto clicker completed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
    }

    public static class MouseOperations
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            XDown = 0x00000080,
            XUp = 0x00000100,
            Wheel = 0x00000800,
            Absolute = 0x00008000
        }

        public static void MouseEvent(MouseEventFlags value, int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event((int)value, x, y, 0, 0);
        }
    }

    public class KeyboardHook : IDisposable
    {
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        private int currentId;
        private IntPtr hookId;

        public KeyboardHook()
        {
            hookId = IntPtr.Zero;
        }

        public void Start()
        {
            hookId = SetHook(HookCallback);
        }

        public void Stop()
        {
            UnhookWindowsHookEx(hookId);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                KeyModifiers modifiers = GetActiveModifiers();
                KeyPressed?.Invoke(this, new KeyPressedEventArgs((Keys)vkCode, modifiers));
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private KeyModifiers GetActiveModifiers()
        {
            KeyModifiers modifiers = KeyModifiers.None;

            if (IsKeyPressed(Keys.ControlKey))
                modifiers |= KeyModifiers.Control;
            if (IsKeyPressed(Keys.Alt))
                modifiers |= KeyModifiers.Alt;
            if (IsKeyPressed(Keys.ShiftKey))
                modifiers |= KeyModifiers.Shift;

            return modifiers;
        }

        private bool IsKeyPressed(Keys key)
        {
            return (GetKeyState(key) & 0x8000) != 0;
        }

        [DllImport("user32.dll")]
        private static extern short GetKeyState(Keys key);

        public void Dispose()
        {
            Stop();
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public Keys Key { get; private set; }
        public KeyModifiers Modifier { get; private set; }

        public KeyPressedEventArgs(Keys key, KeyModifiers modifier)
        {
            Key = key;
            Modifier = modifier;
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        CtrlAlt = Control | Alt
    }
}
