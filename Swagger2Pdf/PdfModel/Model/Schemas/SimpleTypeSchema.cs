using System;
using System.Text;

namespace Swagger2Pdf.PdfModel.Model.Schemas
{
    public class SimpleTypeSchema : Schema
    {
        public SimpleTypeSchema(string type, string format, object example, string description)
        {
            Type = type;
            Format = format;
            object example_value = null;
            switch (type)
            {
                case "string": example_value = example;
                    break;
                case "integer": example_value = Convert.ToInt32(example);
                    break;
                case "number": example_value = Convert.ToDouble(example);
                    break;
                case "object": example_value = example;
                    break;
                case "boolean": example_value = Convert.ToBoolean(example);
                    break;
                default: example_value = example;
                    break;
            }
            Example = example_value ?? GetExampleValue(type, format);
            Description = description ?? string.Empty;
        }

        public static object ExampleToObject(string type, object example)
        {
            object example_value = null;
            if (string.IsNullOrEmpty(type))
                return example;
            switch (type)
            {
                case "string":
                    example_value = example;
                    break;
                case "integer":
                    example_value = Convert.ToInt32(example);
                    break;
                case "number":
                    example_value = Convert.ToDouble(example);
                    break;
                case "object":
                    example_value = example;
                    break;
                case "boolean":
                    example_value = Convert.ToBoolean(example);
                    break;
                default:
                    example_value = example;
                    break;
            }
            if (example_value == null)
            {
                example_value = string.Empty;
            }
            return example_value;
        }

        private static int CurrentIdentifier = 0;
        private static double CurrentDouble = 0.5;

        public string Type { get; }
        public string Format { get; }
        public object Example { get; }
        public string Description { get; }

        private static object GetExampleValue(string type, string format)
        {
            switch (type)
            {
                case "string":
                    switch (format)
                    {
                        case "byte": return Convert.ToBase64String(Encoding.Unicode.GetBytes("example byte-encoded value"));
                        case "binary": return "binary string";
                        case "date": return DateTime.Now.Date;
                        case "date-time": return DateTime.Now;
                        case "password": return "***********";
                        default: return "string";
                    }
                case "integer": return ++CurrentIdentifier;
                case "number": return CurrentDouble += 1;
                case "object": return new object();
                case "boolean": return true;
                default: return null;
            }
        }
    }
}