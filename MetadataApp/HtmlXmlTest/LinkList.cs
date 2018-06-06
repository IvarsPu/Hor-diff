namespace HtmlXmlTest
{
    using System.Collections;
    using System.Collections.Generic;

    class LinkList : IEnumerable<Link>
    {
        public List<Link> Links { get; set; }

        public IEnumerator<Link> GetEnumerator()
        {
            return ((IEnumerable<Link>)Links).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Link>)Links).GetEnumerator();
        }
    }
}
