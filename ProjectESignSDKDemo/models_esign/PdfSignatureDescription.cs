using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class PdfSignatureDescription
    {
        [DataMember(Name = "signedBy", EmitDefaultValue = true)]
        public string SignedBy { get; set; }

        [DataMember(Name = "showSignedDate", EmitDefaultValue = true)]
        public bool ShowSignedDate { get; set; }

        [DataMember(Name = "location", EmitDefaultValue = true)]
        public string Location { get; set; }

        [DataMember(Name = "reason", EmitDefaultValue = true)]
        public string Reason { get; set; }

        [DataMember(Name = "contact", EmitDefaultValue = true)]
        public string Contact { get; set; }

        [DataMember(Name = "displayText", EmitDefaultValue = true)]
        public string DisplayText { get; set; }

        public PdfSignatureDescription(string signedBy = null, bool showSignedDate = false, string location = null, string reason = null, string contact = null, string displayText = null)
        {
            SignedBy = signedBy;
            ShowSignedDate = showSignedDate;
            Location = location;
            Reason = reason;
            Contact = contact;
            DisplayText = displayText;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class PdfSignatureDescription {\n");
            stringBuilder.Append("  SignedBy: ").Append(SignedBy).Append("\n");
            stringBuilder.Append("  ShowSignedDate: ").Append(ShowSignedDate).Append("\n");
            stringBuilder.Append("  Location: ").Append(Location).Append("\n");
            stringBuilder.Append("  Reason: ").Append(Reason).Append("\n");
            stringBuilder.Append("  Contact: ").Append(Contact).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as PdfSignatureDescription);
        }

        public bool Equals(PdfSignatureDescription input)
        {
            if (input == null)
            {
                return false;
            }

            return (SignedBy == input.SignedBy || (SignedBy != null && SignedBy.Equals(input.SignedBy))) && (ShowSignedDate == input.ShowSignedDate || ShowSignedDate.Equals(input.ShowSignedDate)) && (Location == input.Location || (Location != null && Location.Equals(input.Location))) && (Reason == input.Reason || (Reason != null && Reason.Equals(input.Reason))) && (Contact == input.Contact || (Contact != null && Contact.Equals(input.Contact)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (SignedBy != null)
            {
                num = num * 59 + SignedBy.GetHashCode();
            }

            num = num * 59 + ShowSignedDate.GetHashCode();
            if (Location != null)
            {
                num = num * 59 + Location.GetHashCode();
            }

            if (Reason != null)
            {
                num = num * 59 + Reason.GetHashCode();
            }

            if (Contact != null)
            {
                num = num * 59 + Contact.GetHashCode();
            }

            return num;
        }
    }
}
