namespace HtmlXmlTest
{
    internal class File
    {
        public File(string name, string filename, bool attachment = false, string errorMSG = null)
        {
            this.Name = name;
            this.Filename = filename;
            this.Attachment = attachment;
            this.ErrorMSG = errorMSG;
        }

        public string Name { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        public string ErrorMSG { get; set; }
    }
}
