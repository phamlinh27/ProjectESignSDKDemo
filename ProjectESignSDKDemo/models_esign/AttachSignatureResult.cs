using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class AttachSignatureResult
    {
        public DocumentType DocumentType { get; set; }

        public List<AttachSignaturePdfResult> PdfDocs { get; set; }

        public List<AttachSignatureXmlResult> XmlDocs { get; set; }

        public List<AttachSignatureExcelResult> ExcelDocs { get; set; }

        public List<AttachSignatureWordResult> WordDocs { get; set; }
    }

    public class AttachSignaturePdfResult
    {
        public string DocumentId { get; set; }

        public byte[] Document { get; set; }
    }

    public class AttachSignatureXmlResult
    {
        public string DocumentId { get; set; }

        public string Document { get; set; }
    }

    public class AttachSignatureExcelResult
    {
        public string DocumentId { get; set; }

        public byte[] Document { get; set; }
    }

    public class AttachSignatureWordResult
    {
        public string DocumentId { get; set; }

        public byte[] Document { get; set; }
    }
}
