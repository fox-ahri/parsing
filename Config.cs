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
using System.Xml;
using Newtonsoft.Json;

namespace Template
{
    public partial class Config : Form
    {
        string path = Directory.GetCurrentDirectory() + "\\config.json";
        Dictionary<string, string> config;
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
                File.WriteAllText(path, "{\n    \"title\": \"Title\"\n}");
                btnReload_Click(sender, e);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        /// <summary>
        /// 载入配置文件
        /// </summary>
        void LoadConfig()
        {
            if (!File.Exists(path))
            {
                Console.WriteLine(1);
                File.WriteAllText(path, "{\n    \"title\": \"Title\"\n}");
            }
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int i=0;i<lines.Length - 1; i++)
            {
                lines[i] = lines[i].Replace(@"\", @"\\").Replace(@"\\\\", @"\\");
            }
            try
            {
                config = JsonConvert.DeserializeObject<Dictionary<string, string>>(string.Join("\n", lines));
                SetUI();
                SetJson();
            }
            catch (Exception ex)
            {
                MessageBox.Show("配置文件错误 请重置 详细信息：\n" + ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetStatus(2);
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        void SaveConfig()
        {
            try
            {
                string tmp = "{\n";
                foreach (KeyValuePair<string, string> kvp in config)
                {
                    string t = kvp.Value.Replace(@"\", @"\\").Replace(@"\\\\", @"\\");
                    tmp += "\t\"" + kvp.Key + "\": \"" + t + "\",\n";
                }
                tmp += "}";
                File.WriteAllText(path, tmp);
                SetStatus(2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Json 格式错误 详细信息：\n" + ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Menu
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
        #endregion

        #region Set Data
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

        void SetUI()
        {
            this.txtTitle.Text = config.ContainsKey("title") ? config["title"] : "";
            this.txtInput.Text = config.ContainsKey("input") ? config["input"] : "";
            this.txtOutput.Text = config.ContainsKey("output") ? config["output"] : "";
            this.txtErrorlog.Text = config.ContainsKey("error") ? config["error"] : "";
            this.txtTemplate.Text = config.ContainsKey("template") ? config["template"] : "";
        }

        void SetJson(string k = "", string v = "")
        {
            if (!k.Equals(""))
            {
                if (config.ContainsKey(k))
                {
                    config[k] = v;
                }
                else
                {
                    config.Add(k, v);
                }
            }
            string tmp = "{\n";
            foreach (KeyValuePair<string, string> kvp in config)
            {
                tmp += "\t\"" + kvp.Key + "\": \"" + kvp.Value + "\",\n";
            }
            tmp += "}";
            this.rtxtConfig.Text = tmp;
        }
        #endregion

        #region Change JSON
        private void rtxtConfig_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            try
            {
                config = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.rtxtConfig.Text.Replace("\\", "\\\\"));
                SetUI();
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Change UI
        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            SetJson("title", this.txtTitle.Text);
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            SetJson("input", this.txtInput.Text);
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            SetJson("output", this.txtOutput.Text);
        }

        private void txtErrorlog_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            SetJson("error", this.txtErrorlog.Text);
        }

        private void txtTemplate_TextChanged(object sender, EventArgs e)
        {
            SetStatus(1);
            SetJson("template", this.txtTemplate.Text);
        }
        #endregion
    }
}
