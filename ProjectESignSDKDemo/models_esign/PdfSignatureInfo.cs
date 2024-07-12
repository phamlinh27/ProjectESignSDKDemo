using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.models_esign
{
    public enum PDFSignatureColor
    {
        Black,
        Red,
        White,
        Blue
    }

    public enum RenderingMode
    {
        Description,
        GraphicAndDescription,
        Graphic
    }
    public class PdfSignatureInfo
    {
        [DataMember(Name = "textColor", EmitDefaultValue = false)]
        public PDFSignatureColor? TextColor { get; set; }

        [DataMember(Name = "renderingMode", EmitDefaultValue = false)]
        public RenderingMode? RenderingMode { get; set; }

        [DataMember(Name = "positionX", EmitDefaultValue = false)]
        public float PositionX { get; set; }

        [DataMember(Name = "positionY", EmitDefaultValue = false)]
        public float PositionY { get; set; }

        [DataMember(Name = "width", EmitDefaultValue = false)]
        public float Width { get; set; }

        [DataMember(Name = "height", EmitDefaultValue = false)]
        public float Height { get; set; }

        [DataMember(Name = "fontSize", EmitDefaultValue = false)]
        public float FontSize { get; set; }

        [DataMember(Name = "fontPath", EmitDefaultValue = false)]
        public string FontPath { get; set; }

        [DataMember(Name = "fontData", EmitDefaultValue = false)]
        public byte[] FontData { get; set; }

        [DataMember(Name = "signatureImage", EmitDefaultValue = true)]
        public string SignatureImage { get; set; }

        [DataMember(Name = "signatureDescription", EmitDefaultValue = false)]
        public PdfSignatureDescription SignatureDescription { get; set; }

        [DataMember(Name = "page", EmitDefaultValue = false)]
        public int Page { get; set; }

        [DataMember(Name = "signatureName", IsRequired = true, EmitDefaultValue = false)]
        public string SignatureName { get; set; }

        [DataMember(Name = "hashAlgorithm", EmitDefaultValue = true)]
        public string HashAlgorithm { get; set; }

        [DataMember(Name = "logoImage", EmitDefaultValue = true)]
        public byte[] LogoImage { get; set; }

        [JsonConstructor]
        protected PdfSignatureInfo()
        {
        }

        public PdfSignatureInfo(float positionX = 0f, float positionY = 0f, float width = 0f, float height = 0f, float fontSize = 0f, PDFSignatureColor? textColor = null, RenderingMode? renderingMode = null, string signatureImage = null, PdfSignatureDescription signatureDescription = null, int page = 0, string signatureName = null, string hashAlgorithm = "SHA256", byte[] logoImage = null)
        {
            SignatureName = signatureName ?? throw new ArgumentNullException("signatureName is a required property for PdfSignatureInfo and cannot be null");
            PositionX = positionX;
            PositionY = positionY;
            Width = width;
            Height = height;
            FontSize = fontSize;
            TextColor = textColor;
            RenderingMode = renderingMode;
            SignatureImage = signatureImage;
            SignatureDescription = signatureDescription;
            Page = page;
            HashAlgorithm = hashAlgorithm;
            LogoImage = logoImage;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("class PdfSignatureInfo {\n");
            stringBuilder.Append("  PositionX: ").Append(PositionX).Append("\n");
            stringBuilder.Append("  PositionY: ").Append(PositionY).Append("\n");
            stringBuilder.Append("  Width: ").Append(Width).Append("\n");
            stringBuilder.Append("  Height: ").Append(Height).Append("\n");
            stringBuilder.Append("  FontSize: ").Append(FontSize).Append("\n");
            stringBuilder.Append("  TextColor: ").Append(TextColor).Append("\n");
            stringBuilder.Append("  RenderingMode: ").Append(RenderingMode).Append("\n");
            stringBuilder.Append("  SignatureImage: ").Append(SignatureImage).Append("\n");
            stringBuilder.Append("  SignatureDescription: ").Append(SignatureDescription).Append("\n");
            stringBuilder.Append("  Page: ").Append(Page).Append("\n");
            stringBuilder.Append("  SignatureName: ").Append(SignatureName).Append("\n");
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override bool Equals(object input)
        {
            return Equals(input as PdfSignatureInfo);
        }

        public bool Equals(PdfSignatureInfo input)
        {
            if (input == null)
            {
                return false;
            }

            return (PositionX == input.PositionX || PositionX.Equals(input.PositionX)) && (PositionY == input.PositionY || PositionY.Equals(input.PositionY)) && (Width == input.Width || Width.Equals(input.Width)) && (Height == input.Height || Height.Equals(input.Height)) && (FontSize == input.FontSize || FontSize.Equals(input.FontSize)) && (TextColor == input.TextColor || TextColor.Equals(input.TextColor)) && (RenderingMode == input.RenderingMode || RenderingMode.Equals(input.RenderingMode)) && (SignatureImage == input.SignatureImage || (SignatureImage != null && SignatureImage.Equals(input.SignatureImage))) && (SignatureDescription == input.SignatureDescription || (SignatureDescription != null && SignatureDescription.Equals(input.SignatureDescription))) && (Page == input.Page || Page.Equals(input.Page)) && (SignatureName == input.SignatureName || (SignatureName != null && SignatureName.Equals(input.SignatureName)));
        }

        public override int GetHashCode()
        {
            int num = 41;
            num = num * 59 + PositionX.GetHashCode();
            num = num * 59 + PositionY.GetHashCode();
            num = num * 59 + Width.GetHashCode();
            num = num * 59 + Height.GetHashCode();
            num = num * 59 + FontSize.GetHashCode();
            num = num * 59 + TextColor.GetHashCode();
            num = num * 59 + RenderingMode.GetHashCode();
            if (SignatureImage != null)
            {
                num = num * 59 + SignatureImage.GetHashCode();
            }

            if (SignatureDescription != null)
            {
                num = num * 59 + SignatureDescription.GetHashCode();
            }

            num = num * 59 + Page.GetHashCode();
            if (SignatureName != null)
            {
                num = num * 59 + SignatureName.GetHashCode();
            }

            return num;
        }
    }
}
