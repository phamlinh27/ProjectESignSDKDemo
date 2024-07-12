using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class SignHashDataV2Dto
    {
        [DataMember(Name = "name", EmitDefaultValue = true)]
        public string Name { get; set; }

        [DataMember(Name = "hash", EmitDefaultValue = true)]
        public byte[] Hash { get; set; }

        public SignHashDataV2Dto(string name = null, byte[] hash = null)
        {
            Name = name;
            Hash = hash;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class SignHashDataV2Dto {\n");
            stringBuilder.Append("  Name: ").Append(Name).Append("\n");
            stringBuilder.Append("  Hash: ").Append(Hash).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as SignHashDataV2Dto);
        }

        public bool Equals(SignHashDataV2Dto input)
        {
            if (input == null)
            {
                return false;
            }

            return (Name == input.Name || (Name != null && Name.Equals(input.Name))) && (Hash == input.Hash || (Hash != null && Hash.Equals(input.Hash)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            if (Name != null)
            {
                num = num * 59 + Name.GetHashCode();
            }

            if (Hash != null)
            {
                num = num * 59 + Hash.GetHashCode();
            }

            return num;
        }
    }
}
