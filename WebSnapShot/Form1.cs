using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace WebSnapShot
{
    public partial class Form1 : Form
    {
        //WebBrowser webBrowser1 = new WebBrowser();
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] FilesList = Directory.GetFiles(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
                foreach (string item in FilesList)
                {
                    try
                    {
                        if (File.GetLastAccessTime(item) < DateTime.Now.AddHours(-48))
                        {
                            File.Delete(item);
                            richTextBox1.Text += "\n" + (item) + " *Deleted* \n";
                            richTextBox1.SelectionStart = richTextBox1.Text.Length;
                            richTextBox1.ScrollToCaret();
                            Application.DoEvents();
                        }
                    }
                    catch (Exception Exp)
                    {
                        richTextBox1.Text += (Exp) + " \n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        Application.DoEvents();
                    }

                }
                button1.ForeColor = Color.White;
                button1.Text = "Started";
                button1.BackColor = Color.Red;
                richTextBox1.Text = "";
                richTextBox1.Text += "START JOB \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
                    try
                    {
                        File.Delete(ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim());
                    }
                    catch { }
                if (ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim().ToLower().StartsWith("http"))
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim(), ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim());
                    richTextBox1.Text += "IMAGE SAVED \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }
                else
                {
                    File.Copy(ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim(), ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim(),true);
                }

                if (File.Exists(ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim()))
                    render();
                else
                {
                    button1.ForeColor = Color.White;
                    button1.Text = "START";
                    button1.BackColor = Color.Navy;
                    timer1.Enabled = true;
                    richTextBox1.Text += "Error  Save Image From API or image is not exist\n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }
            }
            catch (Exception Ex)
            {
                richTextBox1.Text += Ex.Message + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
                button1.ForeColor = Color.White;
                button1.Text = "START";
                button1.BackColor = Color.Navy;
                timer1.Enabled = true;
            }
        }
        protected void render()
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["AeRenderPath"].ToString().Trim() + "\"";
                string DateTimeStr = string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);
                DirectoryInfo Dir = new DirectoryInfo(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
                if (!Dir.Exists)
                    Dir.Create();
                proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4" + "\"";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.EnableRaisingEvents = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                if (!proc.Start())
                {
                    return;
                }
                proc.PriorityClass = ProcessPriorityClass.Normal;
                StreamReader reader = proc.StandardOutput;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (richTextBox1.Lines.Length > 8)
                    {
                        richTextBox1.Text = "";
                    }
                    richTextBox1.Text += (line) + " \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }
                proc.Close();
                try
                {
                    string StaticDestFileName = ConfigurationSettings.AppSettings["ScheduleDestFileName"].ToString().Trim();
                    // File.Delete(StaticDestFileName);
                    File.Copy(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4", StaticDestFileName, true);
                    richTextBox1.Text += "COPY FINAL:" + StaticDestFileName + " \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }
                catch (Exception Ex)
                {
                    richTextBox1.Text += Ex.Message + " \n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    Application.DoEvents();
                }
                button1.ForeColor = Color.White;
                button1.Text = "START";
                button1.BackColor = Color.Navy;
                timer1.Enabled = true;
            }
            catch { timer1.Enabled = true; }
        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {

            int StartMinute = int.Parse(ConfigurationSettings.AppSettings["TimeScheduleMinute"].ToString().Trim());
            if (DateTime.Now.Minute >= StartMinute && DateTime.Now.Minute <= StartMinute + 1)
            {
                timer1.Enabled = false;
                button1_Click(null, null);
            }
        }
    }
}
