using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class SignHashV2Dto
    {
        [DataMember(Name = "credentialId", IsRequired = true, EmitDefaultValue = false)]
        public string CredentialId { get; set; }

        [DataMember(Name = "refTranId", EmitDefaultValue = true)]
        public string RefTranId { get; set; }

        [DataMember(Name = "notifyUrl", EmitDefaultValue = true)]
        public string NotifyUrl { get; set; }

        [DataMember(Name = "description", EmitDefaultValue = true)]
        public string Description { get; set; }

        [DataMember(Name = "datas", IsRequired = true, EmitDefaultValue = false)]
        public List<SignHashDataV2Dto> Datas { get; set; }

        [DataMember(Name = "documentName", IsRequired = true, EmitDefaultValue = false)]
        public string DocumentName { get; set; }

        [DataMember(Name = "documentDetail", IsRequired = true, EmitDefaultValue = false)]
        public string DocumentDetail { get; set; }

        [JsonConstructor]
        protected SignHashV2Dto()
        {
        }

        public SignHashV2Dto(string credentialId = null, string refTranId = null, string notifyUrl = null, string description = null, List<SignHashDataV2Dto> datas = null, string documentName = null, string documentDetail = null)
        {
            if (credentialId == null)
            {
                throw new ArgumentNullException("credentialId is a required property for SignHashV2Dto and cannot be null");
            }

            CredentialId = credentialId;
            if (datas == null)
            {
                throw new ArgumentNullException("datas is a required property for SignHashV2Dto and cannot be null");
            }

            Datas = datas;
            RefTranId = refTranId;
            NotifyUrl = notifyUrl;
            Description = description;
            DocumentName = documentName ?? throw new ArgumentNullException("documentName is a required property for SignHashDto and cannot be null");
            DocumentDetail = documentDetail;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class SignHashV2Dto {\n");
            stringBuilder.Append("  CredentialId: ").Append(CredentialId).Append("\n");
            stringBuilder.Append("  RefTranId: ").Append(RefTranId).Append("\n");
            stringBuilder.Append("  NotifyUrl: ").Append(NotifyUrl).Append("\n");
            stringBuilder.Append("  Description: ").Append(Description).Append("\n");
            stringBuilder.Append("  Datas: ").Append(Datas).Append("\n");
            stringBuilder.Append("  DocumentName: ").Append(DocumentName).Append("\n");
            stringBuilder.Append("  DocumentDetail: ").Append(DocumentDetail).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as SignHashV2Dto);
        }

        public bool Equals(SignHashV2Dto input)
        {
            if (input == null)
            {
                return false;
            }

            return (CredentialId == input.CredentialId || (CredentialId != null && CredentialId.Equals(input.CredentialId))) && (RefTranId == input.RefTranId || (RefTranId != null && RefTranId.Equals(input.RefTranId))) && (NotifyUrl == input.NotifyUrl || (NotifyUrl != null && NotifyUrl.Equals(input.NotifyUrl))) && (Description == input.Description || (Description != null && Description.Equals(input.Description))) && (Datas == input.Datas || (Datas != null && input.Datas != null && Datas.SequenceEqual(input.Datas))) && (DocumentName == input.DocumentName || (DocumentName != null && DocumentName.Equals(input.DocumentName))) && (DocumentDetail == input.DocumentDetail || (DocumentDetail != null && DocumentDetail.Equals(input.DocumentDetail)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (CredentialId != null)
            {
                num = num * 59 + CredentialId.GetHashCode();
            }

            if (RefTranId != null)
            {
                num = num * 59 + RefTranId.GetHashCode();
            }

            if (NotifyUrl != null)
            {
                num = num * 59 + NotifyUrl.GetHashCode();
            }

            if (Description != null)
            {
                num = num * 59 + Description.GetHashCode();
            }

            if (Datas != null)
            {
                num = num * 59 + Datas.GetHashCode();
            }

            if (DocumentName != null)
            {
                num = num * 59 + DocumentName.GetHashCode();
            }

            if (DocumentDetail != null)
            {
                num = num * 59 + DocumentDetail.GetHashCode();
            }

            return num;
        }
    }
}
