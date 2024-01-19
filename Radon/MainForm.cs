using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        private NotifyIcon notifyIcon;
        private const string CustomFileExtension = ".rset";

        public MainForm()
        {
            InitializeComponent();
            clickPoints = new Point[0];
            currentPointIndex = 0;
            loopCount = 0;
            clickTimer = new System.Threading.Timer(PerformMouseClick, null, Timeout.Infinite, Timeout.Infinite);
            keyboardHook = new KeyboardHook();
            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupUI();
            RegisterHotKey();
            SetDefaultFileAssociation();
        }

        private void SetupUI()
        {
            notifyIcon = new NotifyIcon { Icon = SystemIcons.Information, Visible = false };
            keyboardHook.Start();
            Text = "Radon: An Open-Source Automated Mouse Clicker for Windows";
            Size = new Size(760, 510); 
            MinimumSize = new Size(760, 510); 
            StartPosition = FormStartPosition.CenterScreen;
            ShowIcon = false;

            txtPoints = CreateTextBox(0, 0, 380, 510);
            Controls.Add(txtPoints);
            txtPoints.ReadOnly = true;

            Label lblAppInfo = CreateLabel("Radon Auto Clicker\n\nTo add a point, press Ctrl+Alt+F10.\nAdjust settings on the right.\nClick 'Start' to begin auto-clicking.", 400, 20, 350, 150);
            Controls.Add(lblAppInfo);

            Button btnClearPoints = CreateButton("Clear Points", 400, 200, 100, 30);
            btnClearPoints.Click += btnClearPoints_Click;
            Controls.Add(btnClearPoints);

            Button btnStart = CreateButton("Start", 400, 250, 100, 30);
            btnStart.Click += btnStart_Click;
            Controls.Add(btnStart);

            Label lblInterval = CreateLabel("Interval (ms):", 400, 300, 80, 20);
            Controls.Add(lblInterval);

            txtInterval = CreateTextBox(490, 300, 80, 20);
            Controls.Add(txtInterval);

            Label lblLoopCount = CreateLabel("Loop Count:", 400, 330, 80, 20);
            Controls.Add(lblLoopCount);

            txtLoopCount = CreateTextBox(490, 330, 80, 20);
            Controls.Add(txtLoopCount);

            Button btnImportSettings = CreateButton("Import Settings", 400, 370, 150, 30);
            btnImportSettings.Click += btnImportSettings_Click;
            Controls.Add(btnImportSettings);

            Button btnExportSettings = CreateButton("Export Settings", 400, 420, 150, 30);
            btnExportSettings.Click += btnExportSettings_Click;
            Controls.Add(btnExportSettings);
        }

        private void btnClearPoints_Click(object sender, EventArgs e)
        {
            clickPoints = new Point[0];
            txtPoints.Text = string.Empty;
            loopCount = 0;
        }

        private void SetDefaultFileAssociation()
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(CustomFileExtension))
                {
                    key.SetValue("", "RadonSettingsFile");
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("RadonSettingsFile"))
                {
                    key.SetValue("", "Radon Settings File");
                    key.CreateSubKey("DefaultIcon").SetValue("", Application.ExecutablePath + ",0");
                    key.CreateSubKey("Shell\\Open\\Command").SetValue("", Application.ExecutablePath + " \"%1\"");
                }

                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
            
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);


        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new Button { Text = text, Size = new Size(width, height), Location = new Point(x, y), FlatStyle = FlatStyle.Flat };
        }

        private TextBox CreateTextBox(int x, int y, int width, int height)
        {
            var textBox = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Size = new Size(width, height), Location = new Point(x, y) };
            return textBox;
        }

        private Label CreateLabel(string text, int x, int y, int width, int height)
        {
            return new Label { Text = text, Size = new Size(width, height), Location = new Point(x, y) };
        }

        private void btnExportSettings_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Radon Settings Files (*.rset)|*.rset|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Radon Settings File"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ExportSettingsToFile(saveFileDialog.FileName);
                ShowNotification("Settings exported successfully!", "Success");
            }
        }

        private void ExportSettingsToFile(string filePath)
        {
            try
            {
                var pointsText = string.Join(Environment.NewLine, clickPoints.Select(p => $"{p.X},{p.Y}"));
                var lines = new[] { pointsText, txtInterval.Text, txtLoopCount.Text };
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
            
            }
        }

        private void ShowNotification(string message, string title)
        {
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000);
            notifyIcon.Visible = false;
        }

        private void btnImportSettings_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Radon Settings Files (*.rset)|*.rset|All Files (*.*)|*.*",
                Title = "Select Radon Settings File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (ImportSettingsFromFile(openFileDialog.FileName))
                    ShowNotification("Settings imported successfully!", "Success");
                else
                    ShowNotification("Failed to import settings. Please check the file format.", "Error");
            }
        }

        private void RegisterHotKey()
        {
            RegisterHotKey(Handle, 1, (uint)KeyModifiers.CtrlAlt, (uint)Keys.F10);
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
                btnAddPoint_Click(this, EventArgs.Empty);
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
                btnAddPoint_Click(sender, e);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, 1);
            keyboardHook.Stop();
        }

        private void ShowNotification(string message)
        {
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipTitle = "Notification";
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000);
            notifyIcon.Visible = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (clickPoints.Length == 0)
            {
                ShowNotification("Please add at least one point before starting the auto clicker.");
                return;
            }

            int interval;
            if (int.TryParse(txtInterval.Text, out interval) && interval > 0 &&
                int.TryParse(txtLoopCount.Text, out loopCount) && loopCount > 0)
            {
                clickTimer.Change(0, interval);
                currentPointIndex = 0;
                int totalClicks = clickPoints.Length * loopCount;
                ShowNotification($"Auto clicker started. Total clicks: {totalClicks}");
            }
            else
            {
                ShowNotification("Please enter valid values for interval and loop count.");
            }
        }

        private bool ImportSettingsFromFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);

                if (lines.Length >= 3)
                {
                    var pointLines = lines[0].Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    clickPoints = pointLines.Select(line =>
                    {
                        var coordinates = line.Split(',');
                        if (coordinates.Length == 2 && int.TryParse(coordinates[0], out int x) && int.TryParse(coordinates[1], out int y))
                            return new Point(x, y);
                        return Point.Empty;
                    }).ToArray();

                    var pointsBuilder = new StringBuilder();
                    for (int i = 0; i < clickPoints.Length; i++)
                        pointsBuilder.AppendLine($"Point {i + 1}: X={clickPoints[i].X}, Y={clickPoints[i].Y}");

                    txtPoints.Text = pointsBuilder.ToString().TrimEnd();
                    txtInterval.Text = lines[1];
                    txtLoopCount.Text = lines[2];

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
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
        private int currentId;
        private IntPtr hookId;
        private LowLevelKeyboardProc hookProc;

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        public KeyboardHook()
        {
            hookId = IntPtr.Zero;
        }

        public void Start()
        {
            hookProc = HookCallback;
            hookId = SetHook(hookProc);
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
