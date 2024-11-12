using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd
{
    public class Modules
    {
        public Modules()
        {
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string CustomGraphingDirectory
        {
            get
            {
                string strPath = Modules.AssemblyDirectory;
                strPath = strPath.TrimEnd('\\');
                strPath += "\\CustomGraphing";

                return strPath;
            }
        }

        public static bool CustomGraphingExists(string strName)
        {
            string strPath = CustomGraphingDirectory + "\\" + strName + ".dll";
            return File.Exists(strPath);
        }
    }
}
