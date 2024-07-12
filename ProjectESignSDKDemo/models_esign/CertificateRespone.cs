
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ProjectESignSDKDemo.models_esign
{
    public class CertificateRespone
    {
        public CertificateRespone(string userId, string keyAlias, string appName, string keyStatus, string certificate, List<string> certiticateChain, string certStatus, string certKey, DateTime effectiveDate, DateTime expirationDate, object emailName, object signAlgo, object clientId, object certName, object internationalCertName, int certType, bool isAutoSign)
        {
            this.userId = userId;
            this.keyAlias = keyAlias;
            this.appName = appName;
            this.keyStatus = keyStatus;
            this.certificate = certificate;
            this.certiticateChain = certiticateChain;
            this.certStatus = certStatus;
            this.certKey = certKey;
            this.effectiveDate = effectiveDate;
            this.expirationDate = expirationDate;
            this.emailName = emailName;
            this.signAlgo = signAlgo;
            this.clientId = clientId;
            this.certName = certName;
            this.internationalCertName = internationalCertName;
            this.certType = certType;
            this.isAutoSign = isAutoSign;
        }

        public string userId { get; set; }
        public string keyAlias { get; set; }
        public string appName { get; set; }
        public string keyStatus { get; set; }
        public string certificate { get; set; }
        public List<string> certiticateChain { get; set; }
        public string certStatus { get; set; }
        public string certKey { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime expirationDate { get; set; }
        public object emailName { get; set; }
        public object signAlgo { get; set; }
        public object clientId { get; set; }
        public object certName { get; set; }
        public object internationalCertName { get; set; }
        public int certType { get; set; }
        public bool isAutoSign { get; set; }
    }
}