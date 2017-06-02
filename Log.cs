using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Fleck.aiplay
{
    class Log
    {
        private string LogPath;
        private string spath = "log";
        private StreamWriter log;
        public Log()
        {
            if (!Directory.Exists(spath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(spath);
                directoryInfo.Create();
            }

            LogPath = DateTime.Now.ToLongDateString();
            log = new StreamWriter(spath + "/" + LogPath + "-" + Setting.port + ".log", true);
        }

        public Log(string name)
        {
            if (!Directory.Exists(spath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(spath);
                directoryInfo.Create();
            }
            log = new StreamWriter(spath + "/" + name + ".log", true);
        }

        public void WriteInfo(string message)
        {
            if (LogPath != DateTime.Now.ToLongDateString())
            {
                log.Close();
                LogPath = DateTime.Now.ToLongDateString();
                log = new StreamWriter(spath + "/" + LogPath + "-" + Setting.port + ".log", true);
            }
            WriteInfo("{0}", message);
        }

        public void WritePosition(string message)
        {
            WriteInfo("{0}", message);
        }

        public void WriteInfo(string format, params object[] obj)
        {
            try
            {
                log.WriteLine(string.Format("[{0}] {1}", System.DateTime.Now, string.Format(format, obj)));
                log.Flush();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
