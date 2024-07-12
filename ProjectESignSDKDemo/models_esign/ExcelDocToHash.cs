using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class ExcelDocToHash
    {
        public string DocumentId { get; set; }

        public byte[] FileToSign { get; set; }

        public byte[] SignatureImage { get; set; }
    }

    public class ExcelDocToHashResult
    {
        public string DocumentId { get; set; }

        public byte[] DocumentBytes { get; set; }

        public string SignatureId { get; set; }

        public byte[] Digest { get; set; }

        public string MainDom { get; set; }
    }
}
