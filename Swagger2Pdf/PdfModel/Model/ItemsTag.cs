using System;
using System.Collections.Generic;
using System.Text;

namespace Swagger2Pdf.PdfModel.Model
{
    public class ItemsTag
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public List<EndpointInfo> Items { get; set; }
        public ItemsTag()
        {
            Items = new List<EndpointInfo>();
        }
    }
}
