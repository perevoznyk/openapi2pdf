﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Swagger2Pdf.Filters;
using Swagger2Pdf.Model;
using Swagger2Pdf.Model.Converters;
using Swagger2Pdf.Model.Properties;
using Swagger2Pdf.Model.ReferenceResolver;
using Swagger2Pdf.PdfModel.Model;

namespace Swagger2Pdf
{
    public class SwaggerPdfDocumentModelProvider
    {
        private readonly SwaggerJsonProvider _swaggerJsonProvider;
        private ReferenceResolver _referenceResolver;
        public static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        public SwaggerPdfDocumentModelProvider()
        {
            _swaggerJsonProvider = new SwaggerJsonProvider();
        }

        public SwaggerPdfDocumentModel PrepareSwaggerPdfModel(CommandLineInputParameters parameters)
        {
            Logger.Info("Started preparing swagger pdf model");
            var swaggerJsonString = _swaggerJsonProvider.GetSwaggerJsonString(parameters.InputFileName);
            var swaggerJsonInfo = GetSwaggerInfoFromJsonString(swaggerJsonString);

            Logger.Info("Preparing PDF model");
            var docModel = new SwaggerPdfDocumentModel();
            docModel.PdfDocumentPath = parameters.OutputFileName;
            docModel.WelcomePageImage = parameters.WelcomePageImagePath;
            docModel.Title = parameters.Title ?? swaggerJsonInfo.SwaggerJsonInfo.Title;
            docModel.Version = parameters.Version ?? swaggerJsonInfo.SwaggerJsonInfo.Version;
            docModel.Author = parameters.Author ?? "";
            docModel.DocumentDate = DateTime.Now;
            docModel.Description = swaggerJsonInfo.SwaggerJsonInfo.Description;
            docModel.Security = parameters.Security;
            docModel.BasePath = swaggerJsonInfo.BasePath;
            docModel.Host = swaggerJsonInfo.Host;

            if (swaggerJsonInfo.SwaggerJsonInfo.Contact != null)
            {
                docModel.Name = swaggerJsonInfo.SwaggerJsonInfo.Contact.Name;
                docModel.URL = swaggerJsonInfo.SwaggerJsonInfo.Contact.Url;
                docModel.Email = swaggerJsonInfo.SwaggerJsonInfo.Contact.Email;
            }
            else
            {
                docModel.Name = "";
                docModel.URL = "";
                docModel.Email = "";
            }

            docModel.DocumentationEntries = PrepareDocumentationEntries(parameters.EndpointFilters, swaggerJsonInfo);
            docModel.AuthorizationInfo = PrepareAuthorizationInfos(swaggerJsonInfo);
            docModel.CustomPageName = parameters.CustomPageName;
            docModel.Tags = swaggerJsonInfo.Tags;
            docModel.Definitions = _referenceResolver.Definitions;
            docModel.ReferenceResolver = _referenceResolver;
            return docModel;
        }

        private SwaggerJsonModel GetSwaggerInfoFromJsonString(string jsonString)
        {
            Logger.Info("Retrieving reference resolver");
            _referenceResolver = JsonConvert.DeserializeObject<ReferenceResolver>(jsonString, new PropertyBaseJsonConverter());

            Logger.Info("Retrieveing swagger.json information");
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                if (settings.Converters == null)
                {
                    settings.Converters = new List<JsonConverter>();
                }

                settings.Converters.Add(new PropertyBaseJsonConverter());
                settings.Converters.Add(new SecurityDefinitionConverter());
                return settings;
            };

            
            var swaggerInfo = JsonConvert.DeserializeObject<SwaggerJsonModel>(jsonString);
            return swaggerInfo;

        }

        private List<EndpointInfo> PrepareDocumentationEntries(IEnumerable<string> endpointFiltersStrings, SwaggerJsonModel swaggerJsonJsonModel)
        {
            Logger.Info("Preparing endpoint information");
            var endpointFilters = endpointFiltersStrings?.Select(EndpointFilterFactory.CreateEndpointFilter).ToList() ?? new List<EndpointFilter>();
            var schemaResolutionContext = _referenceResolver.CreateResolutionContext();

            var swaggerPdfEndpointList = swaggerJsonJsonModel.Paths
                .SelectMany(path => BuildEndpointEntry(path, schemaResolutionContext))
                .ToList();

            if (!endpointFilters.Any())
            {
                Logger.Info($"No filters applied, endpoints obtained: {swaggerPdfEndpointList.Count}");
                return swaggerPdfEndpointList;
            }

            var filteredSwaggerPdfEndpointList = new List<EndpointInfo>(swaggerPdfEndpointList.Capacity);
            foreach (var endpointFilter in endpointFilters)
            {
                Logger.Info($"Applying filter to endpoint documentation: {endpointFilter.EndpointFilterString}");
                filteredSwaggerPdfEndpointList.AddRange(swaggerPdfEndpointList.Where(e => endpointFilter.MatchEndpoint(e.HttpMethod, e.EndpointPath)));
            }

            Logger.Info($"Got endpoints: {swaggerPdfEndpointList.Count}, after filtering {filteredSwaggerPdfEndpointList.Count} left");
            return filteredSwaggerPdfEndpointList;
        }

