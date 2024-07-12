using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace ProjectESignSDKDemo.models_esign
{
    public class PreSigning : IExternalSignatureContainer
    {
        protected PdfDictionary sigDic;

        private byte[] hash;

        private string _hashAlgorithm;

        public PreSigning(PdfName filter, PdfName subFilter, string hashAlgorithm)
        {
            //IL_0009: Unknown result type (might be due to invalid IL or missing references)
            //IL_0013: Expected O, but got Unknown
            sigDic = new PdfDictionary();
            sigDic.Put(PdfName.FILTER, (PdfObject)(object)filter);
            sigDic.Put(PdfName.SUBFILTER, (PdfObject)(object)subFilter);
            _hashAlgorithm = hashAlgorithm;
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            signDic.PutAll(sigDic);
        }

        public byte[] Sign(Stream data)
        {
            hash = DigestAlgorithms.Digest(data, DigestAlgorithms.GetMessageDigest(_hashAlgorithm));
            return new byte[0];
        }

        public byte[] GetHash()
        {
            return hash;
        }

        public void SetHash(byte[] hash)
        {
            this.hash = hash;
        }
    }
}
