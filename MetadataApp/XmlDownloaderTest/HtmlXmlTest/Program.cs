namespace HtmlXmlTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Program prog = new Program();
            prog.LoadRestServiceMetadata();

            Console.ReadKey();
        }

        public async void LoadRestServiceMetadata()
        {

            string rootUrl = ConfigurationManager.ConnectionStrings["Server"].ConnectionString;
            string rootLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\rest\\";
            string metadataPath = null;
            List<XmlFile> attachmentData = new List<XmlFile>();

            try
            {
                Logger.LogInfo("Loading REST service structure");
                List<RestService> services = WebResourceReader.LoadRestServices(rootUrl, ref rootLocalPath, ref metadataPath);

                Logger.LogInfo("Loading REST service metadata");
                WebResourceLoader webResourceLoader = new WebResourceLoader(rootUrl);
                await webResourceLoader.LoadServiceMetadata(services, metadataPath);

                Logger.LogInfo("Generating json tree data");
                JsonGenerator.generateJSONMetadata(rootLocalPath, metadataPath);
                Console.WriteLine("Work complete!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

 

    }
}
