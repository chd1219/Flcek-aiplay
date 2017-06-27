using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Fleck.aiplay
{
    class Setting
    {
        static public string port { get; set; }
        static public string level { get; set; }
        static public bool isSupportCloudApi { get; set; }
        static public string engine { get; set; }
        static public int thinktimeout { get; set; }

        public Setting()
        {
            LoadXml();
        }

        public void LoadXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(".\\config.xml");
            XmlNode xn = doc.SelectSingleNode("configuration");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xn1 in xnl)
            {
                XmlElement xe = (XmlElement)xn1;
                if (xe.GetAttribute("key").ToString() == "Port")
                {
                    Setting.port = xe.GetAttribute("value").ToString();
                }
                if (xe.GetAttribute("key").ToString() == "Level")
                {
                    Setting.level = xe.GetAttribute("value").ToString();
                }
                if (xe.GetAttribute("key").ToString() == "TimeOut")
                {
                    Setting.thinktimeout = int.Parse(xe.GetAttribute("value").ToString());
                }
                if (xe.GetAttribute("key").ToString() == "CloudAPI")
                {
                    Setting.isSupportCloudApi = Convert.ToBoolean(xe.GetAttribute("value"));
                }
                if (xe.GetAttribute("key").ToString() == "EnginePath")
                {
                    Setting.engine = xe.GetAttribute("value").ToString();
                }
            }
        }
    }    
}
