using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class ModuleCache
    {
        Dictionary<string, IGraphPlotDataEx> m_rgModules = new Dictionary<string, IGraphPlotDataEx>();

        public ModuleCache()
        {
        }

        public List<string> Names
        {
            get
            {
                List<string> rgstrNames = new List<string>();

                foreach (KeyValuePair<string, IGraphPlotDataEx> kv in m_rgModules)
                {
                    rgstrNames.Add(kv.Key);
                }

                return rgstrNames;
            }
        }

        public IGraphPlotDataEx Find(string strName, bool bThrowException = true)
        {
            if (string.IsNullOrEmpty(strName))
                return null;

            if (!m_rgModules.ContainsKey(strName))
            {
                if (bThrowException)
                    throw new Exception("Could not find the custom module '" + strName + "'!");

                return null;
            }

            return m_rgModules[strName];
        }

        public List<string> Load()
        {
            List<string> rgstrNames = new List<string>();
            string strPath = Modules.CustomGraphingDirectory;

            if (!Directory.Exists(strPath))
                throw new Exception("The custom graph directory '" + strPath + "' could not be found!");

            string[] rgstrFiles = Directory.GetFiles(strPath);

            foreach (string strFile in rgstrFiles)
            {
                FileInfo fi = new FileInfo(strFile);
                if (fi.Extension.ToLower() == ".dll")
                {
                    Exception err;
                    IGraphPlotDataEx idata = load(strFile, out err);
                    if (idata != null)
                    {
                        if (!m_rgModules.ContainsKey(idata.Name))
                        {
                            string strName = idata.Name;
                            m_rgModules.Add(strName, idata);
                            rgstrNames.Add(strName);
                        }
                    }
                }
            }

            return rgstrNames;
        }

        private IGraphPlotDataEx load(string strPath, out Exception err)
        {
            err = null;

            try
            {
                Assembly a = Assembly.LoadFile(strPath);
                AssemblyName aName = a.GetName();
                IGraphPlotDataEx idata = null;

                foreach (Type t in a.GetTypes())
                {
                    if (t.IsPublic)
                    {
                        Type iface = t.GetInterface("IGraphPlotDataEx");

                        if (iface != null)
                        {
                            object obj = Activator.CreateInstance(t);
                            idata = (IGraphPlotDataEx)obj;
                            return idata;
                        }
                    }
                }

                return null;
            }
            catch (Exception excpt)
            {
                err = excpt;
                return null;
            }
        }

    }
}
