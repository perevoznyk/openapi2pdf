using Newtonsoft.Json;
using Swagger2Pdf.PdfModel.Model;
using Swagger2Pdf.PdfModel.Model.Schemas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swagger2Pdf.Model.Properties
{
    public class ObjectProperty : PropertyBase
    {
        [JsonProperty("properties")]
        public Dictionary<string, PropertyBase> Properties { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("required")]
        public IList<string> Required { get; set; }

        public override Schema ResolveSchema(SchemaResolutionContext resolutionContext)
        {
            var complexTypeSchema = new ComplexTypeSchema();
            if (Properties != null)
            {
                foreach (var property in Properties)
                {
                    complexTypeSchema.AddProperty(property.Key, property.Value?.ResolveSchema(resolutionContext));
                }
            }
            return complexTypeSchema;
        }
    }
}
