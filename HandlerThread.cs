using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Template
{
    class HandlerThread
    {
        public Entrust.UpdateStatus StartHandler;
        public Entrust.UpdateStatus EndHandler;
        string configPath = Directory.GetCurrentDirectory() + "\\config.json";
        Dictionary<string, string> config;

        public HandlerThread()
        {
            if (File.Exists(configPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configPath);
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        lines[i] = lines[i].Replace("\\", "\\\\");
                    }
                    config = JsonConvert.DeserializeObject<Dictionary<string, string>>(string.Join("\n", lines));
                    WindowInfo info;
                    info.name = config["title"];
                    info.process = 0;
                    info.success_count_lines = 0;
                    info.error_count_lines = 0;
                    StartHandler?.Invoke(info);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("配置文件错误，请矫正\n" + ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        string GetAnalyticData(string temp, string[] line)
        {
            string[] template = temp.Substring(2, temp.Length - 3).Split('_');
            if (template[0] == "V")
            {
                return line[Convert.ToInt32(template[1])];
            }
            else if (template[0] == "R")
            {
                switch (template[2])
                {
                    case "F":
                        return Path.GetFileName(line[Convert.ToInt32(template[1])]);
                    case "N":
                        return Path.GetFileNameWithoutExtension(line[Convert.ToInt32(template[1])]);
                    case "E":
                        return Path.GetExtension(line[Convert.ToInt32(template[1])]);
                    default:
                        Dictionary<string, string> ProductDetailList = JsonConvert.DeserializeObject<Dictionary<string, string>>(line[Convert.ToInt32(template[1])].Replace("\\\"", "\""));
                        if (ProductDetailList.ContainsKey(template[2]))
                        {
                            string result = ProductDetailList[template[2]];
                            if(template.Length > 3)
                            {
                                string[] opera = template[3].Split(',');
                                for (int i = 0; i < opera.Length; i++)
                                {
                                    if (result == opera[i].Split(':')[0])
                                    {
                                        result = opera[i].Split(':')[1];
                                    }
                                }
                            }
                            return result;
                        }
                        return "Error";
                }
            }
            else if (template[0] == "O")
            {
                string result = line[Convert.ToInt32(template[1])];
                string[] opera = template[2].Split(',');
                for (int i = 0; i < opera.Length; i++)
                {
                    switch (opera[i].Split(':')[0])
                    {
                        case "A":
                            result = (Convert.ToDouble(result) + Convert.ToDouble(opera[i].Split(':')[1])).ToString();
                            break;
                        case "S":
                            result = (Convert.ToDouble(result) - Convert.ToDouble(opera[i].Split(':')[1])).ToString();
                            break;
                        case "M":
                            result = (Convert.ToDouble(result) * Convert.ToDouble(opera[i].Split(':')[1])).ToString();
                            break;
                        case "D":
                            result = (Convert.ToDouble(result) / Convert.ToDouble(opera[i].Split(':')[1])).ToString();
                            break;
                        default:
                            return "Error";
                    }
                }
                if (Convert.ToDouble(result) < 0)
                    result = "0";
                return result;
            }
            else
            {
                return "Error";
            }
        }

        public void StartTask(Params p)
        {
            // 文件读取器
            ReadFile reader = new ReadFile();

            // 错误日志
            List<string> errotLog = new List<string>();

            // 回显信息
            WindowInfo info;
            info.name = "";
            info.process = 0;
            info.success_count_lines = 0;
            info.error_count_lines = 0;

            // 生成文件模板
            List<string> TemplateList = reader.ReadFileLineAsStringList(p.template, 1);

            string file_name = "";

            // 使用模板条件
            string[] condition = TemplateList[0].Split('|');
            foreach (string con in condition)
            {
                if (con.Contains("NC"))
                {
                    file_name = con.Split(':')[1];
                }
            }

            // 生成文件标题
            string title = TemplateList[1];

            // 获取文件列表
            DirectoryInfo TheFolder = new DirectoryInfo(p.input);
            FileInfo[] fileList = TheFolder.GetFiles();

            // 处理结果
            Dictionary<string, List<string>> printDict = new Dictionary<string, List<string>>();
            List<string> printList = new List<string>();

            try
            {
                for (int fileIndex = 0; fileIndex < fileList.Length; fileIndex++)
                {
                    if (file_name == "" || fileList[fileIndex].Name.Contains(file_name))
                    {
                        List<string> fileLines = reader.ReadFileLineAsStringList(fileList[fileIndex].FullName, 3);
                        for (int lineIndex = 0; lineIndex < fileLines.Count; lineIndex++)
                        {
                            try
                            {
                                string name = GetAnalyticData(p.ruleFrag, fileLines[lineIndex].Split('\t'));
                                Console.WriteLine(name);
                                Regex regImg = new Regex(@"\$\{.*?\}", RegexOptions.IgnoreCase);
                                MatchCollection matches;
                                bool flag = true;
                                foreach (string con in condition)
                                {
                                    if (con.Contains("$") && GetAnalyticData(con.Split(':')[0], fileLines[lineIndex].Split('\t')) != con.Split(':')[1])
                                    {
                                        flag = false;
                                    }
                                }
                                if (flag)
                                {
                                    // 行模板
                                    string template = TemplateList[2];
                                    for (int tempIndex=0;tempIndex< TemplateList.Count; tempIndex++)
                                    {
                                        if(TemplateList[tempIndex].Split(':')[0] == "condition" && GetAnalyticData(TemplateList[tempIndex].Split(':')[1], fileLines[lineIndex].Split('\t')) == TemplateList[tempIndex].Split(':')[2])
                                        {
                                            template = TemplateList[tempIndex + 1];
                                        }
                                    }

                                    // 将数据插入模板
                                    matches = regImg.Matches(template);
                                    for (int count = 0; count < matches.Count; count++)
                                    {
                                        template = regImg.Replace(template, GetAnalyticData(matches[count].Value, fileLines[lineIndex].Split('\t')), 1);
                                    }

                                    // 保存解析结果
                                    if (p.fragmentation)
                                    {
                                        if (printDict.ContainsKey(name))
                                        {
                                            printDict[name].Add(template);
                                        }
                                        else
                                        {
                                            printDict.Add(name, new List<string>());
                                            printDict[name].Add(template);
                                        }
                                    }
                                    else
                                    {
                                        printList.Add(template);
                                    }
                                }

                                /**
                                 * 回显
                                 */
                                info.process = fileIndex * 100 / fileList.Length;
                                info.success_count_lines++;
                                StartHandler?.Invoke(info);
                            }
                            catch (Exception ex)
                            {
                                info.process = fileIndex * 100 / fileList.Length;
                                info.error_count_lines++;
                                StartHandler?.Invoke(info);
                                MessageBox.Show(ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                if (p.fragmentation)
                {
                    foreach (KeyValuePair<string, List<string>> kvp in printDict)
                    {
                        PrintData(title + "\n" + string.Join("\n", kvp.Value.ToArray()), p.output + "\\" + kvp.Key + ".txt");
                    }
                    printDict.Clear();
                }
                else
                {
                    PrintData(title + "\n" + string.Join("\n", printList.ToArray()), p.output + "\\result.txt");
                    printList.Clear();
                }
                EndHandler?.Invoke(info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void PrintData(string content, string path)
        {
            FileStream fs;
            if (File.Exists(path))
            {
                File.WriteAllText(path, string.Empty);
                fs = new FileStream(path, FileMode.Open, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            }
            StreamWriter sr = new StreamWriter(fs);
            sr.NewLine = "\n";
            sr.WriteLine(content);
            sr.Close();
            fs.Close();
        }
    }
}