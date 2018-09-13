﻿using System;
using System.Collections.Generic;

namespace DiffCheck
{
    public interface IElement { }

    public class Schema : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; }//??

        public int hashCode { get; set; }

        public int noNamspaceHashCode { get; set; }

        public string type { get; set; }

        public String diffHtmlFile { get; set; }//ill do it

        public int httpCode { get; set; }//ill do it

        public String errorMessage { get; set; }
    }

    public class Folder : IElement
    {
        public string title { get; set; }

        public string extraClasses { get; set; }//??

        public string type { get; set; }

        public IList<IElement> children { get; set; }
    }
}
