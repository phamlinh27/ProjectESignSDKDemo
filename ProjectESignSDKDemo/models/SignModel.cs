using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models
{
    public class Document
    {
        public Document(string documentId, string fileToSign, string documentName)
        {
            DocumentId = documentId;
            FileToSign = fileToSign;
            DocumentName = documentName;
        }

        public string DocumentId { get; set; }
        public string FileToSign { get; set; }
        public string DocumentName { get; set; }
    }

    public class SignModel
    {
        public SignModel(string dataToBeDisplayed, string userId, string certAlias, List<Document> documents)
        {
            DataToBeDisplayed = dataToBeDisplayed;
            UserId = userId;
            CertAlias = certAlias;
            Documents = documents;
        }

        public string DataToBeDisplayed { get; set; }
        public string UserId { get; set; }
        public string CertAlias { get; set; }
        public List<Document> Documents { get; set; }
    }

    public class SignRespone
    {
        public string transactionId { get; set; }
        public int signingAuthorizeTimeout { get; set; }
        public object signatures { get; set; }
        public object userSigningSession { get; set; }
        public bool sessionSigningEnabled { get; set; }
    }

    public class SignChecker
    {
        public string status { get; set; }
        public string errorCode { get; set; }
        public string errorDescription { get; set; }
        public List<Signature> signatures { get; set; }
    }

    public class Signature
    {
        public string signature { get; set; }
    }
}
