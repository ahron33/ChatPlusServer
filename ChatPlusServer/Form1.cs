using FluentFTP;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatPlusServer
{
    public partial class Form1 : Form
    {
        public string url = "";
        public string user = "";
        public string pass = "";
        public string path = "";
        public string urlhttp = "";



        public int loopchat = 0;
        public Form1()
        {
            InitializeComponent();
        }

        public static string GetMotherboardUUID()
        {
            string uuid = string.Empty;
            using (ManagementClass mc = new ManagementClass("Win32_ComputerSystemProduct"))
            using (ManagementObjectCollection moc = mc.GetInstances())
            {
                foreach (ManagementObject mo in moc)
                {
                    uuid = mo.Properties["UUID"]?.Value?.ToString();
                    break;
                }
            }
            return uuid;
        }


        public async Task<bool> StartChatAsync()
        {
            HttpClient http = new HttpClient();
            var client = new AsyncFtpClient(url, user, pass);
            await client.Connect();
            while (button3.ForeColor == Color.Green)
            {
                Console.WriteLine(loopchat);

                foreach (FtpListItem item in await client.GetListing(path + "Users/", FtpListOption.Recursive))
                {
                    if (item.Type == FtpObjectType.Directory)
                    {
                        Console.WriteLine($"Folder: {item.Name}");



                        string status = await http.GetStringAsync(urlhttp + "Users/" + item.Name.Trim() + "/status.txt");

                        if (status == "Pending")
                        {
                            string PreviousChat = await http.GetStringAsync(urlhttp + "Chat/" + "chat.txt");

                            string NewMessage = await http.GetStringAsync(urlhttp + "Users/" + item.Name.Trim() + "/chat.txt");

                            await client.Connect();

                            byte[] NewChat = Encoding.UTF8.GetBytes(PreviousChat + "\n" + NewMessage);
                            byte[] NewStatus = Encoding.UTF8.GetBytes("MessageSent");
                            await client.UploadBytes(NewChat, path + "Chat/" + "chat.txt", FtpRemoteExists.Overwrite, createRemoteDir: true);
                            await client.UploadBytes(NewStatus, path + "Users/" + GetMotherboardUUID() + "/status.txt", FtpRemoteExists.Overwrite, createRemoteDir: true);

                            await Task.Delay(150);

                            await client.Disconnect();

                        }
                        await Task.Delay(500);
                        loopchat++;
                    }
                }
            }
            MessageBox.Show("Chat stopped.");
            return true;
        }

        private static readonly HttpClient http = new HttpClient();

        public async Task<bool> StartChatNewAsync()
        {
            Console.WriteLine(loopchat);
            var client = new AsyncFtpClient(url, user, pass);
            await client.Connect();

            while (button3.ForeColor == Color.Green)
            {
                try
                {
                    // make sure the connection is still alive
                    if (!client.IsConnected)
                        await client.Connect();

                    // no Recursive: we only need the user folders directly under Users/
                    var items = await client.GetListing(path + "Users/");

                    foreach (FtpListItem item in items)
                    {
                        if (item.Type != FtpObjectType.Directory)
                            continue;

                        string status;
                        try
                        {
                            status = (await http.GetStringAsync(urlhttp + "Users/" + item.Name + "/status.txt")).Trim();
                        }
                        catch (HttpRequestException)
                        {
                            continue; // no status.txt in this folder, skip it
                        }
                        Console.WriteLine(DateTime.Now + $" - Folder: {item.Name}");
                        if (status == "Pending")
                        {
                            string previousChat = await http.GetStringAsync(urlhttp + "Chat/chat.txt");
                            string newMessage = await http.GetStringAsync(urlhttp + "Users/" + item.Name + "/chat.txt");

                            byte[] newChat = Encoding.UTF8.GetBytes(previousChat + "\n" + newMessage);
                            byte[] newStatus = Encoding.UTF8.GetBytes("MessageSent");

                            await client.UploadBytes(newChat, path + "Chat/chat.txt",
                                FtpRemoteExists.Overwrite, createRemoteDir: true);

                            await client.UploadBytes(newStatus, path + "Users/" + GetMotherboardUUID() + "/status.txt",
                                FtpRemoteExists.Overwrite, createRemoteDir: true);
                        }

                        loopchat++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Chat loop error: " + ex.Message);
                    await Task.Delay(2000); // back off, then try again
                }

                await Task.Delay(500); // pause between scans so you don't hammer the server
            }

            MessageBox.Show("Chat stopped.");
            return true;
        }


        public async Task<bool> StarIdleAsync()
        {

            HttpClient http = new HttpClient();
            var client = new AsyncFtpClient(url, user, pass);
            await client.Connect();
            while (button3.ForeColor == Color.Green)
            {

                try
                {
                    if (!client.IsConnected)
                        await client.Connect();

                    Console.WriteLine(loopchat);

                    foreach (FtpListItem item in await client.GetListing(path + "Users/", FtpListOption.Recursive))
                    {
                        if (item.Type == FtpObjectType.Directory)
                        {
                            Console.WriteLine($"Folder: {item.Name}");



                            string status = await http.GetStringAsync(urlhttp + "Users/" + item.Name.Trim() + "/status.txt");

                            if (status == "Pending")
                            {
                                string PreviousChat = await http.GetStringAsync(urlhttp + "Chat/" + "chat.txt");

                                string NewMessage = await http.GetStringAsync(urlhttp + "Users/" + item.Name.Trim() + "/chat.txt");

                                await client.Connect();

                                byte[] NewChat = Encoding.UTF8.GetBytes(PreviousChat + "\n" + NewMessage);
                                byte[] NewStatus = Encoding.UTF8.GetBytes("MessageSent");
                                await client.UploadBytes(NewChat, path + "Chat/" + "chat.txt", FtpRemoteExists.Overwrite, createRemoteDir: true);
                                await client.UploadBytes(NewStatus, path + "Users/" + GetMotherboardUUID() + "/status.txt", FtpRemoteExists.Overwrite, createRemoteDir: true);

                                await Task.Delay(150);

                                await client.Disconnect();

                            }
                            await Task.Delay(500);
                            loopchat++;
                        }
                    }



                    await client.Execute("NOOP");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FTP error: " + ex.Message);

                    try { await client.Disconnect(); } catch { }
                    await Task.Delay(3000);  
                    continue;                 
                }

                await Task.Delay(700);








            }
            MessageBox.Show("Chat stopped.");
            return true;
        }


        private async void button3_Click(object sender, EventArgs e)
        {
            if (button3.ForeColor == Color.Red)
            {
                StarIdleAsync();
                button3.ForeColor = Color.Green;
            }
            else if (button3.ForeColor == Color.Green)
            {
                button3.ForeColor = Color.Red;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var client = new AsyncFtpClient(url, user, pass);
            await client.Connect();
            byte[] bytes = Encoding.UTF8.GetBytes("---------------------------------------ChatPlus--------------------------------------");
           
            //byte[] bytes = Encoding.UTF8.GetBytes(@"{\rtf1\ansi This is in \b bold\b0.}");

            await client.UploadBytes(bytes, path + "Chat/" + "chat.txt");

            await client.Disconnect();

            MessageBox.Show("Cleaned Chat");
        }
    }
}
