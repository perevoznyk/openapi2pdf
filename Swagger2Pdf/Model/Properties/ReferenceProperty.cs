using System;
using System.Linq;
using Newtonsoft.Json;
using Swagger2Pdf.PdfModel.Model;
using Swagger2Pdf.PdfModel.Model.Schemas;

namespace Swagger2Pdf.Model.Properties
{
    public sealed class ReferenceProperty : PropertyBase
    {

        public string Ref { get; set; }

        public override string GetReference()
        {
            return Ref.Split('/').Last();
        }

        public override Schema ResolveSchema(SchemaResolutionContext resolutionContext)
        {
            var complexTypeSchema = new ComplexTypeSchema();
            complexTypeSchema.Ref = Ref;
            var definition = resolutionContext.ReferenceResolver.ResolveReference(Ref);

            if (definition == null)
            {
                throw new ArgumentException($"Unable to resolve definition for reference code: {Ref}");
            }

            if (string.IsNullOrEmpty(Description))
                Description = definition.Description;

            complexTypeSchema.Required = definition.Required;

            if (definition.Properties != null)
            {
                foreach (var property in definition.Properties)
                {
                    complexTypeSchema.AddProperty(property.Key, property.Value?.ResolveSchema(resolutionContext));
                }
            }
            else
            {
                SimpleTypeSchema simple = new SimpleTypeSchema(definition.Type, null, definition.Example, definition.Description);
                return simple;
            }

            return complexTypeSchema;
        }
    }

    public class SchemaResolutionContext
    {
        public ReferenceResolver.ReferenceResolver ReferenceResolver { get; }

        public SchemaResolutionContext(ReferenceResolver.ReferenceResolver referenceResolver)
        {
            ReferenceResolver = referenceResolver;
        }
    }
}