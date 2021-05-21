using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SnipOCR
{
    class MyXml
    {
        /// <summary>
        /// Xml文件的路径
        /// </summary>
        public static string XMLPATH = Directory.GetCurrentDirectory() + "/configure.xml";

        /// <summary>
        /// 获取对应节点的值
        /// </summary>
        /// <param name="Node">节点名</param>
        /// <returns></returns>
        public static string GetXmlValue(string Node)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(XMLPATH);
                XmlNodeList list = doc.GetElementsByTagName(Node);
                string str = list[list.Count - 1].InnerText;
                return str;
            }
            catch
            {
                return "false";
            }
        }

        /// <summary>
        /// 初始化Xml, 添加API_Key,Secret_Key
        /// <para name="API_KEY">API Key</para>
        /// <para name="Secret_Key">Serect Key</para>
        /// </summary>
        public static void CreateXml(string API_KEY, string Secret_Key)
        {
            XmlTextWriter writer = new XmlTextWriter(XMLPATH, Encoding.UTF8);

            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument(); //XML声明 
            writer.WriteStartElement("Main");
            writer.WriteStartElement("KEY");
            writer.WriteAttributeString("id", "1");
            writer.WriteElementString("API_Key",API_KEY);
            writer.WriteElementString("Secret_Key", Secret_Key);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
        }

        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="Node">节点</param>
        /// <param name="Value">值</param>
        /// <param name="id">id号</param>
        /// <param name="MainNode">主节点 默认为Main</param>
        public static void InsertNode(string Node,string Value, int id, string MainNode)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(XMLPATH))
            {
                xmlDoc.Load(XMLPATH);
                XmlNodeList memberlist = xmlDoc.GetElementsByTagName(MainNode);
                XmlElement member = xmlDoc.CreateElement(Node);
                member.SetAttribute("id", id.ToString());
                member.InnerText = Value;
                memberlist[memberlist.Count-1].AppendChild(member);
                xmlDoc.Save(XMLPATH);
            }
            else
            {
                
            }
            
        }

        /// <summary>
        /// 插入一个节点和子节点
        /// </summary>
        /// <param name="Node">子节点</param>
        /// <param name="Value">子节点值</param>
        /// <param name="id">id</param>
        /// <param name="MainNode">主节点</param>
        /// <param name="FirstNode">主节点</param>
        public static void InsertNode(string Node, string Value, int id, string MainNode, string FirstNode)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(XMLPATH))
            {
                xmlDoc.Load(XMLPATH);
                XmlNode memberlist = xmlDoc.SelectSingleNode(FirstNode);        // selectsinglenode只能获取最外的一层Node
                XmlElement member = xmlDoc.CreateElement(MainNode);
                XmlElement xmlele = xmlDoc.CreateElement(Node);
                xmlele.SetAttribute("id", id.ToString());
                xmlele.InnerText = Value; 
                member.AppendChild(xmlele);
                memberlist.AppendChild(member);
                xmlDoc.Save(XMLPATH);

            }
            else
            {

            }
        }

        /// <summary>
        /// 改变节点的值
        /// </summary>
        /// <param name="Node">节点</param>
        /// <param name="Value">节点值</param>
        /// <param name="MainNode">主节点</param>
        public static void ChangeNode(string Node, string Value, string MainNode)
        {
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(XMLPATH);
            XmlNodeList memberlist = xmlDoc.GetElementsByTagName(MainNode);
            XmlNodeList nodelist = memberlist[0].ChildNodes;
            foreach (XmlNode node in nodelist)
            {
                if (node.Name==Node)
                {
                    node.InnerText = Value;
                    break;
                }
            }
            xmlDoc.Save(XMLPATH);
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="Node"></param>
        public static void RemoveXml(string Node)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(XMLPATH);
            XmlNode node = xmlDoc.SelectSingleNode(Node);
            node.RemoveAll();
            xmlDoc.Save(XMLPATH);
        }
        /// <summary>
        /// 判断某个节点是否存在
        /// </summary>
        /// <param name="Node">节点名</param>
        /// <returns></returns>
        public static bool FindNode(string Node)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(XMLPATH);
                XmlNodeList node = xmldoc.GetElementsByTagName(Node);

                return node[0]!= null;
            }
            catch
            {
                return false;
            }
        }
    }
}
