using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace DiffAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string versonRelease = "515.3"; // [version.release]
            const int MAX_DEEPNESS = 5;
            int deepness = 0;

            var elementLists = new List<IElement>[MAX_DEEPNESS];

            for (int i = 0; i < elementLists.Length; i++)
                elementLists[i] = new List<IElement>();

            var folders = new Folder[MAX_DEEPNESS];

            for (int i = 0; i < folders.Length; i++)
                folders[i] = new Folder();

            Schema schema = new Schema();

            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\" + versonRelease;

            XmlReader xmlReader = XmlReader.Create(filePath + "\\metadata.xml");

            while(xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        string name = xmlReader.GetAttribute("name");
                        string xPath = string.Format("//service[@name='{0}']", name);


                        if ((xmlReader.Name == "service_group") || (xmlReader.Name == "service") || (xmlReader.Name == "resource"))
                        {
                            folders[deepness].title = name;

                            folders[deepness].extraClasses = "service_ok";

                            if (!xmlReader.IsEmptyElement)
                            {
                                filePath += "\\" + name;
                                deepness++;
                            }
                            else
                            {
                                elementLists[deepness].Add(folders[deepness]);

                                folders[deepness] = new Folder();
                            }
                        }
                        else if ((xmlReader.Name == "service_schema")|| (xmlReader.Name == "data_schema"))
                        {
                            schema.title = name;

                            filePath += "\\" + name;
                            Console.WriteLine(filePath);

                            if (xmlReader.GetAttribute("status") == "error")
                            {
                                schema.extraClasses = "doc_error";

                                schema.hashCode = -1;
                            }
                            else
                            {
                                schema.extraClasses = "doc_ok";

                                XmlDocument xmlDoc = new XmlDocument();

                                xmlDoc.Load(filePath);

                                string xmlContents = xmlDoc.InnerXml;

                                int hashCode = xmlContents.GetHashCode();

                                schema.hashCode = hashCode;
                            }

                            int removeId = filePath.LastIndexOf("\\");
                            filePath = filePath.Remove(removeId);

                            elementLists[deepness].Add(schema);
                            schema = new Schema();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if ((xmlReader.Name == "service_group") || (xmlReader.Name == "service") || (xmlReader.Name == "resource"))
                        {
                            deepness--;

                            folders[deepness].children = elementLists[deepness + 1];

                            elementLists[deepness].Add(folders[deepness]);

                            elementLists[deepness + 1] = new List<IElement>();
                            folders[deepness] = new Folder();
                        }

                        int removeFrom = filePath.LastIndexOf("\\");
                        filePath = filePath.Remove(removeFrom);
                        break;
                }
            }

            string json = JsonConvert.SerializeObject(elementLists[0]);
            //json = string.Format("var ontJson = {0}; $(function(){{$(\"#tree\").fancytree({{source: ontJson}});}});", json);//var ontJson = {0}; $(function(){$(\"#tree\").fancytree({source: ontJson});});
            
            string jsonFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\" + versonRelease +"\\mockup_tree_data.json";
            File.WriteAllText(jsonFilePath, json);

            Console.WriteLine("Complete!");
            Console.ReadKey();
        }
    }
}
