using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
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
            new Program(515,3,520,1);
            DiffColorer(515,3,520,1);
        }

        private static void DiffColorer(int firstVersion, int firstRelease, int secondVersion, int secondRelease)
        {
            StringBuilder sb = new StringBuilder();

            string oldText = File.ReadAllText("MetadataLocalFolder/" + firstVersion + "/" + firstRelease + "/metadata.xml");
            string newText = File.ReadAllText("MetadataLocalFolder/" + secondVersion + "/" + secondRelease + "/metadata.xml");

            var d = new Differ();
            var builder = new InlineDiffBuilder(d);
            var result = builder.BuildDiffModel(oldText, newText);

            foreach (var line in result.Lines)
            {
                if (line.Type == ChangeType.Inserted)
                {
                    sb.Append("<font color='GREEN'>");
                }
                else if (line.Type == ChangeType.Deleted)
                {
                    sb.Append("<font color='RED'>");
                }
                else if (line.Type == ChangeType.Modified)
                {
                    sb.Append("<font color='YELLOW'>");
                }
                else if (line.Type == ChangeType.Imaginary)
                {
                    sb.Append("<font color='FUCHSIA'>");
                }
                else if (line.Type == ChangeType.Unchanged)
                {
                    sb.Append("<font>");
                }

                sb.Append(line.Text + "</font><br/>");
            }

            File.WriteAllText("C:/Users/ralfs.zangis/Desktop/testColored.txt", sb.ToString());
        }

        public Program(int firstVersion, int firstRelease, int secondVersion, int secondRelease)
        {
            XmlDocument firstXml = new XmlDocument();
            firstXml.Load("MetadataLocalFolder/" + firstVersion + "/" + firstRelease + "/metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondXml.Load("MetadataLocalFolder/" + secondVersion + "/" + secondRelease + "/metadata.xml");

            secondXml = Compare(firstXml, secondXml, secondRelease);
            
            secondXml.RemoveChild(secondXml.FirstChild);
            
            string json = JsonConvert.SerializeObject(AddClass(secondXml));

            File.WriteAllText("C:/Users/ralfs.zangis/Desktop/test.txt",json);

            Console.WriteLine("Done");
        }

        #region fill objects
        private Folder AddClass(XmlDocument xml)
        {
            Folder folder = new Folder();
            foreach (XmlNode node in xml.SelectNodes("//rest_api_metadata"))
            {
                folder.title = "root";
                folder.type = node.Name;
                folder.children = GetChildren(node);
            }
            return folder;
        }

        private IList<IElement> GetChildren(XmlNode node)
        {
            IList<IElement> elements = new List<IElement>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name.Equals("service") || child.Name.Equals("service_group") || child.Name.Equals("resource"))
                {
                    Folder folder = new Folder();
                    folder.title = child.Attributes["name"].Value;
                    folder.type = child.Name;
                    folder.children = GetChildren(child);
                    elements.Add(folder);
                }
                else
                {
                    Schema schema = new Schema();
                    schema.title = child.Attributes["name"].Value;
                    schema.hashCode = Int32.Parse(child.Attributes["hashCode"].Value);
                    schema.noNamspaceHashCode = Int32.Parse(child.Attributes["noNamspaceHashCode"].Value);
                    schema.type = child.Name;
                    try
                    {
                        schema.errorMessage = child.Attributes["error_message"].Value;
                    }
                    catch
                    {

                    }
                    elements.Add(schema);
                }
            }
            return elements;
        }
        #endregion

        #region compare
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
        #endregion
    }
}
