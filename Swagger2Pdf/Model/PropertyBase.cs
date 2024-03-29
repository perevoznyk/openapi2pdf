﻿using Newtonsoft.Json;
using Swagger2Pdf.Model.Properties;
using Swagger2Pdf.PdfModel.Model;

namespace Swagger2Pdf.Model
{
    public abstract class PropertyBase
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("example")]
        public object Example { get; set; }

        public abstract Schema ResolveSchema(SchemaResolutionContext resolutionContext);

        public virtual string GetReference()
        {
            return string.Empty;
        }
    }
}