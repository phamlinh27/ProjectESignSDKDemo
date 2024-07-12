using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class SignHashResDto
    {
        [DataMember(Name = "signingAuthorizeTimeout", EmitDefaultValue = true)]
        public int SigningAuthorizeTimeout { get; set; }

        [DataMember(Name = "transactionId", EmitDefaultValue = true)]
        public string TransactionId { get; set; }

        [DataMember(Name = "signatures", EmitDefaultValue = true)]
        public List<SignatureDto> Signatures { get; set; }

        [DataMember(Name = "errorCode", EmitDefaultValue = true)]
        public string ErrorCode { get; set; }

        [DataMember(Name = "errorDescription", EmitDefaultValue = true)]
        public string ErrorDescription { get; set; }

        [DataMember(Name = "status", EmitDefaultValue = true)]
        public string Status { get; set; }

        [DataMember(Name = "userSigningSession", EmitDefaultValue = true)]
        public UserSigningSessionGetDto UserSigningSession { get; set; }

        [DataMember(Name = "sessionSigningEnabled", EmitDefaultValue = true)]
        public bool SessionSigningEnabled { get; set; }

        public SignHashResDto(string transactionId = null, List<SignatureDto> signatures = null, string errorCode = null, string errorDescription = null, string status = null, int signingAuthorizeTimeout = 120, UserSigningSessionGetDto userSigningSession = null, bool sessionSigningEnabled = false)
        {
            TransactionId = transactionId;
            SigningAuthorizeTimeout = signingAuthorizeTimeout;
            Signatures = signatures;
            ErrorDescription = errorDescription;
            Status = status;
            ErrorCode = errorCode;
            UserSigningSession = userSigningSession;
            SessionSigningEnabled = sessionSigningEnabled;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class SignHashResDto {\n");
            stringBuilder.Append("  TransactionId: ").Append(TransactionId).Append("\n");
            stringBuilder.Append("  Signatures: ").Append(Signatures).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as SignHashResDto);
        }

        public bool Equals(SignHashResDto input)
        {
            if (input == null)
            {
                return false;
            }

            return (TransactionId == input.TransactionId || (TransactionId != null && TransactionId.Equals(input.TransactionId))) && (Signatures == input.Signatures || (Signatures != null && input.Signatures != null && Signatures.SequenceEqual(input.Signatures)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (TransactionId != null)
            {
                num = num * 59 + TransactionId.GetHashCode();
            }

            if (Signatures != null)
            {
                num = num * 59 + Signatures.GetHashCode();
            }

            return num;
        }
    }
}
