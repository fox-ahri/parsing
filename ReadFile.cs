using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Template
{
    class ReadFile
    {
        public List<string> ReadFileLineAsStringList(string path, int part = 0, string separator = "----------")
        {
            List<string> fileData = new List<string>();
            foreach (string i in File.ReadAllLines(path, Encoding.UTF8))
            {
                fileData.Add(i);
            }
            if (part == 0)
            {
                return fileData;
            }
            else
            {
                return GetPartBySeparator(fileData, part, separator);
            }
        }

        public List<string> GetPartBySeparator(List<string> fileData, int part = 1, string separator = "----------")
        {
            List<string> partFileData = new List<string>();
            int num = 0;
            foreach (string i in fileData)
            {
                if (num == part - 1)
                {
                    partFileData.Add(i);
                }
                else if (i.Contains(separator))
                {
                    num++;
                }
            }
            return partFileData;
        }
    }
}

