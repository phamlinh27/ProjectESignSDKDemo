using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public class UserSigningSessionGetDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid CertificateId { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }
    }
}
