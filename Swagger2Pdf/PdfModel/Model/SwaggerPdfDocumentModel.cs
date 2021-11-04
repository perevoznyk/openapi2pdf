using Swagger2Pdf.Model;
using Swagger2Pdf.Model.ReferenceResolver;
using System;
using System.Collections.Generic;

namespace Swagger2Pdf.PdfModel.Model
{
    public class SwaggerPdfDocumentModel
    {
        public List<EndpointInfo> DocumentationEntries { get; set; }
        public string PdfDocumentPath { get; set; }
        public string WelcomePageImage { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Email { get; set; }
        public DateTime DocumentDate { get; set; }
        public Dictionary<string, AuthorizationInfo> AuthorizationInfo { get; set; }
        public string CustomPageName { get; set; }
        public Tag[] Tags { get; set; }
        public SortedDictionary<string, Definition> Definitions { get; set; }
        public ReferenceResolver ReferenceResolver { get; set; }
        public string Security { get; set; }
        public string BasePath { get; set; }
        public string Host { get; set; }
        public List<ItemsTag> ItemsTags { get; set; }
        public int ModelIndex { get; set; }
    }
}
