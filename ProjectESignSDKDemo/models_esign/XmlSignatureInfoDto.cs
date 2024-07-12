using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ProjectESignSDKDemo.models_esign
{
    public class XmlSignatureInfoDto
    {
        [DataMember(Name = "id", EmitDefaultValue = true)]
        public string Id { get; set; }

        [DataMember(Name = "nodeToSign", IsRequired = true, EmitDefaultValue = false)]
        public List<string> NodeToSign { get; set; }

        [DataMember(Name = "signatureLocation", EmitDefaultValue = true)]
        public string SignatureLocation { get; set; }

        [DataMember(Name = "showSigningTime", EmitDefaultValue = true)]
        public bool ShowSigningTime { get; set; }

        [DataMember(Name = "TimeNodeId", EmitDefaultValue = true)]
        public string TimeNodeId { get; set; }

        [DataMember(Name = "HashAlgorithm", EmitDefaultValue = true)]
        public string HashAlgorithm { get; set; }

        public XmlSignatureInfoDto(string id = null, List<string> nodeToSign = null, string signatureLocation = null, bool showSigningTime = false, string timeNodeId = null, string hashAlgorithm = "SHA-256")
        {
            NodeToSign = nodeToSign ?? throw new ArgumentNullException("nodeToSign is a required property for XmlSignatureInfoDto and cannot be null");
            Id = id;
            SignatureLocation = signatureLocation;
            HashAlgorithm = hashAlgorithm;
            ShowSigningTime = showSigningTime;
            TimeNodeId = timeNodeId;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class XmlSignatureInfoDto {\n");
            stringBuilder.Append("  Id: ").Append(Id).Append("\n");
            stringBuilder.Append("  NodeToSign: ").Append(NodeToSign).Append("\n");
            stringBuilder.Append("  SignatureLocation: ").Append(SignatureLocation).Append("\n");
            stringBuilder.Append("  ShowSigningTime: ").Append(ShowSigningTime).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as XmlSignatureInfoDto);
        }

        public bool Equals(XmlSignatureInfoDto input)
        {
            if (input == null)
            {
                return false;
            }

            return (Id == input.Id || (Id != null && Id.Equals(input.Id))) && (NodeToSign == input.NodeToSign || (NodeToSign != null && NodeToSign.Equals(input.NodeToSign))) && (SignatureLocation == input.SignatureLocation || (SignatureLocation != null && SignatureLocation.Equals(input.SignatureLocation)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (Id != null)
            {
                num = num * 59 + Id.GetHashCode();
            }

            if (NodeToSign != null)
            {
                num = num * 59 + NodeToSign.GetHashCode();
            }

            if (SignatureLocation != null)
            {
                num = num * 59 + SignatureLocation.GetHashCode();
            }

            return num;
        }
    }
}
