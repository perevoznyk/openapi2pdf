﻿using System;
using System.Text;

namespace Swagger2Pdf.PdfModel.Model
{
    public class Parameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deprecated { get; set; }
        public bool AllowEmptyValue { get; set; }
        public string Title { get; set; }
        public string MultipleOf { get; set; }

        public string Pattern { get; set; }

        public string Maximum { get; set; }
        public string Minimum { get; set; }

        public string ExclusiveMinimum { get; set; }
        public string ExclusiveMaximum { get; set; }

        public string MinLength { get; set; }
        public string MaxLength { get; set; }

        public string MaxItems { get; set; }
        public string MinItems { get; set; }

        public string MaxProperties { get; set; }
        public string MinProperties { get; set; }

        public bool UniqueItems { get; set; }

        public bool IsRequired { get; set; }
        public string Enum { get; set; }
        public Schema Schema { get; set; }
        public string Type { get; set; }
        public string Ref { get; set; }

        public string GetMinMaxString()
        {
            return GetTwoPartString("Minimum", Minimum, "maximum", Maximum);
        }

        public string GetExclusiveMinMaxString()
        {
            return GetTwoPartString("Exclusive minimum", ExclusiveMinimum, "exclusive maximum", ExclusiveMaximum);
        }

        public string GetMinMaxItems()
        {
            return GetTwoPartString("Minimum items", MinItems, "maximum items", MaxItems);
        }

        public string GetMinMaxProperties()
        {
            return GetTwoPartString("Minimum properties", MinProperties, "maximum properties", MaxProperties);
        }

        private static string GetTwoPartString(string leftPartLabel, string leftPart, string rightPartLabel, string rightPart)
        {
            if (leftPart.IsNullOrEmpty() && rightPart.IsNullOrEmpty())
            {
                return null;
            }

            if (!leftPart.IsNullOrEmpty() && !rightPart.IsNullOrEmpty())
            {
                return $"{leftPartLabel.FirstCharToUpper()}: {leftPart}, {rightPartLabel.ToLower()}: {rightPart}";
            }

            if (!leftPart.IsNullOrEmpty())
            {
                return $"{leftPartLabel.FirstCharToUpper()}: {leftPart}";
            }

            if (!rightPart.IsNullOrEmpty())
            {
                return $"{rightPartLabel.FirstCharToUpper()}: {rightPart}";
            }

            return null;
        }
    }
}