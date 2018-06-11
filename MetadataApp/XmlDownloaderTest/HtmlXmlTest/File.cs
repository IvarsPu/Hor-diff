namespace HtmlXmlTest
{
    internal class File
    {
        public File(string name, string filename)
        {
            this.Set(name, filename, false);
        }

        public File(string name, string filename, bool attachment)
        {
            this.Set(name, filename, attachment);
        }

        public string Name { get; set; }

        public string Filename { get; set; }

        public bool Attachment { get; set; }

        private void Set(string name, string filename, bool attachment)
        {
            this.Name = name;
            this.Filename = filename;
            this.Attachment = attachment;
        }
    }
}
