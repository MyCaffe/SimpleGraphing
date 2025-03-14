﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd
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
            string[] rgstrFiles = null;

            if (Directory.Exists(strPath))
                rgstrFiles = Directory.GetFiles(strPath);

            if (rgstrFiles == null || rgstrFiles.Length == 0)
                rgstrFiles = Directory.GetFiles(Modules.AssemblyDirectory, "CustomGraphing.*");

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
                Exception[] rgLoaderExceptions = null;

                if (excpt is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = excpt as ReflectionTypeLoadException;
                    rgLoaderExceptions = typeLoadException.LoaderExceptions;
                }

                if (rgLoaderExceptions != null && rgLoaderExceptions.Length > 0)
                {
                    excpt = new Exception(excpt.Message, rgLoaderExceptions[0]);
                    Trace.WriteLine("Loader Exception: " + rgLoaderExceptions[0].Message);
                }

                err = excpt;
                return null;
            }
        }
    }
}
