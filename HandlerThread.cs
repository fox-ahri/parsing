using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
                string[] lines = System.IO.File.ReadAllLines(configPath);
                config = JsonConvert.DeserializeObject<Dictionary<string, string>>(string.Join("\n", lines));
                WindowInfo info;
                info.name = config["title"];
                info.process = 0;
                StartHandler?.Invoke(info);
            }
        }

        public void StartTask(Params p)
        {
            WindowInfo info;
            info.name = "";
            info.process = 0;
            while (true)
            {
                info.process++;
                StartHandler?.Invoke(info);
                Console.WriteLine(p.input + p.output);
                Console.WriteLine(p.error);
                if (info.process > 10)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            EndHandler?.Invoke(info);
        }
    }
}