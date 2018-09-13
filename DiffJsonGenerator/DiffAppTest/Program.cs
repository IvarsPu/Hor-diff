using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace DiffAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string version = "515";
            string release = "21";


            string root = "C:/Projects/Hor-diff/DiffApp/rest/" + version + "/" + release + "/";

            /*
            string test1 = "C:/Projects/Hor-diff/DiffApp/rest/" + version + "/13/Virsgrāmata/TdmPDok/TdmPDok.wadl";
            XDocument xdoc1 = XDocument.Load(test1, LoadOptions.None);
            int hash1 = xdoc1.ToString().GetHashCode();

            string test2 = "C:/Projects/Hor-diff/DiffApp/rest/" + version + "/21/Virsgrāmata/TdmPDok/TdmPDok.wadl";
            XDocument xdoc2 = XDocument.Load(test2, LoadOptions.None);
            int hash2 = xdoc1.ToString().GetHashCode();

            Console.WriteLine(hash1);
            Console.WriteLine(hash2);
            Console.WriteLine(hash1 == hash2);
            */

            XmlDocument metadataXml = new XmlDocument();
            metadataXml.Load(root + "metadata.xml");

            UpdateSchemaHash(root, metadataXml, "//service_schema");
            UpdateSchemaHash(root, metadataXml, "//data_schema");
            UpdateSchemaHash(root, metadataXml, "//query_schema");

            metadataXml.Save(root + "metadata.xml");
            Console.WriteLine("Done");
        }

        private static void UpdateSchemaHash(string root, XmlDocument metadataXml, string nodeXpath)
        {
            foreach (XmlNode node in metadataXml.SelectNodes(nodeXpath))
            {

                if (node.Attributes["hashCode"].Value != "-1")
                {
                    String filePath = node.Attributes["name"].Value;
                    XmlNode fileNode = node.ParentNode;
                    do
                    {
                        filePath = fileNode.Attributes["name"].Value + "/" + filePath;
                        fileNode = fileNode.ParentNode;
                    } while (!fileNode.Name.Equals("rest_api_metadata"));

                    filePath = root + filePath;
                    XDocument xdoc = XDocument.Load(filePath, LoadOptions.None);

                    int hash = xdoc.ToString().GetHashCode();
                    int noNamespaceHash = GetNoNamspaceHash(filePath, xdoc);

                    node.Attributes["hashCode"].Value = hash.ToString();
                    node.Attributes["noNamspaceHashCode"].Value = noNamespaceHash.ToString();
                }

            }
        }

        private static int GetNoNamspaceHash(string fileName, XDocument xdoc)
        {
            int hash = -1;

            if (fileName.Contains(".wadl"))
            {
                CleanUpTag(xdoc, "application");
            }
            else if (fileName.Contains(".xsd"))
            {
                CleanUpTag(xdoc, "schema");
                RemoveTags(xdoc, "import");
            }
            else if (fileName.Contains(".xml"))
            {
                CleanUpTag(xdoc, "collection");
            }

            string result = xdoc.ToString();
            result = result.Replace(" ", "");
            result = result.Replace("\r\n", "");
            hash = result.GetHashCode();
            
            return hash;
        }

        private static void CleanUpTag(XDocument doc, string name)
        {
            List<XElement> clearNodes = new List<XElement>();

            // Replace element with clear copy
            clearNodes.AddRange(
                from el in doc.Descendants()
                where el.Name.LocalName == name
                select el);

            foreach (XElement elem in clearNodes)
            {
                var childs = elem.Descendants();
                XElement newElem = new XElement(elem.Name.LocalName);
                newElem.Add(childs);
                elem.ReplaceWith(newElem);
            }

            // Clean up remaining namespaces created during node replacemant
            doc.Descendants()
              .Attributes()
              .Where(x => x.IsNamespaceDeclaration)
              .Remove();

            foreach (var elem in doc.Descendants())
            {
                elem.Name = elem.Name.LocalName;
            }
        }

        private static void RemoveTags(XDocument doc, string name)
        {
            doc.Descendants()
              .Elements()
              .Where(x => x.Name.LocalName == name)
              .Remove();
        }

        private static string GetSchema(string filename)
        {
            int i = filename.LastIndexOf(".");
            string extension = filename.Substring(i);
            string schema = "other_schema";

            if (extension == ".xsd")
            {
                schema = "data_schema";
            }
            else if (extension == ".wadl")
            {
                schema = "service_schema";
            }
            else if (filename == "query.xml")
            {
                schema = "query_schema";
            }

            return schema;
        }


    }
}
