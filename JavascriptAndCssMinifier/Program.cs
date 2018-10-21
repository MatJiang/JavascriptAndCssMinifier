using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace JavascriptAndCssMinifier
{
    class Program
    {
        static void Main(string[] args)
        {
            var loadingFolder = ConfigurationManager.AppSettings["folder"];
            try
            {
                if (Directory.Exists(loadingFolder))
                {
                    LoadFolderRecursive(new DirectoryInfo(loadingFolder));
                }
#if DEBUG
                Console.WriteLine("Enter any key to leave");
                Console.Read();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 遞迴讀取資料夾
        /// </summary>
        /// <param name="baseDir"></param>
        private static void LoadFolderRecursive(DirectoryInfo baseDir)
        {
            foreach (var childDir in baseDir.GetDirectories())
                LoadFolderRecursive(childDir);

            var files = baseDir.GetFiles().Where(f =>
              (!f.Name.ToLower().EndsWith(".min.js") && f.Extension.ToLower() == ".js") ||
              (!f.Name.ToLower().EndsWith(".min.css") && f.Extension.ToLower() == ".css"));

            if (files.Any())
            {
                Console.WriteLine($"Loading：{baseDir.FullName}");

                foreach (var file in files)
                {
                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            Console.WriteLine(file.Name);
                            var input = sr.ReadToEnd();
                            Microsoft.Ajax.Utilities.Minifier minifier = new Microsoft.Ajax.Utilities.Minifier();
                            var fileName = file.Name.Replace(file.Extension, $".min{file.Extension}");
                            using (FileStream fsw = new FileStream($"{file.DirectoryName}/{fileName}", FileMode.Create))
                            {
                                using (StreamWriter sw = new StreamWriter(fsw))
                                {
                                    switch (file.Extension.ToLower())
                                    {
                                        case ".js":
                                            sw.Write(minifier.MinifyJavaScript(input));
                                            break;
                                        case ".css":
                                            sw.Write(minifier.MinifyStyleSheet(input));
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine($"Loading：{baseDir.FullName} Done!");
            }
        }
    }
}
