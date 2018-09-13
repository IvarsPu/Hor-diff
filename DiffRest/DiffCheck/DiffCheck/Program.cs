using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;

namespace DiffCheck
{
    public class Program
    {
        static void Main(string[] args)
        {
            //new Program(515,3,520,1);
            DiffColorer();
        }

        private static void DiffColorer()
        {
            StringBuilder sb = new StringBuilder();

            string oldText = File.ReadAllText("MetadataLocalFolder/515/3/metadata.xml");
            string newText = File.ReadAllText("MetadataLocalFolder/520/1/metadata.xml");

            var d = new Differ();
            var builder = new InlineDiffBuilder(d);
            var result = builder.BuildDiffModel(oldText, newText);

            foreach (var line in result.Lines)
            {
                if (line.Type == ChangeType.Inserted)
                {
                    sb.Append("<font color='green'>");
                }
                else if (line.Type == ChangeType.Deleted)
                {
                    sb.Append("<font color='red'>");
                }
                else if (line.Type == ChangeType.Modified)
                {
                    sb.Append("* ");
                }
                else if (line.Type == ChangeType.Imaginary)
                {
                    sb.Append("? ");
                }
                else if (line.Type == ChangeType.Unchanged)
                {
                    sb.Append("  ");
                }

                sb.Append(line.Text + "<br/>");
            }

            File.WriteAllText("C:/Users/ralfs.zangis/Desktop/test.html", sb.ToString());
        }

        public Program(int firstVersion, int firstRelease, int secondVersion, int secondRelease)
        {
            XmlDocument firstXml = new XmlDocument();
            firstXml.Load("MetadataLocalFolder/" + firstVersion + "/" + firstRelease + "/metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondXml.Load("MetadataLocalFolder/" + secondVersion + "/" + secondRelease + "/metadata.xml");

            secondXml = Compare(firstXml, secondXml, secondRelease);

            //removes all atributes except name
            foreach (XmlNode node in secondXml.SelectNodes("//*"))
            {
                var x = node.Attributes["name"];
                node.Attributes.RemoveAll();
                if (x != null)
                {
                    node.Attributes.Append(x);
                }
            }

            secondXml.RemoveChild(secondXml.FirstChild);

            //secondXml.Save("C:/Users/ralfs.zangis/Desktop/test.xml");

            string json = JsonConvert.SerializeXmlNode(secondXml);

            File.WriteAllText("C:/Users/ralfs.zangis/Desktop/test.txt",json);

            Console.WriteLine("Done");
        }

        private XmlDocument Compare(XmlDocument firstXml, XmlDocument secondXml, int release)
        {
            foreach (XmlNode node in firstXml.SelectNodes("//service/*[count(child::*) = 0]"))
            {
                XmlNode child = secondXml.SelectSingleNode("//" + node.Name + "[@name='" + node.Attributes["name"].Value + "']");
                if (child != null)
                {
                    if (child.Attributes["hashCode"].Value.Equals(node.Attributes["hashCode"].Value))
                    {
                        //not changed
                        child.ParentNode.RemoveChild(child);
                    }
                }
                else
                {
                    //removed
                    XmlNode t = Get(node, secondXml);
                    child = secondXml.SelectSingleNode("//" + t.ParentNode.Name + "[@name='" + t.ParentNode.Attributes["name"].Value + "']");

                    XmlNode newBook = secondXml.ImportNode(t, true);
                    child.AppendChild(newBook);
                }
            }
            return secondXml;
        }

        private XmlNode Get(XmlNode node, XmlDocument xml)
        {
            XmlNode child = xml.SelectSingleNode("//" + node.ParentNode.Name + "[@name='" + node.ParentNode.Attributes["name"].Value + "']");
            if (child == null)
            {
                child = Get(node, xml);
            }

            return node;
        }

        //removes the file

        //XmlAttribute Status = secondXml.CreateAttribute("status");
        //Status.Value = "removed";
        //newBook.Attributes.SetNamedItem(Status);

        //private void RemoveFile(XmlNode node)
        //{
        //    string path = node.Attributes["name"].Value;
        //    while(!node.Name.Equals("service_group"))
        //    {
        //        node = node.ParentNode;
        //        path = node.Attributes["name"].Value + "/" + path;
        //    }
        //    path = secondFilePath + path + ".xml";

        //    if (File.Exists(path))
        //    {
        //        File.Delete(path);
        //    }
        //}

        //var xml1 = XDocument.Load(firstfilePath + "metadata.xml");
        //var xml2 = XDocument.Load(secondFilePath + "metadata.xml");

        ////Combine and remove duplicates
        //var combinedUnique = xml1.Descendants("service_group").Union(xml2.Descendants("service_group"));

        //XDocument xmlDoc = new XDocument();
        //XElement element = new XElement("root");
        //xmlDoc.Add(element);
        //foreach (XElement i in combinedUnique)
        //{
        //    element.Add(i);
        //}

        //File.WriteAllText("C:/Users/ralfs.zangis/Desktop/test.xml", xmlDoc.ToString());
    }
}
