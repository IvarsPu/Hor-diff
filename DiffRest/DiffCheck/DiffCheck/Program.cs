using System;
using System.IO;
using System.Xml;

namespace DiffCheck
{
    public class Program
    {
        private string firstfilePath, secondFilePath;

        static void Main(string[] args)
        {
            new Program(515,3,520,2);
        }

        public Program(int firstVersion, int firstRelease, int secondVersion, int secondRelease)
        {
            XmlDocument firstXml = new XmlDocument();
            firstfilePath = "MetadataLocalFolder/"+ firstVersion + "/"+ firstRelease + "/";
            firstXml.Load(firstfilePath + "metadata.xml");

            XmlDocument secondXml = new XmlDocument();
            secondFilePath = "MetadataLocalFolder/"+ secondVersion + "/"+ secondRelease + "/";
            secondXml.Load(secondFilePath + "metadata.xml");

            //Compare(firstXml, secondXml, secondRelease);

            Console.WriteLine("Done");
        }

        private void Compare(XmlDocument firstXml, XmlDocument secondXml, int release)
        {
            //compares and removes not changed from second file
            foreach (XmlNode node in firstXml.SelectNodes("//service/*[count(child::*) = 0]"))
            {
                foreach(XmlNode child in secondXml.SelectNodes("//" + node.Name + "[@name='" + node.Attributes["name"].Value + "' and @hashCode = '" + node.Attributes["hashCode"].Value + "']"))
                {
                    RemoveFile(child);
                    child.ParentNode.RemoveChild(child);
                }
            }

            //removes the ones that are stored in previous release
            foreach (XmlNode node in secondXml.SelectNodes("//service/*[count(child::*) = 0][not(@stored_release='" + release + "')]"))
            {
                RemoveFile(node);
            }
            
            secondXml.Save("C:/Users/ralfs.zangis/Desktop/metadata.xml");//overwrites the second document
        }

        //removes the file
        private void RemoveFile(XmlNode node)
        {
            string path = node.Attributes["name"].Value;
            while(!node.Name.Equals("service_group"))
            {
                node = node.ParentNode;
                path = node.Attributes["name"].Value + "/" + path;
            }
            path = secondFilePath + path + ".xml";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
