﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using log4net;
using Swagger2Pdf.Model;
using Swagger2Pdf.Model.Properties;
using Swagger2Pdf.PdfModel;
using Swagger2Pdf.PdfModel.Model;
using Swagger2Pdf.PdfModel.Model.Schemas;

namespace Swagger2Pdf.HtmlDocumentBuilder
{
    public class HtmlPdfBuilder : PdfBuilderBase
    {
        private readonly HtmlDocumentBuilder _document = new HtmlDocumentBuilder();

        public HtmlPdfBuilder() : base(LogManager.GetLogger(typeof(Program)))
        {
        }

        protected override void WriteCustomPage(StringWriter writer)
        {
            _document.AddCustomPage(writer.GetStringBuilder());
        }

        protected override void DrawResponses(List<Response> docEntryResponses)
        {
            if (!docEntryResponses.Any()) return;

            _document.P();
            _document.H2().SetText("Responses");

            foreach (var response in docEntryResponses)
            {
                var p = _document.P();
                p.AddChildElement(HtmlElement.Label().SetText($"{response.Code}:").SetStyle("font-weight", "bold").SetStyle("color", "#005b96"));
                p.AddChildElement(HtmlElement.Label().SetText($" {response.Description}"));


                if (response.Schema != null)
                {
                    var responseBody = PdfModelJsonConverter.SerializeObject(response.Schema);
                    p.AddChildElement(HtmlElement.Pre().SetText(responseBody));
                }

                if (!string.IsNullOrEmpty(response.Ref))
                {
                    var link = new Link().Href("#" + response.Ref);
                    link.SetText("Reference: <b>" + response.Ref + "</b>");

                    _document.P().AddChildElement(link).SetStyle("font-size", "12px");

                    //_document.P().SetText($"Reference: {response.Ref}").SetStyle("font-size", "12px");

                    var definition = DocumentModel.ReferenceResolver.ResolveReference(response.Ref);
                    if (definition != null)
                    {
                        if (definition.Properties != null)
                        {
                            var table = _document.Table();
                            table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
                            foreach (var property in definition.Properties)
                            {

                                DrawTableRow(table, property, "", 0);
                                if (property.Value is ReferenceProperty)
                                {
                                    var referrenceproperty = (property.Value as ReferenceProperty);
                                    var subDefinition = DocumentModel.ReferenceResolver.ResolveReference(referrenceproperty.Ref);
                                    if (subDefinition != null)
                                    {
                                        foreach (var subitem in subDefinition.Properties)
                                        {
                                            DrawTableRow(table, subitem, property.Key, 0);
                                        }
                                    }
                                }
                                if (property.Value is ObjectProperty)
                                {
                                    var objectProperty = (property.Value as ObjectProperty);
                                    DrawObjectProperty(table, property.Key, objectProperty, 0);

                                }

                            }
                        }
                    } //definition
                }
            }
        }

