using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class AttachSignatureData
    {
        public DocumentType DocumentType { get; set; }

        public List<AttachSignaturePdfData> PdfDocs { get; set; }

        public List<AttachSignatureXmlData> XmlDocs { get; set; }

        public List<AttachSignatureExcelData> ExcelDocs { get; set; }

        public List<AttachSignatureWordData> WordDocs { get; set; }

        public byte[] Certififcate { get; set; }

        public List<string> CertififcateChain { get; set; }

        public string HashAlgorithm { get; set; }
    }

    public class AttachSignaturePdfData : PdfDocToHashResult
    {
        public byte[] Signature { get; set; }
    }


    public class AttachSignatureXmlData : XmlDocToHashResult
    {
        public byte[] Signature { get; set; }
    }

    public class AttachSignatureExcelData : ExcelDocToHashResult
    {
        public byte[] Signature { get; set; }
    }

    public class AttachSignatureWordData : WordDocToHashResult
    {
        public byte[] Signature { get; set; }
    }
}
