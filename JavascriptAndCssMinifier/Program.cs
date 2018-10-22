using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace JavascriptAndCssMinifier
{
    class Program
    {
        static bool _overwrite = false;

        static void Main(string[] args)
        {
            var loadingFolder = ConfigurationManager.AppSettings["folder"];
            _overwrite = ConfigurationManager.AppSettings["overwrite"].ToLower() == "true" ? true : false;
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
                            var fileName = _overwrite ? file.Name : file.Name.Replace(file.Extension, $".min{file.Extension}");
                            if (_overwrite)
                            {
                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    fs.SetLength(0);
                                    SetContent(file.Extension, input, sw);
                                }
                            }
                            else
                            {
                                using (FileStream fsw = new FileStream($"{file.DirectoryName}/{fileName}", FileMode.Create))
                                {
                                    using (StreamWriter sw = new StreamWriter(fsw))
                                    {
                                        SetContent(file.Extension, input, sw);
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine($"Loading：{baseDir.FullName} Done!");
            }
        }

        /// <summary>
        /// Set File Content
        /// </summary>
        /// <param name="extension">file extension</param>
        /// <param name="input">input content</param>
        /// <param name="sw">StreamWriter</param>
        private static void SetContent(string extension, string input, StreamWriter sw)
        {
            Microsoft.Ajax.Utilities.Minifier minifier = new Microsoft.Ajax.Utilities.Minifier();

            switch (extension.ToLower())
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
