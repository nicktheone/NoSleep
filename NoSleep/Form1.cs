using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;

namespace NoSleep
{
    public partial class Form1 : Form
    {
        private static NotifyIcon notifyIcon = new NotifyIcon();
        private static System.Timers.Timer timer = new System.Timers.Timer();
        private static bool isEnabled = Config.ReadConfig().enabled;

        public Form1()
        {
            InitializeComponent();

            //Check if config file exists else create app folder and config file
            try
            {
                Config.ReadConfig();
            }
            catch (Exception)
            {
                Config.FirstSetup();
            }

            //Start timer
            SetTimer();

            //Start the timer if enabled
            if (isEnabled)
            {
                timer.Start();
            }
        }

        #region Methods

        //Set the timer to send the keystroke
        private static void SetTimer()
        {
            //Attach the Elapsed event to the timer
            timer.Elapsed += TimerElapsed;

            //Set the timer at 59 seconds
            timer.Interval = 59 * 1000;
            timer.AutoReset = true;
        }

        //Create the ContextMenuStrip
        private static ContextMenuStrip AddContextMenuStrip()
        {
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Exit", null, Exit_ItemClicked);
            contextMenuStrip.Items.Add("Shutdown");
            (contextMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add("30 mins", null, Shutdown_ItemClicked);
            (contextMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add("60 mins", null, Shutdown_ItemClicked);
            (contextMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add("90 mins", null, Shutdown_ItemClicked);
            (contextMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add("120 mins", null, Shutdown_ItemClicked);
            //contextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;

            return contextMenuStrip;
        }

        //Check if app is enabled and set the correct icon
        private static Icon AssignIcon()
        {
            Icon icon;

            if (isEnabled)
            {
                icon = Properties.Resources.EnabledIcon;
            }
            else
            {
                icon = Properties.Resources.DisabledIcon;
            }

            return icon;
        }

        #endregion

        #region Events

        //Shutdown the computer
        private static void Shutdown_ItemClicked(object sender, EventArgs e)
        {
            string command = (sender as ToolStripMenuItem).Text;
            int trimmedCommand = Convert.ToInt16(command.Remove(command.Length - 5));
            string shutdownCommand = String.Format("/s /t {0}", trimmedCommand * 60);

            Process.Start(new ProcessStartInfo("shutdown", shutdownCommand)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        //Exit the application
        private static void Exit_ItemClicked(object sender, EventArgs e)
        {
            Application.Exit();
        } 

        //Simulate the keystroke when the timer ticks
        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //Send the keystoke (https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?redirectedfrom=MSDN&view=netframework-4.7.2)
            SendKeys.SendWait("{F15}");
        }

        //Handle the Resize event in order to minimize to tray (https://www.codeproject.com/Articles/27599/Minimize-window-to-system-tray)
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                ShowInTaskbar = false;

                notifyIcon.Visible = true;
                notifyIcon.Icon = AssignIcon();
                notifyIcon.ContextMenuStrip = AddContextMenuStrip();
                notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
                this.Hide();
            }
        }

        //Enable/disable app when double-clicking icon
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (isEnabled)
            {
                Config.WriteConfig(new Config { enabled = false });
                isEnabled = false;
                notifyIcon.Icon = AssignIcon();

                //Stop timer
                timer.Stop();
            }
            else
            {
                Config.WriteConfig(new Config { enabled = true });
                isEnabled = true;
                notifyIcon.Icon = AssignIcon();

                //Start timer
                timer.Start();
            }
        }

        //(Try to) Remove the NotifyIcon from the tray bar
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Icon.Dispose();
            notifyIcon.Dispose();
        }

        //Minimize the windows on load
        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        #endregion
    }
}