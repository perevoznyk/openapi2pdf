﻿using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using Swagger2Pdf.PdfModel.Model;

namespace Swagger2Pdf.PdfModel
{
    public abstract class PdfBuilderBase
    {
        public readonly ILog Logger;

        protected SwaggerPdfDocumentModel currentModel;

        protected PdfBuilderBase(ILog logger)
        {
            Logger = logger;
            
        }

        public void BuildPdf(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            Logger.Info("Building pdf document");
            currentModel = swaggerDocumentModel;
            Logger.Info("Drawing welcome page");
            DrawWelcomePage(swaggerDocumentModel);
            BeginNewPage();
            Logger.Info("Drawing welcome page done.");

            Logger.Info("Drawing custom page");
            DrawCustomPage(swaggerDocumentModel);
            Logger.Info("Drawing custom page done");

            Logger.Info("Drawing authorization info page");
            DrawAuthorizationInfoPage(swaggerDocumentModel);
           
            Logger.Info("Drawing authorization info page done.");

            Logger.Info("Drawing  endpoint documentation");
            DrawEndpointDocumentation(swaggerDocumentModel);
            Logger.Info("Drawing  endpoint documentation done.");

            DrawModelDocumentation(swaggerDocumentModel);

            Logger.Info("Rendering PDF document");
            var fi = new FileInfo(swaggerDocumentModel.PdfDocumentPath);
            Logger.Info($"Saving PDF document to: {fi.FullName}");
            SaveDocument(swaggerDocumentModel);
            Logger.Info("Done");
        }

        private void DrawCustomPage(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            var customPagePath = swaggerDocumentModel.CustomPageName;
            if (string.IsNullOrEmpty(customPagePath))
            {
                Logger.Info("No custom page");
                return;
            }
            FileInfo customPageFileInfo = new FileInfo(customPagePath);
            if (!customPageFileInfo.Exists) throw new ArgumentException($"File {customPagePath} does not exist");
            if (customPageFileInfo.Extension != ".md") throw new ArgumentException($"Only markdown (.md) files are supported. Current: {customPageFileInfo.Extension}");
            Logger.Info("Writing custom page");
            using (var reader = new StreamReader(customPageFileInfo.FullName))
            using (var writer = new StringWriter())
            {
                CommonMark.CommonMarkConverter.Convert(reader, writer);
                WriteCustomPage(writer);
                BeginNewPage();
            }
        }

        public SwaggerPdfDocumentModel DocumentModel { get; set; }

        private void DrawEndpointDocumentation(SwaggerPdfDocumentModel swaggerDocumentModel)
        {
            this.DocumentModel = swaggerDocumentModel;
            DrawEndpointContent(swaggerDocumentModel);
            BeginNewPage();

            if (swaggerDocumentModel.ItemsTags.Count > 0)
            {
                foreach (var tag in swaggerDocumentModel.ItemsTags)
                {
                    DrawTagHeader(tag);
                    int itemIndex = 0;
                    foreach (var docEntry in tag.Items)
                    {
                        docEntry.ItemOrderNumber = ++itemIndex;
                        DrawEndpointHeader(docEntry);
                        DrawPathParameters(docEntry.PathParameters ?? new List<Parameter>());
                        DrawQueryParameters(docEntry.QueryParameter ?? new List<Parameter>());
                        DrawHeaderParameters(docEntry.HeaderParameters ?? new List<Parameter>());
                        DrawFormDataParameters(docEntry.FormDataParameters ?? new List<Parameter>());
                        DrawBodyParameters(docEntry.BodyParameters ?? new List<Parameter>());
                        DrawResponses(docEntry.Responses);
                        BeginNewPage();
                    }
                }
            }
            else
            {
                foreach (var docEntry in swaggerDocumentModel.DocumentationEntries)
                {
                    DrawEndpointHeader(docEntry);
                    DrawPathParameters(docEntry.PathParameters ?? new List<Parameter>());
                    DrawQueryParameters(docEntry.QueryParameter ?? new List<Parameter>());
                    DrawHeaderParameters(docEntry.HeaderParameters ?? new List<Parameter>());
                    DrawFormDataParameters(docEntry.FormDataParameters ?? new List<Parameter>());
                    DrawBodyParameters(docEntry.BodyParameters ?? new List<Parameter>());
                    DrawResponses(docEntry.Responses);
                    BeginNewPage();
                }
            }
        }

        

        protected abstract void WriteCustomPage(StringWriter writer);

        protected abstract void DrawResponses(List<Response> docEntryResponses);

        protected abstract void DrawBodyParameters(List<Parameter> docEntryBodyParameters);

        protected abstract void DrawFormDataParameters(List<Parameter> docEntryFormDataParameters);

        protected abstract void DrawQueryParameters(List<Parameter> docEntryQueryParameter);

        protected abstract void DrawHeaderParameters(List<Parameter> docEntryQueryParameter);

        protected abstract void DrawPathParameters(List<Parameter> docEntryPathParameters);

        protected abstract void DrawEndpointHeader(EndpointInfo docEntry);

        protected abstract void DrawEndpointContent(SwaggerPdfDocumentModel swaggerDocumentModel);

        protected abstract void DrawWelcomePage(SwaggerPdfDocumentModel swaggerDocumentModel);

        protected abstract void BeginNewPage();

        protected abstract void SaveDocument(SwaggerPdfDocumentModel swaggerDocumentModel);

        protected abstract void DrawAuthorizationInfoPage(SwaggerPdfDocumentModel swaggerDocumentModel);

        protected abstract void DrawModelDocumentation(SwaggerPdfDocumentModel swaggerDocumentModel);

        protected abstract void DrawTagHeader(ItemsTag tag);

    }
}
