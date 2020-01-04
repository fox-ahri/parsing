using System;
using System.Collections.Generic;
using System.IO;
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
                    MessageBox.Show("配置文件错误，请矫正", "错误：", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void StartTask(Params p)
        {
            ReadFile reader = new ReadFile();
            List<string> errotLog = new List<string>();
            WindowInfo info;
            info.name = "";
            info.process = 0;
            info.success_count_lines = 0;
            info.error_count_lines = 0;
            DirectoryInfo TheFolder = new DirectoryInfo(p.input);
            FileInfo[] fileList = TheFolder.GetFiles();
            try
            {
                for (int fileIndex = 0; fileIndex < fileList.Length; fileIndex++)
                {
                    List<string> fileLines = reader.ReadFileLineAsStringList(fileList[fileIndex].FullName, 3);
                    for (int lineIndex = 0; lineIndex < fileLines.Count; lineIndex++)
                    {
                        try
                        {
                            if (lineIndex == 10)
                                throw new Exception();
                            info.process = fileIndex * 100 / fileList.Length;
                            info.success_count_lines++;
                            StartHandler?.Invoke(info);
                        }
                        catch (Exception ex)
                        {
                            info.process = fileIndex * 100 / fileList.Length;
                            info.error_count_lines++;
                            StartHandler?.Invoke(info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //while (true)
            //{
            //    info.process++;
            //    StartHandler?.Invoke(info);
            //    Console.WriteLine(p.input + p.output);
            //    Console.WriteLine(p.error);
            //    if (info.process > 10)
            //    {
            //        break;
            //    }
            //    Thread.Sleep(1000);
            //}
            EndHandler?.Invoke(info);
        }
    }
}