        protected override void DrawBodyParameters(List<Parameter> docEntryBodyParameters)
        {
            if (!docEntryBodyParameters.Any()) return;

            _document.P();
            _document.H2().SetText("Request body parameters");

            foreach (var bodyParameter in docEntryBodyParameters)
            {
                var tableParam = _document.Table();
                tableParam.AddColumns(new TextContent("NAME"), new TextContent("DESCRIPTION"), new TextContent("REFERENCE"));
                var nameCell = HtmlElement.Label().SetText(bodyParameter.Name ?? "");
                if (bodyParameter.IsRequired)
                {
                    nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
                }
                var typeCell = HtmlElement.Label().SetText("");
                if (!string.IsNullOrEmpty(bodyParameter.Ref))
                {
                    var link = new Link().Href("#" + bodyParameter.Ref);
                    link.SetText("Reference: <b>" + bodyParameter.Ref + "</b>");

                    typeCell.AddChildElement(link);
                }

                var descriptionCell = new TextContent(bodyParameter.Description ?? "");

                tableParam.AddRow(nameCell, descriptionCell, typeCell);



                var schema = PdfModelJsonConverter.SerializeObject(bodyParameter.Schema);
                _document.Pre().SetText(schema);

                if (bodyParameter.Schema is ComplexTypeSchema)
                {
                    var properties = (bodyParameter.Schema as ComplexTypeSchema).Properties;
                    if (properties != null)
                    {
                        var table = _document.Table();
                        table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
                        foreach (var property in properties)
                        {
                            bool required = false;
                            if ((bodyParameter.Schema as ComplexTypeSchema).Required != null)
                            {
                                required = (bodyParameter.Schema as ComplexTypeSchema).Required.Contains(property.Key);
                            }

                            DrawTableRow(table, property, "", 0, required);
                            if (property.Value is ComplexTypeSchema)
                            {
                                var referrenceproperty = (property.Value as ComplexTypeSchema);

                                if (referrenceproperty != null)
                                {
                                    foreach (var subitem in referrenceproperty.Properties)
                                    {
                                        required = false;
                                        if (referrenceproperty.Required != null)
                                        {
                                            required = referrenceproperty.Required.Contains(subitem.Key);
                                        }
                                        DrawTableRow(table, subitem, property.Key, 1, required);
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(bodyParameter.Ref))
                    {
                        var definition = DocumentModel.ReferenceResolver.ResolveReference(bodyParameter.Ref);
                        if (definition != null)
                        {
                            if (definition.Properties != null)
                            {
                                var table = _document.Table();
                                table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
                                foreach (var property in definition.Properties)
                                {
                                    bool required = false;
                                    if (definition.Required != null)
                                    {
                                        required = definition.Required.Contains(property.Key);
                                    }

                                    DrawTableRow(table, property, "", 0, required);
                                    if (property.Value is ReferenceProperty)
                                    {
                                        var referrenceproperty = (property.Value as ReferenceProperty);
                                        var subDefinition = DocumentModel.ReferenceResolver.ResolveReference(referrenceproperty.Ref);
                                        if (subDefinition != null)
                                        {
                                            foreach (var subitem in subDefinition.Properties)
                                            {
                                                required = false;
                                                if (subDefinition.Required != null)
                                                {
                                                    required = subDefinition.Required.Contains(subitem.Key);
                                                }
                                                DrawTableRow(table, subitem, property.Key, 0, required);
                                            }
                                        }
                                    }
                                    if (property.Value is ObjectProperty)
                                    {
                                        var objectProperty = (property.Value as ObjectProperty);
                                        DrawObjectProperty(table, property.Key, objectProperty, 0);

                                    }

                                }
                            }
                        } //definition
                    } //REF
                }
            }


        }

        protected override void DrawFormDataParameters(List<Parameter> docEntryFormDataParameters)
        {
            if (!docEntryFormDataParameters.Any()) return;

            _document.P();
            _document.H2().SetText("Form data parameters");
            var table = _document.Table();
            table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
            foreach (var parameter in docEntryFormDataParameters)
            {
                var nameCell = HtmlElement.Label().SetText(parameter.Name ?? "");
                if (parameter.IsRequired)
                {
                    nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
                }
                var typeCell = new TextContent(parameter.Type ?? "");
                var descriptionCell = new TextContent(parameter.Description ?? "");
                //WriteDetailedDescription(descriptionCell, parameter);
                table.AddRow(nameCell, typeCell, descriptionCell);
            }
        }

        protected override void DrawQueryParameters(List<Parameter> docEntryQueryParameter)
        {
            if (!docEntryQueryParameter.Any()) return;

            _document.P();
            _document.H2().SetText("Query string parameters");
            var table = _document.Table();
            table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
            foreach (var parameter in docEntryQueryParameter)
            {
                var nameCell = HtmlElement.Label().SetText(parameter.Name ?? "");
                if (parameter.IsRequired)
                {
                    nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
                }
                var typeCell = new TextContent(parameter.Type ?? "");

                var descriptionCell = new TextContent(parameter.Description ?? "");
                //WriteDetailedDescription(descriptionCell, parameter);
                table.AddRow(nameCell, typeCell, descriptionCell);
            }
        }

        protected override void DrawHeaderParameters(List<Parameter> docEntryHeaderParameter)
        {
            if (!docEntryHeaderParameter.Any()) return;

            _document.P();
            _document.H2().SetText("HTTP header parameters");
            var table = _document.Table();
            table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
            foreach (var parameter in docEntryHeaderParameter)
            {
                var nameCell = HtmlElement.Label().SetText(parameter.Name ?? "");
                if (parameter.IsRequired)
                {
                    nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
                }
                var typeCell = new TextContent(parameter.Type ?? "");

                var descriptionCell = new TextContent(parameter.Description ?? "");
                //WriteDetailedDescription(descriptionCell, parameter);
                table.AddRow(nameCell, typeCell, descriptionCell);
            }
        }

        protected override void DrawPathParameters(List<Parameter> docEntryPathParameters)
        {
            if (!docEntryPathParameters.Any()) return;

            _document.P();
            _document.H2().SetText("Path parameters");
            var table = _document.Table();
            table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
            foreach (var parameter in docEntryPathParameters)
            {
                var nameCell = HtmlElement.Label().SetText(parameter.Name ?? "");
                if (parameter.IsRequired)
                {
                    nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
                }
                var typeCell = new TextContent(parameter.Type ?? "");
                var descriptionCell = new TextContent(parameter.Description ?? "");
                //WriteDetailedDescription(descriptionCell, parameter);
                table.AddRow(nameCell, typeCell, descriptionCell);
            }

        }

        protected override void DrawTagHeader(ItemsTag tag)
        {
            _document.H2().Bold().SetText($"{tag.Order}. " + tag.Name);
        }

        protected override void DrawEndpointContent(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            _document.H1().Bold().SetText("INDEX");

            swaggerDocumentModel.ItemsTags = new List<ItemsTag>();

            int tagNumber = 0;
            foreach (var docEntry in swaggerDocumentModel.DocumentationEntries)
            {
                if (docEntry.Tags != null)
                {
                    for (int i = 0; i < docEntry.Tags.Length; i++)
                    {
                        var element = swaggerDocumentModel.ItemsTags.Find(e => e.Name.Equals(docEntry.Tags[i]));
                        if (element == null)
                        {
                            ItemsTag tag = new ItemsTag();
                            tag.Name = docEntry.Tags[i];


                            swaggerDocumentModel.ItemsTags.Add(tag);
                        }
                    }
                }
            }

            if (swaggerDocumentModel.ItemsTags.Count > 0)
            {

                swaggerDocumentModel.ItemsTags.Sort((x, y) => x.Name.CompareTo(y.Name));

                foreach (var tag in swaggerDocumentModel.ItemsTags)
                {
                    tag.Order = ++tagNumber;
                    _document.H2().Bold().SetText($"{tag.Order}. " + tag.Name);
                    var list = _document.Ul().SetStyle("list-style-type", "none");
                    int itemNumber = 0;
                    foreach (var docEntry in swaggerDocumentModel.DocumentationEntries)
                    {
                        if (docEntry.Tags.Contains<string>(tag.Name))
                        {
                            tag.Items.Add(docEntry);
                            docEntry.ItemOrderNumber = ++itemNumber;
                            docEntry.TagOrder = tag.Order;
                            var link = new Link().Href("#" + docEntry.HttpMethod + "-" + docEntry.EndpointPath);
                            link.SetText($"{tag.Order}.{docEntry.ItemOrderNumber}. {docEntry.HttpMethod} {docEntry.EndpointPath}");
                            list.AddChildElement(link);
                            //list.AddChildElement(new TextContent($"{docEntry.HttpMethod} {docEntry.EndpointPath}"));
                        }
                    }
                }
            }
            else
            {
                ++tagNumber;
                int entryIndex = 0;
                var list = _document.Ul().SetStyle("list-style-type", "none");
                foreach (var docEntry in swaggerDocumentModel.DocumentationEntries)
                {
                    docEntry.ItemOrderNumber = ++entryIndex;
                    docEntry.TagOrder = tagNumber;
                    var link = new Link().Href("#" + docEntry.HttpMethod + "-" + docEntry.EndpointPath);
                    link.SetText($"{tagNumber}.{docEntry.ItemOrderNumber}. {docEntry.HttpMethod} {docEntry.EndpointPath}");
                    list.AddChildElement(link);
                    
                    //list.AddChildElement(new TextContent($"{docEntry.HttpMethod} {docEntry.EndpointPath}"));
                }
            }
            
            ++tagNumber;
            _document.H2().Bold().SetText($"{tagNumber}. " + "Models");
            swaggerDocumentModel.ModelIndex = tagNumber;
            var modelsList = _document.Ul().SetStyle("list-style-type", "none");
            int modelNumber = 0;

            foreach (var item in swaggerDocumentModel.Definitions)
            {
                modelNumber++;
                var link = new Link().Href("#" + item.Key);
                link.SetText($"{tagNumber}.{modelNumber}. " + item.Key);
                //modelsList.AddChildElement(new TextContent(item.Key));
                modelsList.AddChildElement(link);
            }

        }
        protected override void DrawEndpointHeader(EndpointInfo docEntry)
        {
            if (docEntry.Deprecated)
            {
                _document.H1().Bold().Deleted().SetText($"{docEntry.TagOrder}.{docEntry.ItemOrderNumber}. {docEntry.HttpMethod} {docEntry.EndpointPath}").SetStyle(" margin-bottom", "0px")
                    .SetAttribute("id", docEntry.HttpMethod + "-" + docEntry.EndpointPath);
                _document.P().Deleted().SetText(docEntry.Summary).SetStyle(" margin-bottom", "0px");
                _document.P().SetText("Deprecated").SetStyle("color", "orangered").SetStyle(" margin-bottom", "0px");
            }
            else
            {
                _document.H1().Bold().SetText($"{docEntry.TagOrder}.{docEntry.ItemOrderNumber}. {docEntry.HttpMethod} {docEntry.EndpointPath}").SetStyle("color", "#DE5600")
                    .SetAttribute("id", docEntry.HttpMethod + "-" + docEntry.EndpointPath);
                if (!string.IsNullOrEmpty(docEntry.Summary))
                {
                    _document.P().Bold().SetText("Summary:").SetStyle(" margin-bottom", "0px");
                    _document.P().SetText(docEntry.Summary).SetStyle(" margin-bottom", "0px");
                }
            }


            if (!string.IsNullOrEmpty(docEntry.Desciption))
            {
                _document.P().Bold().SetText("Description:").SetStyle(" margin-bottom", "0px");
                _document.P().SetText(docEntry.Desciption).SetStyle(" margin-bottom", "0px");
            }

            if (docEntry.Consumes != null)
            {
                if (docEntry.Consumes.Length > 0)
                {
                    _document.P().Bold().SetText("Consumes:").SetStyle(" margin-bottom", "0px").SetStyle(" margin-bottom", "0px");
                    var list = _document.Ul().SetStyle(" margin-bottom", "0px");
                    foreach (var entry in docEntry.Consumes)
                    {
                        list.AddChildElement(new TextContent(entry));
                    }
                }
            }

            if (docEntry.Produces != null)
            {
                if (docEntry.Produces.Length > 0)
                {
                    _document.P().Bold().SetText("Produces:").SetStyle(" margin-bottom", "0px");
                    var list = _document.Ul();
                    foreach (var entry in docEntry.Produces)
                    {
                        list.AddChildElement(new TextContent(entry));
                    }
                }
            }
        }

        protected override void DrawWelcomePage(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            if (!string.IsNullOrEmpty(swaggerDocumentModel.WelcomePageImage))
            {
                var imageFile = new FileInfo(swaggerDocumentModel.WelcomePageImage);
                if (imageFile.Exists)
                {
                    var companyLogo = HtmlElement.Img().Src(imageFile.Name).SetStyle("margin-top", "150px");
                    _document.Div().AddChildElement(companyLogo).SetStyle("text-align", "center");
                }
            }
            else
            {
                _document.Div().SetStyle("width", "100px").SetStyle("height", "300").SetStyle("display", "block");
            }


            _document.P().Right().SetText("API Reference").SetStyle("font-size", "20px").SetStyle("color", "#005b96").SetStyle(" margin-bottom", "0px");
            swaggerDocumentModel.Title.IfNotNull(x => _document.P().Right().SetText(x).SetStyle("font-size", "32px").SetStyle("font-weight", "bold").SetStyle(" margin-bottom", "0px"));
            swaggerDocumentModel.Version.IfNotNull(x => _document.P().Right().SetText($"API Version: {x}").SetStyle(" margin-bottom", "0px"));

            swaggerDocumentModel.Author.IfNotNull(x => _document.P().Right().SetText($"Author: {x}").SetStyle(" margin-bottom", "0px"));
            swaggerDocumentModel.DocumentDate.ToShortDateString().IfNotNull(x => _document.P().Right().SetText($"Date: {x}"));

            //space between title and description
            _document.P().SetText("\u3164");
            _document.P().SetText("\u3164"); ;
            _document.P().SetText("\u3164"); ;

            if (swaggerDocumentModel.Description != null)
            {
                _document.P().Left().SetText("Description:").SetStyle("font-size", "16px").SetStyle("color", "#005b96").SetStyle(" margin-bottom", "0px");
                swaggerDocumentModel.Description.IfNotNull(x => _document.P().Left().SetText(x).SetStyle(" margin-bottom", "0px"));
            }

            if (swaggerDocumentModel.BasePath != null)
            {
                _document.P().Left().SetText("Base path:").SetStyle("font-size", "16px").SetStyle("color", "#005b96").SetStyle(" margin-bottom", "0px");
                swaggerDocumentModel.BasePath.IfNotNull(x => _document.P().Left().SetText(x).SetStyle(" margin-bottom", "0px"));
            }

            if (!string.IsNullOrEmpty(swaggerDocumentModel.Name) || !string.IsNullOrEmpty(swaggerDocumentModel.URL) || !(string.IsNullOrEmpty(swaggerDocumentModel.Email)))
            {
                _document.P().Left().SetText("Contact:").SetStyle("font-size", "16px").SetStyle("color", "#005b96").SetStyle(" margin-bottom", "0px");
                swaggerDocumentModel.Name.IfNotNull(x => _document.P().Left().SetText(x).SetStyle(" margin-bottom", "0px"));
                swaggerDocumentModel.URL.IfNotNull(x => _document.P().Left().SetText($"URL: {x}").SetStyle(" margin-bottom", "0px"));
                swaggerDocumentModel.Email.IfNotNull(x => _document.P().Left().SetText($"Email: {x}").SetStyle(" margin-bottom", "0px"));
            }

            if (swaggerDocumentModel.Security != null)
            {
                _document.P().Left().SetText(swaggerDocumentModel.Security).SetStyle("font-size", "16px").SetStyle("color", "#005b96").SetStyle(" margin-bottom", "0px");
            }

        }

        protected override void BeginNewPage()
        {
            _document.AddPageBreak();
        }

        protected override void SaveDocument(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            var documentString = _document.GetDocumentString();

            string fileName = System.IO.Path.GetTempFileName();

            var pdfDocument = new PdfDocument(new PdfWriter(fileName));

            var properties = new ConverterProperties();


            if (!string.IsNullOrEmpty(swaggerDocumentModel.WelcomePageImage))
            {
                var imageFile = new FileInfo(swaggerDocumentModel.WelcomePageImage);
                properties.SetBaseUri(imageFile.DirectoryName + "\\");
            }

            HtmlConverter.ConvertToDocument(documentString, pdfDocument, properties);

            pdfDocument.Close();

            PdfDocument pdfDoc = new PdfDocument(new PdfReader(fileName), new PdfWriter(swaggerDocumentModel.PdfDocumentPath));
            Document doc = new Document(pdfDoc);

            int numberOfPages = pdfDoc.GetNumberOfPages();
            for (int i = 2; i <= numberOfPages; i++)
            {

                // Write aligned text to the specified by parameters point
                if (!string.IsNullOrEmpty(swaggerDocumentModel.Title))
                {
                    Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                    float x = pageSize.GetWidth() / 2;
                    doc.SetFontSize(9);
                    doc.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
                    doc.ShowTextAligned(new Paragraph($"{swaggerDocumentModel.Title} API reference"), x, doc.GetBottomMargin() - 10, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                }
                doc.ShowTextAligned(new Paragraph($"{i}"), 559, doc.GetBottomMargin() - 10, i, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);
            }

            doc.Close();
            File.Delete(fileName);
        }

        protected void DrawTableRow(TableElement table, KeyValuePair<string, Schema> property, string prefix, int offset, bool required = false)
        {
            string _name;
            if (!string.IsNullOrEmpty(prefix))
            {
                _name = prefix + ".";
            }
            else
            {
                _name = "";
            }

            var nameCell = HtmlElement.Label().SetText($"{_name}{property.Key}" ?? "");
            if (offset > 0)
            {
                int i = 6 * offset;
                nameCell.SetStyle("padding-left", $"{i}px");
            }

            if (required)
            {
                nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
            }

            string desc = "";


            if (property.Value is SimpleTypeSchema)
            {
                if (!string.IsNullOrEmpty((property.Value as SimpleTypeSchema).Description))
                {
                    desc = (property.Value as SimpleTypeSchema).Description;
                }

            }


            //var descriptionCell = new TextContent(desc ?? "");
            var descriptionCell = HtmlElement.Label().SetText(desc ?? "").SetStyle("white-space", "pre-wrap").SetStyle("word-break", "break-all");
            if (property.Value is SimpleTypeSchema)
            {
                if ((property.Value as SimpleTypeSchema).Example != null)
                {
                    SimpleTypeSchema simpleType = property.Value as SimpleTypeSchema;
                    var sampletext = PdfModelJsonConverter.SerializeObject(SimpleTypeSchema.ExampleToObject(simpleType.Type, simpleType.Example));
                    if (!string.IsNullOrEmpty(sampletext))
                    {
                        if (!string.IsNullOrEmpty(desc))
                        {
                            descriptionCell.AddChildElement(HtmlElement.Br());
                        }
                        descriptionCell.AddChildElement(HtmlElement.Label().SetText("Example: " + sampletext));
                    }
                }
            }

            string propertyType = "";
            if (property.Value is SimpleTypeSchema)
                propertyType = (property.Value as SimpleTypeSchema).Type;
            if (property.Value is ArraySchema)
                propertyType = "array";
            if (property.Value is EnumTypeSchema)
                propertyType = "enum";

            Link propertyLink = null;
            if (property.Value is ComplexTypeSchema)
            {
                propertyType = "object";
                
                var referenceproperty = property.Value as ComplexTypeSchema;
                if (!string.IsNullOrEmpty(referenceproperty.Ref))
                {
                    var referenceName = referenceproperty.Ref.Split('/').Last();
                    if (!string.IsNullOrEmpty(propertyType))
                    {
                        propertyType = propertyType + ", ";
                    }
                    else
                    {
                        propertyType = "";
                    }
                    propertyType = propertyType + "see: " + @"<b>" + referenceName + @"</b>";
                    propertyLink = new Link().Href("#" + referenceName);
                    propertyLink.SetText(propertyType);

                }
            }
            var typeCell = new TextContent(propertyType ?? "");
            if (propertyLink == null)
            {
                table.AddRow(nameCell, typeCell, descriptionCell);
            }
            else
            {
                table.AddRow(nameCell, propertyLink, descriptionCell);
            }
        }

        protected string TypeToString(PropertyBase property)
        {
            string propertyType = "";
            if (property is SimpleTypeProperty)
                propertyType = (property as SimpleTypeProperty).Type;
            if (property is ArrayProperty)
                propertyType = "array";
            if (property is EnumSimpleTypeProperty)
                propertyType = "enum";
            if (property is ObjectProperty)
                propertyType = "object";
            return propertyType;
        }

        protected string TypeToString(Definition property)
        {
            return property.Type;
        }

        protected void DrawTableRow(TableElement table, KeyValuePair<string, PropertyBase> property, string prefix, int offset, bool required = false)
        {
            string _name;
            if (!string.IsNullOrEmpty(prefix))
            {
                _name = prefix + ".";
            }
            else
            {
                _name = "";
            }

            var nameCell = HtmlElement.Label().SetText($"{_name}{property.Key}" ?? "");
            if (offset > 0)
            {
                int i = 6 * offset;
                nameCell.SetStyle("padding-left", $"{i}px");
            }

            if (required)
            {
                nameCell.AddChildElement(HtmlElement.Label().SetText("*").SetStyle("color", "red"));
            }

            string desc = "";
            if (!string.IsNullOrEmpty(property.Value.Description))
            {
                desc = property.Value.Description;
            }

            string propertyType = TypeToString(property.Value);
            string sampleText = string.Empty;


            if (property.Value.Example != null)
            {
                sampleText = PdfModelJsonConverter.SerializeObject(SimpleTypeSchema.ExampleToObject(propertyType, property.Value.Example));
                if (sampleText == "\"\"")
                {
                    sampleText = string.Empty;
                }
            }


            if (property.Value is ObjectProperty)
            {
                propertyType = "object";
            }


            Link propertyLink = null;
            ReferenceProperty referrenceproperty = null;

            if (property.Value is ReferenceProperty)
            {
                referrenceproperty = (property.Value as ReferenceProperty);
            }

            if (property.Value is ArrayProperty)
            {
                if ((property.Value as ArrayProperty).Items is ReferenceProperty)
                {
                    referrenceproperty = ((property.Value as ArrayProperty).Items as ReferenceProperty);
                }
                else
                {
                    var arrayItemType = TypeToString((property.Value as ArrayProperty).Items);
                    if (!arrayItemType.IsNullOrEmpty())
                    {
                        propertyType = "array of " + arrayItemType;
                    }
                }
            }

            if (referrenceproperty != null)
            {
                var referenceName = referrenceproperty.Ref.Split('/').Last();
                var definition = currentModel.ReferenceResolver.ResolveReference(referrenceproperty.Ref);
                if (definition != null)
                {
                    propertyType = definition.Type;
                    if (!string.IsNullOrEmpty(definition.Description))
                    {
                        desc = definition.Description;
                    }


                    if (definition.Example != null)
                    {
                        sampleText = PdfModelJsonConverter.SerializeObject(SimpleTypeSchema.ExampleToObject(definition.Type, definition.Example));
                    }

                }

                if (!string.IsNullOrEmpty(propertyType))
                {
                    propertyType = propertyType + ", ";
                }
                else
                {
                    propertyType = "";
                }
                propertyType = propertyType + "see: " + @"<b>" + referenceName + @"</b>";
                propertyLink = new Link().Href("#" + referenceName);
                propertyLink.SetText(propertyType);
            }


            var descriptionCell = HtmlElement.Label().SetText(desc ?? "").SetStyle("white-space", "pre-wrap").SetStyle("word-break", "break-all");
            //TextContent(desc ?? "").SetStyle("white-space", "pre-wrap");
            if (!string.IsNullOrEmpty(sampleText))
            {
                if (!string.IsNullOrEmpty(desc))
                {
                    descriptionCell.AddChildElement(HtmlElement.Br());
                }
                descriptionCell.AddChildElement(HtmlElement.Label().SetText("Example: " + sampleText));
            }

            var typeCell = new TextContent(propertyType ?? "");
            if (propertyLink == null)
            {
                table.AddRow(nameCell, typeCell, descriptionCell);
            }
            else
            {
                table.AddRow(nameCell, propertyLink, descriptionCell);
            }
        }

        protected void DrawObjectProperty(TableElement table, string prefix, ObjectProperty objectProperty, int offset)
        {
            offset++;
            if (objectProperty.Properties != null)
            {
                foreach (var subitem in objectProperty.Properties)
                {
                    bool required = false;
                    if (objectProperty.Required != null)
                    {
                        required = objectProperty.Required.Contains(subitem.Key);
                    }
                    DrawTableRow(table, subitem, prefix, offset, required);
                    if (subitem.Value is ObjectProperty)
                    {
                        string extraPrefix = prefix;
                        if (string.IsNullOrEmpty(extraPrefix))
                            extraPrefix = subitem.Key;
                        else
                            extraPrefix = extraPrefix + "." + subitem.Key;

                        DrawObjectProperty(table, extraPrefix, (ObjectProperty)subitem.Value, offset);
                    }
                }
            }
        }

        protected override void DrawModelDocumentation(SwaggerPdfDocumentModel swaggerDocumentModel)
        {


            _document.H1().SetText("Models");

            int modelNumber = 0;

            foreach (var item in swaggerDocumentModel.Definitions)
            {
                string itemTitle;
                modelNumber++;

                if (item.Value.Properties != null)
                {
                    itemTitle = $"{swaggerDocumentModel.ModelIndex}.{modelNumber}. " + item.Key;
                }
                else
                {
                    itemTitle = $"{swaggerDocumentModel.ModelIndex}.{modelNumber}. " + item.Key + ": " + item.Value.Type;
                }
                _document.P();//.AddChildElement(new Link().SetAttribute("id", item.Key));
                _document.H2().SetText(itemTitle).SetAttribute("id", item.Key).SetStyle("page -break-after", "avoid"); ;
                item.Value.Description.IfNotNull(x => _document.P().Left().SetText(x).SetStyle(" margin-bottom", "0px"));
                if (item.Value.Example != null)
                {
                    string propertyType = TypeToString(item.Value);
                    var sample = PdfModelJsonConverter.SerializeObject(SimpleTypeSchema.ExampleToObject(propertyType, item.Value.Example));
                    _document.Pre().Left().SetText($"Example: {sample}").SetStyle(" margin-bottom", "0px");
                    _document.P();
                }

                if (item.Value.Properties != null)
                {


                    var table = _document.Table();
                    table.AddColumns(new TextContent("NAME"), new TextContent("TYPE"), new TextContent("DESCRIPTION"));
                    foreach (var property in item.Value.Properties)
                    {
                        DrawTableRow(table, property, "", 0);
                        if (property.Value is ReferenceProperty)
                        {
                            var referrenceproperty = (property.Value as ReferenceProperty);
                            var definition = swaggerDocumentModel.ReferenceResolver.ResolveReference(referrenceproperty.Ref);
                            if (definition != null)
                            {
                                if (definition.Properties != null)
                                {
                                    foreach (var subitem in definition.Properties)
                                    {
                                        DrawTableRow(table, subitem, property.Key, 0);
                                    }
                                }
                            }
                        }
                        if (property.Value is ObjectProperty)
                        {
                            var objectProperty = (property.Value as ObjectProperty);
                            DrawObjectProperty(table, property.Key, objectProperty, 0);

                        }

                    }
                }

            }
        }

        protected override void DrawAuthorizationInfoPage(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            if (swaggerDocumentModel.AuthorizationInfo == null)
                return;
            _document.H1().SetText("Authorization information");
            _document.P();
            foreach (var authorizationInfo in swaggerDocumentModel.AuthorizationInfo)
            {
                _document.H2().Bold().SetText($"Authorization option: {authorizationInfo.Key}");
                _document.P();

                var infoParagraph = _document.Div();
                authorizationInfo.Value.WriteAuthorizationInfo(new HtmlAuthorizationWriter(infoParagraph));
                _document.P();
            }
            BeginNewPage();
        }

        private static void WriteDetailedDescription(HtmlElement element, Parameter p)
        {
            if (p.Deprecated)
            {
                element.AddChildElement(HtmlElement.Label().SetText("Deprecated").SetColor("red")).AddChildElement(HtmlElement.Br());
                p.Description.IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText(x).Deleted().SetColor("red")).AddChildElement(HtmlElement.Br()));
            }
            else
            {
                p.Description.IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText(x)));
            }

            if (p.IsRequired)
            {
                element.AddChildElement(HtmlElement.Br());
                element.AddChildElement(HtmlElement.Label().Bold().SetText("Required"));
            }

            if (p.Schema != null)
            {
                element.AddChildElement(HtmlElement.Br());
                p.Schema?.WriteDetailedDescription(new HtmlAuthorizationWriter(element));
                element.AddChildElement(HtmlElement.Br());
            }

            p.Pattern.IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText($"Pattern: {x}")).AddChildElement(HtmlElement.Br()));
            p.GetMinMaxString().IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText(x)).AddChildElement(HtmlElement.Br()));
            p.GetExclusiveMinMaxString().IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText(x)).AddChildElement(HtmlElement.Br()));
            p.GetMinMaxItems().IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText(x)).AddChildElement(HtmlElement.Br()));
            p.GetMinMaxProperties().IfNotNull(x => element.AddChildElement(HtmlElement.Label().SetText(x)).AddChildElement(HtmlElement.Br()));

        }
    }
}
