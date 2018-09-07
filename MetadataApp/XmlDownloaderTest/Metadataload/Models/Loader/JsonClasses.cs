using System.Collections.Generic;

namespace Metadataload.Models
{
    public interface IElement { }

    public class Schema : IElement
    {
        public string Title { get; set; }

        public string ExtraClasses { get; set; }

        public int HashCode { get; set; }

        public int HttpCode { get; set; }

        public string Error { get; set; }
    }

    public class Folder : IElement
    {
        public string Title { get; set; }

        public string ExtraClasses { get; set; }

        public string Description { get; set; }

        public IList<IElement> Children { get; set; }
    }
}