        private static Dictionary<string, AuthorizationInfo> PrepareAuthorizationInfos(SwaggerJsonModel swaggerJsonJsonModel)
        {
            if (swaggerJsonJsonModel.SecurityDefinitions != null)
                return swaggerJsonJsonModel.SecurityDefinitions.ToDictionary(x => x.Key, x => x.Value.CreateAuthorizationInfo());
            else
                return null;
        }

        private static IEnumerable<EndpointInfo> BuildEndpointEntry(KeyValuePair<string, Dictionary<string, Operation>> path, SchemaResolutionContext schemaResolutionContext)
        {
            Logger.Info($"Processing endpoint: {path.Key}");
            
            return path.Value.Select(httpMethod => new EndpointInfo
            {
                EndpointPath = path.Key,
                Tags = httpMethod.Value.Tags,
                HttpMethod = httpMethod.Key.ToUpper(),
                Deprecated = httpMethod.Value.Deprecated,
                Summary = httpMethod.Value.Summary,
                Desciption = httpMethod.Value.Description,
                Produces = httpMethod.Value.Produces,
                Consumes = httpMethod.Value.Consumes,
                QueryParameter = httpMethod.Value.OperationParameters?.Where(x => x.In == "query")
                    .Select(parameter => BuildParameter(parameter, schemaResolutionContext))
                    .ToList(),
                BodyParameters = httpMethod.Value.OperationParameters?.Where(x => x.In == "body")
                    .Select(parameter => BuildParameter(parameter, schemaResolutionContext))
                    .ToList(),
                HeaderParameters = httpMethod.Value.OperationParameters?.Where(x => x.In == "header")
                    .Select(parameter => BuildParameter(parameter, schemaResolutionContext))
                    .ToList(),
                FormDataParameters = httpMethod.Value.OperationParameters?.Where(x => x.In == "formData")
                    .Select(parameter => BuildParameter(parameter, schemaResolutionContext))
                    .ToList(),
                PathParameters = httpMethod.Value.OperationParameters?.Where(x => x.In == "path")
                    .Select(parameter => BuildParameter(parameter, schemaResolutionContext))
                    .ToList(),
                Responses = httpMethod.Value.Responses?
                    .Select(response => BuildResponse(response, schemaResolutionContext))
                    .ToList()
            });
        }

        private static Parameter BuildParameter(OperationParameter p, SchemaResolutionContext resolutionContext)
        {
            return new Parameter
            {
                Name = p.Name,
                IsRequired = p.IsRequired,
                Schema = p.Schema?.ResolveSchema(resolutionContext),
                Description = p.Description,
                Type = p.Type,
                Deprecated = p.Deprecated,
                AllowEmptyValue = p.AllowEmptyValue,
                Enum = p.Enum,
                ExclusiveMaximum = p.ExclusiveMaximum,
                ExclusiveMinimum = p.ExclusiveMinimum,
                MaxItems = p.MaxItems,
                MaxLength = p.MaxLength,
                MaxProperties = p.MaxProperties,
                Maximum = p.Maximum,
                MinItems = p.MinItems,
                MinLength = p.MinLength,
                MinProperties = p.MinProperties,
                Minimum = p.Minimum,
                MultipleOf = p.MultipleOf,
                Pattern = p.Pattern,
                Title = p.Title,
                UniqueItems = p.UniqueItems,
                Ref = p.Schema?.GetReference()
            };
        }

        private static Response BuildResponse(KeyValuePair<string, OperationResponse> responseKvp, SchemaResolutionContext resolutionContext)
        {
            return new Response
            {
                Code = responseKvp.Key,
                Description = responseKvp.Value.Description,
                Schema = responseKvp.Value.Schema?.ResolveSchema(resolutionContext),
                Ref = responseKvp.Value.Schema?.GetReference()
            };
        }
    }
}