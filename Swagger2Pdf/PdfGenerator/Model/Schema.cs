﻿using MigraDoc.DocumentObjectModel;

namespace Swagger2Pdf.PdfGenerator.Model
{
    public abstract class Schema
    {
        public virtual void WriteDetailedDescription(Paragraph paragraph)
        {
        }
    }
}