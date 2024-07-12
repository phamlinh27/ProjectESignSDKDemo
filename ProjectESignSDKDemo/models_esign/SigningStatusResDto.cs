using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System;
using System.Linq;

namespace ProjectESignSDKDemo.models_esign
{
    public class SigningStatusResDto : IEquatable<SigningStatusResDto>
    {
        [DataMember(Name = "status", EmitDefaultValue = true)]
        public string Status { get; set; }

        [DataMember(Name = "transactionId", EmitDefaultValue = true)]
        public string TransactionId { get; set; }

        [DataMember(Name = "signatures", EmitDefaultValue = true)]
        public List<SignatureDto> Signatures { get; set; }

        [DataMember(Name = "errorCode", EmitDefaultValue = true)]
        public string ErrorCode { get; set; }

        [DataMember(Name = "errorDescription", EmitDefaultValue = true)]
        public string ErrorDescription { get; set; }

        [DataMember(Name = "extraData", EmitDefaultValue = true)]
        public Dictionary<string, string> ExtraData { get; set; }

        public SigningStatusResDto(string status = null, string transactionId = null, List<SignatureDto> signatures = null, string errorCode = null, string errorDescription = null, Dictionary<string, string> extraData = null)
        {
            Status = status;
            TransactionId = transactionId;
            Signatures = signatures;
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
            ExtraData = extraData;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class SigningStatusResDto {\n");
            stringBuilder.Append("  Status: ").Append(Status).Append("\n");
            stringBuilder.Append("  TransactionId: ").Append(TransactionId).Append("\n");
            stringBuilder.Append("  Signatures: ").Append(Signatures).Append("\n");
            stringBuilder.Append("  ExtraData: ").Append(ExtraData).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as SigningStatusResDto);
        }

        public bool Equals(SigningStatusResDto input)
        {
            if (input == null)
            {
                return false;
            }

            return (Status == input.Status || (Status != null && Status.Equals(input.Status))) && (TransactionId == input.TransactionId || (TransactionId != null && TransactionId.Equals(input.TransactionId))) && (Signatures == input.Signatures || (Signatures != null && input.Signatures != null && Signatures.SequenceEqual(input.Signatures))) && (ExtraData == input.ExtraData || (ExtraData != null && input.ExtraData != null && ExtraData.SequenceEqual(input.ExtraData)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (Status != null)
            {
                num = num * 59 + Status.GetHashCode();
            }

            if (TransactionId != null)
            {
                num = num * 59 + TransactionId.GetHashCode();
            }

            if (Signatures != null)
            {
                num = num * 59 + Signatures.GetHashCode();
            }

            if (ExtraData != null)
            {
                num = num * 59 + ExtraData.GetHashCode();
            }

            return num;
        }
    }
}
