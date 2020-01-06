using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Template
{
    public partial class Main : Form
    {
        public static Main MyWindow;
        string configPath = Directory.GetCurrentDirectory() + "\\config.json";
        string errorLogPath = Directory.GetCurrentDirectory() + "\\error_log.txt";
        Dictionary<string, string> config;
        public Main()
        {
            InitializeComponent();
            if (File.Exists(configPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configPath);
                    config = JsonConvert.DeserializeObject<Dictionary<string, string>>(string.Join("\n", lines));
                    this.Text = config["title"];
                    if (config.ContainsKey("input") && !config["input"].Equals(""))
                        this.txtInput.Text = config["input"];
                    else
                        this.txtInput.Text = Directory.GetCurrentDirectory() + "\\input";
                    if (config.ContainsKey("output") && !config["output"].Equals(""))
                        this.txtOutput.Text = config["output"];
                    else
                        this.txtOutput.Text = Directory.GetCurrentDirectory() + "\\output";
                    if (config.ContainsKey("error") && !config["error"].Equals(""))
                    {
                        if (Directory.Exists(Path.GetDirectoryName(config["error"])))
                        {
                            this.errorLogPath = config["error"];
                        }
                        else
                        {
                            MessageBox.Show("配置文件中错误日志的路径不存在，已设置为：\n" + errorLogPath, "错误：", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("配置文件错误，请矫正", "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txtInput.Text = Directory.GetCurrentDirectory() + "\\input";
                    this.txtOutput.Text = Directory.GetCurrentDirectory() + "\\output";
                }
            }
            else
            {
                this.txtInput.Text = Directory.GetCurrentDirectory() + "\\input";
                this.txtOutput.Text = Directory.GetCurrentDirectory() + "\\output";
            }
            Main.MyWindow = this;
        }

        private void btnInput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbDlg = new FolderBrowserDialog();
            if (fbDlg.ShowDialog() == DialogResult.OK)
            {
                this.txtInput.Text = fbDlg.SelectedPath;
                this.txtOutput.Text = fbDlg.SelectedPath + "\\output";
            }
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbDlg = new FolderBrowserDialog();
            if (fbDlg.ShowDialog() == DialogResult.OK)
            {
                this.txtOutput.Text = fbDlg.SelectedPath;
            }
        }

        private HandlerThread HT;
        Thread myThread = null;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(this.txtInput.Text))
            {
                if (!Directory.Exists(this.txtOutput.Text))
                {
                    Directory.CreateDirectory(this.txtOutput.Text);
                }
                BeforeStart();
                HT = new HandlerThread();
                HT.StartHandler = new Entrust.UpdateStatus(StartOfTask);
                HT.EndHandler = new Entrust.UpdateStatus(EndOfTask);
                Params p;
                p.input = this.txtInput.Text;
                p.output = this.txtOutput.Text;
                p.error = config["error"];
                p.template = config["template"];
                p.fragmentation = this.ckFragmentation.Checked;
                p.ruleFrag = this.txtRuleFrge.Text;
                myThread = new Thread(() => HT.StartTask(p));
                myThread.IsBackground = true;
                myThread.Start();
            }
            else
            {
                MessageBox.Show("输入目录不存在", "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            myThread.Abort();
            this.lbStatus.Text = "任务已取消";
            AfterEnd();
        }

        void BeforeStart()
        {
            this.btnStop.Visible = true;
            this.btnStart.Enabled = false;
            this.txtInput.Enabled = false;
            this.txtOutput.Enabled = false;
            this.btnInput.Enabled = false;
            this.btnOutput.Enabled = false;
            this.lbStatus.Text = "准备开始";
            this.ckFragmentation.Enabled = false;
            this.txtRuleFrge.Enabled = false;
        }

        void StartOfTask(WindowInfo info)
        {
            if (Main.MyWindow.InvokeRequired)
            {
                BeginInvoke(HT.StartHandler, new object[] { info });
            }
            else
            {
                this.lbStatus.Text = "任务进行中... (" + info.process.ToString() + "%)";
                this.progressBar.Value = info.process;
                this.lbSuccessLinesCount.Text = "已成功处理行数：" + info.success_count_lines.ToString();
                this.lbErrorLinesCount.Text = "已失败处理行数：" + info.error_count_lines.ToString();
            }
        }

        void EndOfTask(WindowInfo info)
        {
            if (Main.MyWindow.InvokeRequired)
            {
                BeginInvoke(HT.EndHandler, new object[] { info });
            }
            else
            {
                this.progressBar.Value = 100;
                this.lbStatus.Text = "任务进行中... (100%)";
                this.lbStatus.Text = "任务已完成！";
                MessageBox.Show("任务已完成！", "提示：", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AfterEnd();
            }
        }

        void AfterEnd()
        {
            this.btnStop.Visible = false;
            this.btnStart.Enabled = true;
            this.txtInput.Enabled = true;
            this.txtOutput.Enabled = true;
            this.btnInput.Enabled = true;
            this.btnOutput.Enabled = true;
            this.ckFragmentation.Enabled = true;
            this.txtRuleFrge.Enabled = true;
        }

        private void menuInput_Click(object sender, EventArgs e)
        {
            btnInput_Click(sender, e);
        }

        private void menuOutput_Click(object sender, EventArgs e)
        {
            btnOutput_Click(sender, e);
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void menuConfig_Click(object sender, EventArgs e)
        {
            Config config = new Config();
            config.Show();
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(this.txtInput.Text))
            {
                DirectoryInfo TheFolder = new DirectoryInfo(this.txtInput.Text);
                this.lbCount.Text = "待处理文件数： " + TheFolder.GetFiles().Length;
                this.lbCount.ForeColor = System.Drawing.Color.DeepSkyBlue;
            }
            else
            {
                this.lbCount.Text = "输入路径不存在";
                this.lbCount.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void ckFragmentation_CheckedChanged(object sender, EventArgs e)
        {
            this.txtRuleFrge.Visible = this.ckFragmentation.Checked;
        }
    }
}
