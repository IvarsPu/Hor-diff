﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using System.Web;

namespace DiffCheck
{
    public class Program
    {
        private static string pathToSave = "C:/Users/ralfs.zangis/Desktop/";
        private static string folderName = "MetadataLocalFolder/";

        static void Main(string[] args)
        {
            new Program(515, 3, 520, 1);
            DiffColorer("515/3/", "520/1/", "metadata.xml");

            Console.WriteLine("Done");
        }

        #region Text Color Generator
        private static void DiffColorer(string firstFile, string secondFile, string file)
        {
            StringBuilder sb = new StringBuilder();

            string oldText = "";
            if (File.Exists(folderName + firstFile + file))
            {
                oldText = File.ReadAllText(folderName + firstFile + file);
            }

            string newText = "";
            if (File.Exists(folderName + secondFile + file))
            {
                newText = File.ReadAllText(folderName + secondFile + file);
            }

            var d = new Differ();
            var builder = new InlineDiffBuilder(d);
            var result = builder.BuildDiffModel(oldText, newText);
            sb.Append("<html>"+
                "<head>" +
                "<style>" +
                "span { color: gray; }" +
                ".deleted { color:black; background-color:red; } " +
                ".new { color:black; background-color:yellow; } " +
                "</style>" +
                "</head>"+
                "<body>");
            foreach (var line in result.Lines)
            {
                if (line.Type == ChangeType.Inserted)
                {
                    sb.Append("<span class='new'>");
                }
                else if (line.Type == ChangeType.Deleted)
                {
                    sb.Append("<span class='deleted'>");
                }
                else if (line.Type == ChangeType.Unchanged)
                {
                    sb.Append("<span>");
                }
                sb.Append(HttpUtility.HtmlEncode(line.Text) + "</span><br/>");
            }
            sb.Append("</body>"+
                "</html>");
            File.WriteAllText(pathToSave + "testColored.html", sb.ToString(), Encoding.UTF8);
        }
        #endregion

        #region Make Json tree
        public Program(int firstVersion, int firstRelease, int secondVersion, int secondRelease)
        {
            XmlDocument firstXml = new XmlDocument();
            firstXml.Load(folderName + firstVersion + "/" + firstRelease + "/metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondXml.Load(folderName + secondVersion + "/" + secondRelease + "/metadata.xml");

            secondXml = Compare(firstXml, secondXml, secondRelease);
            secondXml.RemoveChild(secondXml.FirstChild);
            
            string json = JsonConvert.SerializeObject(AddClass(secondXml));

            File.WriteAllText(pathToSave+"test.txt",json);
        }
        
        #region fill objects
        private Folder AddClass(XmlDocument xml)
        {
            Folder folder = new Folder();
            XmlNode node = xml.SelectSingleNode("//rest_api_metadata");
            folder.title = "root";
            folder.type = node.Name;
            folder.children = GetChildren(node);

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
                    schema.diffHtmlFile = GetPath(child);
                    elements.Add(schema);
                }
            }
            return elements;
        }

        private string GetPath(XmlNode node)
        {
            string path = node.Attributes["name"].Value;
            while (!node.Name.Equals("service_group"))
            {
                node = node.ParentNode;
                path = node.Attributes["name"].Value + "/" + path;
            }
            return path = folderName + path + ".xml";
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
        #endregion
    }
}
