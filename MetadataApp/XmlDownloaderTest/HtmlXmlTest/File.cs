namespace HtmlXmlTest
{
    internal class File
    {
        public File(string name, string filename, bool attachment = false, string errorMSG = null)
        {
            this.Set(name, filename, attachment, errorMSG);
        }

        public string Name { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        public string ErrorMSG { get; set; }

        private void Set(string name, string filename, bool attachment, string errorMSG)
        {
            this.Name = name;
            this.Filename = filename;
            this.Attachment = attachment;
            this.ErrorMSG = errorMSG;
        }
    }
}
