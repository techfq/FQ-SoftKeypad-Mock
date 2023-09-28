using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator_WPF
{
    public class IniFile
    {
        private readonly Dictionary<string, Dictionary<string, string>> sections;

        public IniFile(string filePath)
        {
            sections = new Dictionary<string, Dictionary<string, string>>();

            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    string section = "";
                    string[] keyValue = new string[1];

                    while (!reader.EndOfStream)
                    {
                        string line = "" + reader.ReadLine();

                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            section = line.Substring(1, line.Length - 2);
                        }
                        else if (!string.IsNullOrEmpty(line))
                        {
                            keyValue = line.Split('=');

                            if (keyValue.Length == 2)
                            {
                                var key = keyValue[0].Trim();
                                var value = keyValue[1].Trim();

                                if (!sections.ContainsKey(section))
                                {
                                    sections.Add(section, new Dictionary<string, string>());
                                }

                                sections[section].Add(key, value);
                            }
                        }
                    }
                }
            }
        }

        public string ReadValue(string section, string key, string defaultValue = "")
        {
            if (!sections.ContainsKey(section) || !sections[section].ContainsKey(key))
            {
                return defaultValue;
            }

            return sections[section][key];
        }

        public void WriteValue(string section, string key, string value)
        {
            if (!sections.ContainsKey(section))
            {
                sections.Add(section, new Dictionary<string, string>());
            }

            sections[section][key] = value;
        }

        public void Save(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var section in sections.Keys)
                {
                    writer.WriteLine($"[{section}]");

                    foreach (var keyValue in sections[section])
                    {
                        writer.WriteLine($"{keyValue.Key}={keyValue.Value}");
                    }
                }
            }
        }
    }

}
