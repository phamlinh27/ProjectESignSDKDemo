using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class SignatureDto
    {
        [DataMember(Name = "documentId", EmitDefaultValue = true)]
        public string DocumentId { get; set; }

        [DataMember(Name = "signature", EmitDefaultValue = true)]
        public byte[] Signature { get; set; }

        public SignatureDto(string documentId = null, byte[] signature = null)
        {
            DocumentId = documentId;
            Signature = signature;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class SignatureDto {\n");
            stringBuilder.Append("  DocumentId: ").Append(DocumentId).Append("\n");
            stringBuilder.Append("  Signature: ").Append(Signature).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as SignatureDto);
        }

        public bool Equals(SignatureDto input)
        {
            if (input == null)
            {
                return false;
            }

            return (DocumentId == input.DocumentId || (DocumentId != null && DocumentId.Equals(input.DocumentId))) && (Signature == input.Signature || (Signature != null && Signature.Equals(input.Signature)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (DocumentId != null)
            {
                num = num * 59 + DocumentId.GetHashCode();
            }

            if (Signature != null)
            {
                num = num * 59 + Signature.GetHashCode();
            }

            return num;
        }
    }
}
