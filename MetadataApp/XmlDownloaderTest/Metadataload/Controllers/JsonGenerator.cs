using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Metadataload.Models;

namespace Metadataload.Controllers
{
    public class JsonGenerator
    {
        public static void generateJSONMetadata(string metadataPath, string metadataFilePath)
        {
            const int MAX_DEEPNESS = 5;
            int deepness = 0;

            var elementLists = new List<IElement>[MAX_DEEPNESS];

            for (int i = 0; i < elementLists.Length; i++)
                elementLists[i] = new List<IElement>();

            var folders = new Folder[MAX_DEEPNESS];

            for (int i = 0; i < folders.Length; i++)
                folders[i] = new Folder();

            Schema schema = new Schema();

            string filePath = metadataPath;

            XmlReader xmlReader = XmlReader.Create(metadataFilePath);

            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        string name = xmlReader.GetAttribute("name");
                        string xPath = string.Format("//service[@name='{0}']", name);


                        if ((xmlReader.Name == "service_group") || (xmlReader.Name == "service") || (xmlReader.Name == "resource"))
                        {
                            folders[deepness].Title = name;

                            if (xmlReader.Name == "service")
                            {
                                folders[deepness].Description = xmlReader.GetAttribute("description");
                            }

                            folders[deepness].ExtraClasses = "service_ok";

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
                        else if (xmlReader.Name.Contains("_schema"))
                        {
                            schema.Title = name;
                            schema.HttpCode = Convert.ToInt32(xmlReader.GetAttribute("http_code"));

                            filePath += "\\" + name;

                            if (xmlReader.GetAttribute("status") == "error")
                            {
                                schema.ExtraClasses = "doc_error";
                                schema.HashCode = -1;
                                schema.Error = xmlReader.GetAttribute("error_message");
                            }
                            else
                            {
                                schema.ExtraClasses = "doc_ok";
                                schema.HashCode = Convert.ToInt32(xmlReader.GetAttribute("hashCode"));
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

                            folders[deepness].Children = elementLists[deepness + 1];

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

            string jsonFilePath = metadataPath + "version.json";
            File.WriteAllText(jsonFilePath, json);
        }
    }
}