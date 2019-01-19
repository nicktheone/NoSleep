using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

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
            contextMenuStrip.Items.Add("Exit");
            contextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;

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

        //Exit the application
        private static void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
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