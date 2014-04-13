using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace InsertAnalysisCode
{
    public class Document
    {
        public XDocument XML { get; set; }
        public string Path { get; set; }

        public Document(string path)
        {
            this.Path = path;
        }

    }
}
