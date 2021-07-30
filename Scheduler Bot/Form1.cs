using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Scheduler_Bot.Properties;
using Telegram.Bot.Types.InputFiles;
using System.Collections;

namespace Scheduler_Bot
{
    public partial class Form1 : Form
    {
        TelegramBotClient bot;
        ChatId chatId;
        int folder = 0;
        int file = 0;
        int posted = 0;
        int seconds = 0;

        public Form1()
        {
            InitializeComponent();
            if (Settings.Default.running)
            {
                DialogResult que = MessageBox.Show("It seems that you closed application, when images aren't fully sent. Do you want to continue?", "Saved settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (que == DialogResult.Yes)
                {
                    foreach (string str in Settings.Default.folders)
                    {
                        folderList.Items.Add(str);
                    }
                    folder = Settings.Default.folderposted;
                    file = Settings.Default.fileposted;
                    tokenTxt.Text = Settings.Default.token;
                    chidTxt.Text = Settings.Default.chatId.ToString();
                    imgNum.Text = GetFilesNum().ToString();
                    bot = new TelegramBotClient(Settings.Default.token);
                    chatId = new ChatId(Settings.Default.chatId);
                    seconds = Settings.Default.timer * 60;
                    if (GetFiles(folder).Length == file)
                    {
                        folder++;
                        file = 0;
                    }
                    if (folderList.Items.Count > folder)
                    {
                        PostImage(folder, file == 0 ? 0 : ++file);
                        Settings.Default.fileposted = file;
                        Settings.Default.folderposted = folder;
                        Settings.Default.Save();
                        timer1.Enabled = true;
                        button3.Text = "Stop posting";
                        notifyIcon.Text = "Bot is working...";
                        tokenTxt.Enabled = false;
                        chidTxt.Enabled = false;
                        button1.Enabled = false;
                        button2.Enabled = false;
                        timeTxt.Enabled = false;
                        button5.Enabled = true;
                        button6.Enabled = true;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select folder with pictures you'd like to post";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK) folderList.Items.Add(fbd.SelectedPath);
            imgNum.Text = GetFilesNum().ToString();
        }

        private string[] GetFiles(int folderNum)
        {
            string fold = folderList.Items[folderNum].ToString();
            string[] files = Directory.GetFiles(fold, "*.png");
            string[] files1 = Directory.GetFiles(fold, "*.jpg");
            string[] files2 = Directory.GetFiles(fold, "*.bmp");
            List<string> filesout = new List<string>();
            foreach (string file in files) filesout.Add(file);
            foreach (string file in files1) filesout.Add(file);
            foreach (string file in files2) filesout.Add(file);
            return filesout.ToArray();
        }

        private int GetFilesNum()
        {
            int piccount = 0;
            foreach (string folder in folderList.Items)
            {
                string[] files = Directory.GetFiles(folder, "*.png");
                string[] files1 = Directory.GetFiles(folder, "*.jpg");
                string[] files2 = Directory.GetFiles(folder, "*.bmp");
                piccount += files.Length + files1.Length + files2.Length;
            }
            return piccount;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int selected = folderList.SelectedIndex;
            folderList.Items.RemoveAt(selected);
            imgNum.Text = GetFilesNum().ToString();
        }

        private async void PostImage(int folder, int image)
        {
            try
            {
                var img = GetFiles(folder)[image];
                InputOnlineFile file = new InputOnlineFile(new FileStream(img, FileMode.Open));
                var msg = await bot.SendPhotoAsync(chatId, file);
                if (notifyPost.Checked) notifyIcon.ShowBalloonTip(3000);
                posted++;
                label7.Text = "Images posted: " + posted;
            }
            catch (Exception ex)
            {
                if (notifyIcon.Visible)
                {
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error occured";
                    notifyIcon.BalloonTipText = "Error text: " + ex.Message;
                    notifyIcon.ShowBalloonTip(1000);
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon.BalloonTipText = "Bot posted an image";
                    notifyIcon.BalloonTipTitle = "Image posted";
                }
                else
                {
                    MessageBox.Show("Error occured: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                tokenTxt.Enabled = true;
                chidTxt.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                timeTxt.Enabled = true;
                button5.Enabled = false;
                button6.Enabled = false;
                Settings.Default.running = false;
                Settings.Default.Save();
                button3.Text = "Start posting";
                notifyIcon.Text = "Bot is still";
            }
            else
            {
                if (tokenTxt.Text == "" || chidTxt.Text == "")
                {
                    MessageBox.Show("Check token or chat id. If you don't know what to do, Google is always accessible.", "( ﾉ ﾟｰﾟ)ﾉ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (folderList.Items.Count == 0 || GetFilesNum() == 0)
                {
                    MessageBox.Show("Check folder list", "Oh yeah", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    bot = new TelegramBotClient(tokenTxt.Text);
                    chatId = new ChatId(Convert.ToInt64(chidTxt.Text));
                    folder = 0;
                    posted = 0;
                    PostImage(0, 0);
                    file = 1;
                    seconds = Convert.ToInt32(timeTxt.Value * 60);
                    timer1.Enabled = true;
                    button3.Text = "Stop posting";
                    notifyIcon.Text = "Bot is working...";
                    tokenTxt.Enabled = false;
                    chidTxt.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    timeTxt.Enabled = false;
                    button5.Enabled = true;
                    button6.Enabled = true;
                    Settings.Default.chatId = Convert.ToInt64(chidTxt.Text);
                    Settings.Default.token = tokenTxt.Text;
                    Settings.Default.timer = Convert.ToInt32(timeTxt.Value);
                    List<string> folders = new List<string>();
                    foreach (string str in folderList.Items) folders.Add(str);
                    Settings.Default.folders = new ArrayList(folders);
                    Settings.Default.running = true;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hehehe, error\n" + ex.Message, "(╯‵□′)╯︵┻━┻", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void Work()
        {
            PostImage(folder, file++);
            Settings.Default.fileposted = file;
            Settings.Default.folderposted = folder;
            if (GetFiles(folder).Length == file)
            {
                folder++;
                file = 0;
            }
            if (folderList.Items.Count == folder)
            {
                timer1.Enabled = false;
                button3.Text = "Start posting";
                Settings.Default.fileposted = 0;
                Settings.Default.folderposted = 0;
                Settings.Default.running = false;
            }
            Settings.Default.Save();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            notifyIcon.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = true;
            Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Work();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            secTxt.Text = seconds--.ToString();
            if (seconds < 0)
            {
                Work();
                seconds = Settings.Default.timer * 60;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("File info:\n\nFile number: " + Settings.Default.fileposted + "\nFolder number: " + Settings.Default.folderposted + "\nFile name: " + GetFiles(folder)[file], "File info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
