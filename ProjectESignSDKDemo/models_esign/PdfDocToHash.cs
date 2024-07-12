using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class PdfDocToHash
    {
        public string DocumentId { get; set; }

        public byte[] FileToSign { get; set; }

        public PdfSignatureInfo SignatureInfo { get; set; }
    }

    public class PdfDocToHashResult
    {
        public string DocumentId { get; set; }

        public byte[] DocumentBytes { get; set; }

        public byte[] DocumentHash { get; set; }

        public byte[] Sh { get; set; }

        public string SignatureName { get; set; }

        public byte[] Digest { get; set; }

        public byte[] Certificate { get; set; }
    }
}
