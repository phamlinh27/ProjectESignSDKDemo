using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace ProjectESignSDKDemo.models_esign
{
    public class PostSigning : IExternalSignatureContainer
    {
        protected byte[] _sig;

        protected PdfDictionary sigDic;

        public PostSigning(byte[] sign, PdfName filter, PdfName subFilter)
        {
            //IL_0010: Unknown result type (might be due to invalid IL or missing references)
            //IL_001a: Expected O, but got Unknown
            _sig = sign;
            sigDic = new PdfDictionary();
            sigDic.Put(PdfName.FILTER, (PdfObject)(object)filter);
            sigDic.Put(PdfName.SUBFILTER, (PdfObject)(object)subFilter);
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            signDic.PutAll(sigDic);
        }

        public byte[] Sign(Stream data)
        {
            return _sig;
        }
    }
}
