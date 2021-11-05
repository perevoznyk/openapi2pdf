[![Build status](https://dev.azure.com/andrzejderylo/swagger2pdf/_apis/build/status/swagger2pdf-netcore)](https://dev.azure.com/andrzejderylo/swagger2pdf/_build/latest?definitionId=3)

# openapi2pdf
Console tool for generating pdf documents out of `swagger.json` or `swagger.yaml` file. Initially forked from https://github.com/andrzejderylo/swagger2pdf, this project goes its own way

# Features:
- Generate pdf from `swagger.json`  or `swagger.yaml`
- Include company logo on first page
- Include custom page created with .md right after welcome page
- Override `swagger.json`  or `swagger.yaml` author name
- Filter endpoints which needs to be printed to pdf doc (wildcards supported)

# Thanks
I would like to say thank you to:
- Authors & contributors of [iText7](https://github.com/itext/itext7-dotnet) for that amazingly easy to use pdf library
- Authors & contruibutors of [CommandLineParser](https://github.com/commandlineparser/commandline) for awesome piece of code for parsing command line parameters like 
a boss. 
- Authors & contributors of [CommonMark.NET](https://github.com/Knagis/CommonMark.NET/) for providing such easy way to parse .md files.

# Endpoints markup

For filtering sake, following combinations are allowed: 
- `GET:/pet` - only endpoint with that specific verb and path is taken under consideration
- `/pet` - all endpoints matching this path regardless verb are taken under consideration
- `POST:/*` - all endpoints matching this verb regardless path are taken under consideration
- `POST:/p*t` - all endpoints with verb POST and path starting with 'p' and ending with 't' are taken under consideration. Wildcards are supported only at path section of URL address.

# Usage
## Help
`Swagger2Pdf.exe --help`
## Normal scenario
`Swagger2Pdf.exe --input https://petstore.swagger.io/v2/swagger.json --output ./petstore.pdf`
## Using local swagger.json file
`Swagger2Pdf.exe --input ./swagger.json --output ./petstore.pdf`
## Include company logo
``Swagger2Pdf.exe --input https://petstore.swagger.io/v2/swagger.json --output ./petstore.pdf --picture ./image.png``
## Include custom page
``Swagger2Pdf.exe --input https://petstore.swagger.io/v2/swagger.json --output ./petstore.pdf --picture ./image.png --custom-page ./page.md``
## Filtering endpoints
`Swagger2Pdf.exe --filter :/pet GET:/store/inventory --input https://petstore.swagger.io/v2/swagger.json --output ./petstore.pdf`
## Filtering endpoints with wildcard
`Swagger2Pdf.exe --filter GET:/pet* /store/* --input https://petstore.swagger.io/v2/swagger.json --output ./petstore.pdf`
  
