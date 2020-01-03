using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Template
{
    public partial class Config : Form
    {
        string path = Directory.GetCurrentDirectory() + "\\config.json";
        public Config()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确认重置配置？", "重置配置", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                System.IO.File.WriteAllText(path, "{\n    \"title\": \"Title\"\n}");
                btnReload_Click(sender, e);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.rtxtConfig.Text);
                System.IO.File.WriteAllText(path, this.rtxtConfig.Text);
                SetStatus(2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Json 格式错误 详细信息：\n" + ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 载入配置文件
        /// </summary>
        void LoadConfig()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{\n    \"title\": \"Title\"\n}");
            }
            string[] lines = File.ReadAllLines(path);
            this.rtxtConfig.Text = string.Join("\n", lines);
            try
            {
                Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.rtxtConfig.Text);
                this.txtTitle.Text = config.ContainsKey("title") ? config["title"] : "";
                this.txtInput.Text = config.ContainsKey("input") ? config["input"] : "";
                this.txtOutput.Text = config.ContainsKey("output") ? config["output"] : "";
                this.txtErrorlog.Text = config.ContainsKey("error") ? config["error"] : "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("配置文件错误 请重置 详细信息：\n" + ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetStatus(2);
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
        }

        private void menuReload_Click(object sender, EventArgs e)
        {
            btnReload_Click(sender, e);
        }

        private void menuReset_Click(object sender, EventArgs e)
        {
            btnReset_Click(sender, e);
        }

        void SetStatus(int type)
        {
            if(type == 1)
            {
                this.lbStatus.Text = "未保存";
                this.lbStatus.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                this.lbStatus.Text = "已保存";
                this.lbStatus.ForeColor = System.Drawing.Color.Lime;
            }
        }

        #region Change JSON
        private void rtxtConfig_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            try
            {
                Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.rtxtConfig.Text);
                this.txtTitle.Text = config["title"];
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Change UI
        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
        }
        #endregion
    }
}
