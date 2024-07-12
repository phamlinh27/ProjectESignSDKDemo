using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using iText.Signatures;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using ProjectESignSDKDemo.models;
using ProjectESignSDKDemo.models_esign;
using System.Collections.Generic;
using System;
using System.IO;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
using PdfPKCS7 = iText.Signatures.PdfPKCS7;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;

namespace ProjectESignSDKDemo.action
{
    internal class CommonFunction
    {
        private static readonly byte[] _sha1Prefix;

        private static readonly byte[] _sha256Prefix;

        private static readonly byte[] _sha384Prefix;

        private static readonly byte[] _sha512Prefix;

        static CommonFunction()
        {
            _sha1Prefix = new byte[15]
            {
         48, 33, 48, 9, 6, 5, 43, 14, 3, 2,
         26, 5, 0, 4, 20
            };
            _sha256Prefix = new byte[19]
            {
         48, 49, 48, 13, 6, 9, 96, 134, 72, 1,
         101, 3, 4, 2, 1, 5, 0, 4, 32
            };
            _sha384Prefix = new byte[19]
            {
         48, 65, 48, 13, 6, 9, 96, 134, 72, 1,
         101, 3, 4, 2, 2, 5, 0, 4, 48
            };
            _sha512Prefix = new byte[19]
            {
         48, 81, 48, 13, 6, 9, 96, 134, 72, 1,
         101, 3, 4, 2, 3, 5, 0, 4, 64
            };
        }

        public CalculateLocalHashResult CalculateLocalHash(CalculateLocalHashData data)
        {
            CalculateLocalHashResult calculateLocalHashResult = new CalculateLocalHashResult
            {
                DocumentType = data.DocumentType
            };
            switch (data.DocumentType)
            {
                case DocumentType.Pdf:
                    calculateLocalHashResult.PdfDocs = PreSignPdf(data.PdfDocs, data.Certificate, data.CertificateChain);
                    break;
                case DocumentType.Xml:
                    calculateLocalHashResult.XmlDocs = PreSignXml(data.XmlDocs, data.Certificate);
                    break;
                case DocumentType.Excel:
                    calculateLocalHashResult.ExcelDocs = PreSignExcel(data.ExcelDocs, data.Certificate, data.CertificateChain);
                    break;
                case DocumentType.Word:
                    calculateLocalHashResult.WordDocs = PreSignWord(data.WordDocs, data.Certificate, data.CertificateChain);
                    break;
            }

            return calculateLocalHashResult;
        }

        private static List<PdfDocToHashResult> PreSignPdf(List<PdfDocToHash> docs, byte[] certificateData, List<string> certificateChain)
        {

            // Khởi tạo 1 list X509Certificate
            List<X509Certificate> list = new List<X509Certificate>();
            if (certificateChain != null)
            {
                foreach (string item2 in certificateChain)
                {
                    X509Certificate2 x509Certificate = new X509Certificate2(Convert.FromBase64String(item2));
                    X509Certificate item = new X509CertificateParser().ReadCertificate(x509Certificate.GetRawCertData());
                    list.Add(item);
                }
            }

            X509Certificate2 x509Certificate2 = new X509Certificate2(certificateData);
            X509Certificate x509Certificate3 = new X509CertificateParser().ReadCertificate(x509Certificate2.GetRawCertData());
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (list.Count == 0)
            {
                list.Add(x509Certificate3);
            }

            List<PdfDocToHashResult> list2 = new List<PdfDocToHashResult>();
            foreach (PdfDocToHash doc in docs)
            {
                using (PdfReader reader = new PdfReader(doc.FileToSign))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfStamper pdfStamper = PdfStamper.CreateSignature(reader, memoryStream, '\0', null, append: true);
                        iTextSharp.text.pdf.PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
                        iTextSharp.text.Rectangle rectangle = new iTextSharp.text.Rectangle(doc.SignatureInfo.PositionX, doc.SignatureInfo.PositionY, doc.SignatureInfo.PositionX + doc.SignatureInfo.Width, doc.SignatureInfo.PositionY + doc.SignatureInfo.Height);

                        signatureAppearance.SetVisibleSignature(rectangle, doc.SignatureInfo.Page, doc.SignatureInfo.SignatureName);
                        float num = ((doc.SignatureInfo.FontSize == 0f) ? 8f : doc.SignatureInfo.FontSize);
                        iTextSharp.text.Font font;

                        var pathToTemplate = Directory.GetCurrentDirectory() +
                                      Path.DirectorySeparatorChar.ToString() +
                                      "VietNamese_Font\\" +
                                      "Hoa_Sen_Typeface.ttf";

                        doc.SignatureInfo.FontData = System.IO.File.ReadAllBytes(pathToTemplate);
                        if (!string.IsNullOrEmpty(doc.SignatureInfo.FontPath))
                        {
                            string fontPath = doc.SignatureInfo.FontPath;
                            BaseFont baseFont = BaseFont.CreateFont(fontPath, "Identity-H", embedded: true);
                            baseFont.Subset = true;
                            font = new iTextSharp.text.Font(baseFont, num, 0);
                            switch (doc.SignatureInfo.TextColor)
                            {
                                case PDFSignatureColor.Red:
                                    font.Color = BaseColor.RED;
                                    break;
                                case PDFSignatureColor.Black:
                                    font.Color = BaseColor.BLACK;
                                    break;
                                case PDFSignatureColor.Blue:
                                    font.Color = BaseColor.BLUE;
                                    break;
                                case PDFSignatureColor.White:
                                    font.Color = BaseColor.WHITE;
                                    break;
                                default:
                                    font.Color = BaseColor.BLACK;
                                    break;
                            }

                            signatureAppearance.Layer2Font = font;
                        }
                        else
                        {
                            if (doc.SignatureInfo.FontData != null)
                            {
                                string name = "customFont.ttf";
                                BaseFont baseFont2 = BaseFont.CreateFont(name, "Identity-H", embedded: true, cached: true, doc.SignatureInfo.FontData, null);
                                baseFont2.Subset = true;
                                font = new iTextSharp.text.Font(baseFont2, num, 0);
                                switch (doc.SignatureInfo.TextColor)
                                {
                                    case PDFSignatureColor.Red:
                                        font.Color = BaseColor.RED;
                                        break;
                                    case PDFSignatureColor.Black:
                                        font.Color = BaseColor.BLACK;
                                        break;
                                    case PDFSignatureColor.Blue:
                                        font.Color = BaseColor.BLUE;
                                        break;
                                    case PDFSignatureColor.White:
                                        font.Color = BaseColor.WHITE;
                                        break;
                                    default:
                                        font.Color = BaseColor.BLACK;
                                        break;
                                }

                                signatureAppearance.Layer2Font = font;
                            }

                            font = signatureAppearance.Layer2Font;
                            if (font != null)
                            {
                                signatureAppearance.Layer2Font.Size = num;
                                switch (doc.SignatureInfo.TextColor)
                                {
                                    case PDFSignatureColor.Red:
                                        font.Color = BaseColor.RED;
                                        break;
                                    case PDFSignatureColor.Black:
                                        font.Color = BaseColor.BLACK;
                                        break;
                                    case PDFSignatureColor.Blue:
                                        font.Color = BaseColor.BLUE;
                                        break;
                                    case PDFSignatureColor.White:
                                        font.Color = BaseColor.WHITE;
                                        break;
                                    default:
                                        font.Color = BaseColor.BLACK;
                                        break;
                                }
                            }
                        }

                        bool flag = doc.SignatureInfo.LogoImage != null;
                        MemoryStream memoryStream2 = null;
                        if (flag)
                        {
                            memoryStream2 = new MemoryStream(doc.SignatureInfo.LogoImage);
                            Bitmap bitmap = new Bitmap(memoryStream2);
                            bitmap.MakeTransparent(Color.White);
                            bitmap.SetResolution(500f, 500f);
                            bitmap.Save(memoryStream2, ImageFormat.Png);
                        }

                        switch (doc.SignatureInfo.RenderingMode)
                        {
                            default:
                                {
                                    signatureAppearance.SignatureRenderingMode = iTextSharp.text.pdf.PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                                    string text = "";
                                    if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.DisplayText))
                                    {
                                        text = doc.SignatureInfo.SignatureDescription.DisplayText;
                                    }
                                    else if (doc.SignatureInfo.SignatureDescription != null)
                                    {
                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.SignedBy))
                                        {
                                            text = text + "Ký số bởi: " + doc.SignatureInfo.SignatureDescription.SignedBy + "\r\n";
                                        }

                                        if (doc.SignatureInfo.SignatureDescription.ShowSignedDate)
                                        {
                                            text = text + "Ngày ký: " + signatureAppearance.SignDate.ToString("yyyy-MM-dd HH:mm:ss zzz") + "\r\n";
                                        }

                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Contact))
                                        {
                                            text = text + "Liên hệ: " + doc.SignatureInfo.SignatureDescription.Contact + "\r\n";
                                            signatureAppearance.Contact = doc.SignatureInfo.SignatureDescription.Contact;
                                        }

                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Reason))
                                        {
                                            text = text + "Lý do: " + doc.SignatureInfo.SignatureDescription.Reason + "\r\n";
                                            signatureAppearance.Reason = doc.SignatureInfo.SignatureDescription.Reason;
                                        }

                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Location))
                                        {
                                            text = text + "Nơi ký: " + doc.SignatureInfo.SignatureDescription.Location;
                                            signatureAppearance.Location = doc.SignatureInfo.SignatureDescription.Location;
                                        }
                                    }

                                    PdfTemplate layer2 = signatureAppearance.GetLayer(2);
                                    ColumnText columnText2 = new ColumnText(layer2);
                                    columnText2.SetSimpleColumn(rectangle.Width / 2f, 0f, rectangle.Width, rectangle.Height);
                                    PdfPTable pdfPTable2 = new PdfPTable(1);
                                    pdfPTable2.SetWidths(new float[1] { rectangle.Width / 2f });
                                    pdfPTable2.SetTotalWidth(new float[1] { rectangle.Width / 2f });
                                    pdfPTable2.LockedWidth = true;
                                    string text2 = text;
                                    Paragraph paragraph = new Paragraph(text2);
                                    paragraph.Font = font;
                                    paragraph.Alignment = 1;
                                    columnText2.AddElement(paragraph);
                                    int status = columnText2.Go(simulate: true);
                                    bool flag2 = !ColumnText.HasMoreText(status);
                                    if (flag2)
                                    {
                                        ColumnText columnText3 = new ColumnText(layer2);
                                        columnText3.SetSimpleColumn(rectangle.Width / 2f + 5f, 0f, 0f, rectangle.Height);
                                        PdfPTable element = DrawTable(rectangle.Width, rectangle.Height, text2, flag, font.Size, memoryStream2?.ToArray(), font);
                                        columnText3.AddElement(element);
                                        columnText3.Go();
                                        break;
                                    }

                                    while (!flag2 && num > 1f)
                                    {
                                        num -= 0.1f;
                                        font.Size = num;
                                        ColumnText columnText4 = new ColumnText(layer2);
                                        columnText4.SetSimpleColumn(rectangle.Width / 2f + 5f, 0f, 0f, rectangle.Height);
                                        Paragraph paragraph2 = new Paragraph(text2);
                                        paragraph2.Font = font;
                                        paragraph2.Alignment = 5;
                                        columnText4.AddElement(paragraph2);
                                        int status2 = columnText4.Go(simulate: true);
                                        flag2 = !ColumnText.HasMoreText(status2);
                                    }

                                    ColumnText columnText5 = new ColumnText(layer2);
                                    columnText5.SetSimpleColumn(rectangle.Width / 2f + 5f, 0f, 0f, rectangle.Height);
                                    PdfPTable element2 = DrawTable(rectangle.Width, rectangle.Height, text2, flag, font.Size, memoryStream2?.ToArray(), font);
                                    columnText5.AddElement(element2);
                                    columnText5.Go();
                                    break;
                                }
                            case RenderingMode.GraphicAndDescription:
                                {
                                    iTextSharp.text.Image instance2 = iTextSharp.text.Image.GetInstance(Convert.FromBase64String(doc.SignatureInfo.SignatureImage));
                                    string text3 = "";
                                    if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.DisplayText))
                                    {
                                        text3 = doc.SignatureInfo.SignatureDescription.DisplayText;
                                    }
                                    else if (doc.SignatureInfo.SignatureDescription != null)
                                    {
                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.SignedBy))
                                        {
                                            text3 = text3 + "Ký số bởi: " + doc.SignatureInfo.SignatureDescription.SignedBy + "\r\n";
                                        }

                                        if (doc.SignatureInfo.SignatureDescription.ShowSignedDate)
                                        {
                                            text3 = text3 + "Ngày ký: " + signatureAppearance.SignDate.ToString("yyyy-MM-dd HH:mm:ss zzz") + "\r\n";
                                        }

                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Contact))
                                        {
                                            text3 = text3 + "Liên hệ: " + doc.SignatureInfo.SignatureDescription.Contact + "\r\n";
                                            signatureAppearance.Contact = doc.SignatureInfo.SignatureDescription.Contact;
                                        }

                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Reason))
                                        {
                                            text3 = text3 + "Lý do: " + doc.SignatureInfo.SignatureDescription.Reason + "\r\n";
                                            signatureAppearance.Reason = doc.SignatureInfo.SignatureDescription.Reason;
                                        }

                                        if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Location))
                                        {
                                            text3 = text3 + "Nơi ký: " + doc.SignatureInfo.SignatureDescription.Location;
                                            signatureAppearance.Location = doc.SignatureInfo.SignatureDescription.Location;
                                        }
                                    }

                                    PdfTemplate layer3 = signatureAppearance.GetLayer(2);
                                    int num4 = 5;
                                    float num5 = rectangle.Width / 2f - (float)num4;
                                    ColumnText columnText6 = new ColumnText(layer3);
                                    instance2.ScaleToFit(num5, rectangle.Height);
                                    instance2.SetAbsolutePosition(doc.SignatureInfo.PositionX, doc.SignatureInfo.PositionY);
                                    PdfPTable pdfPTable3 = new PdfPTable(1);
                                    pdfPTable3.SetTotalWidth(new float[1] { num5 });
                                    pdfPTable3.SetWidths(new float[1] { num5 });
                                    pdfPTable3.LockedWidth = true;
                                    pdfPTable3.AddCell(new PdfPCell(instance2)
                                    {
                                        HorizontalAlignment = 0,
                                        VerticalAlignment = 5,
                                        Border = 0,
                                        Padding = 0f,
                                        FixedHeight = rectangle.Height
                                    });
                                    columnText6.SetSimpleColumn(0f, 0f, num5, rectangle.Height);
                                    columnText6.AddElement(pdfPTable3);
                                    columnText6.Go();
                                    ColumnText columnText7 = new ColumnText(layer3);
                                    columnText7.SetSimpleColumn(rectangle.Width / 2f, 0f, rectangle.Width, rectangle.Height);
                                    PdfPTable pdfPTable4 = new PdfPTable(1);
                                    pdfPTable4.SetWidths(new float[1] { rectangle.Width / 2f });
                                    pdfPTable4.SetTotalWidth(new float[1] { rectangle.Width / 2f });
                                    pdfPTable4.LockedWidth = true;
                                    string empty = string.Empty;
                                    string empty2 = string.Empty;
                                    string empty3 = string.Empty;
                                    DateTime now = DateTime.Now;
                                    string text4 = now.ToString("dd/MM/yyyy");
                                    string text5 = now.ToString("HH:mm:ss");
                                    string empty4 = string.Empty;
                                    string text6 = text3;
                                    Paragraph paragraph3 = new Paragraph(text6);
                                    paragraph3.Font = font;
                                    paragraph3.Alignment = 1;
                                    columnText7.AddElement(paragraph3);
                                    int status3 = columnText7.Go(simulate: true);
                                    bool flag3 = !ColumnText.HasMoreText(status3);
                                    if (flag3)
                                    {
                                        ColumnText columnText8 = new ColumnText(layer3);
                                        columnText8.SetSimpleColumn(instance2.Width + (rectangle.Width / 2f - instance2.Width), 0f, rectangle.Width, rectangle.Height);
                                        PdfPTable element3 = DrawTable(rectangle.Width, rectangle.Height, text6, flag, font.Size, memoryStream2?.ToArray(), font);
                                        columnText8.AddElement(element3);
                                        columnText8.Go();
                                        break;
                                    }

                                    while (!flag3 && num > 1f)
                                    {
                                        num -= 0.1f;
                                        font.Size = num;
                                        ColumnText columnText9 = new ColumnText(layer3);
                                        columnText9.SetSimpleColumn(instance2.Width + (rectangle.Width / 2f - instance2.Width), 0f, rectangle.Width, rectangle.Height);
                                        Paragraph paragraph4 = new Paragraph(text6);
                                        paragraph4.Font = font;
                                        paragraph4.Alignment = 5;
                                        columnText9.AddElement(paragraph4);
                                        int status4 = columnText9.Go(simulate: true);
                                        flag3 = !ColumnText.HasMoreText(status4);
                                    }

                                    ColumnText columnText10 = new ColumnText(layer3);
                                    columnText10.SetSimpleColumn(instance2.Width + (rectangle.Width / 2f - instance2.Width), 0f, rectangle.Width, rectangle.Height);
                                    PdfPTable element4 = DrawTable(rectangle.Width, rectangle.Height, text6, flag, font.Size, memoryStream2?.ToArray(), font);
                                    columnText10.AddElement(element4);
                                    columnText10.Go();
                                    break;
                                }
                            case RenderingMode.Graphic:
                                {
                                    if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Contact))
                                    {
                                        signatureAppearance.Contact = doc.SignatureInfo.SignatureDescription.Contact;
                                    }

                                    if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Reason))
                                    {
                                        signatureAppearance.Reason = doc.SignatureInfo.SignatureDescription.Reason;
                                    }

                                    if (!string.IsNullOrEmpty(doc.SignatureInfo.SignatureDescription.Location))
                                    {
                                        signatureAppearance.Location = doc.SignatureInfo.SignatureDescription.Location;
                                    }

                                    iTextSharp.text.Image image = (signatureAppearance.SignatureGraphic = iTextSharp.text.Image.GetInstance(Convert.FromBase64String(doc.SignatureInfo.SignatureImage)));
                                    signatureAppearance.SignatureRenderingMode = iTextSharp.text.pdf.PdfSignatureAppearance.RenderingMode.GRAPHIC;
                                    PdfTemplate layer = signatureAppearance.GetLayer(2);
                                    int num2 = 5;
                                    float num3 = rectangle.Width / 2f - (float)num2;
                                    ColumnText columnText = new ColumnText(layer);
                                    image.ScaleToFit(num3, rectangle.Height);
                                    image.SetAbsolutePosition(doc.SignatureInfo.PositionX, doc.SignatureInfo.PositionY);
                                    PdfPTable pdfPTable = new PdfPTable(1);
                                    pdfPTable.SetTotalWidth(new float[1] { num3 });
                                    pdfPTable.SetWidths(new float[1] { num3 });
                                    pdfPTable.LockedWidth = true;
                                    pdfPTable.AddCell(new PdfPCell(image)
                                    {
                                        HorizontalAlignment = 0,
                                        VerticalAlignment = 5,
                                        Border = 0,
                                        Padding = 0f,
                                        FixedHeight = rectangle.Height
                                    });
                                    columnText.SetSimpleColumn(0f, 0f, num3, rectangle.Height);
                                    columnText.AddElement(pdfPTable);
                                    columnText.Go();
                                    break;
                                }
                        }

                        memoryStream2?.Dispose();
                        signatureAppearance.Acro6Layers = true;
                        signatureAppearance.Certificate = x509Certificate3;
                        string text7 = ((!string.IsNullOrEmpty(doc.SignatureInfo.HashAlgorithm)) ? doc.SignatureInfo.HashAlgorithm : "SHA256");
                        PreSigning preSigning = new PreSigning(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED, text7);
                        MakeSignature.SignExternalContainer(signatureAppearance, preSigning, 61440);
                        PdfPKCS7 pdfPKCS = new PdfPKCS7(null, list.ToArray(), text7, false);
                        IDigest digest = DigestUtilities.GetDigest(text7);
                        byte[] authenticatedAttributeBytes = pdfPKCS.GetAuthenticatedAttributeBytes(preSigning.GetHash(), PdfSigner.CryptoStandard.CMS, null, null);
                        byte[] sh = (byte[])authenticatedAttributeBytes.Clone();
                        digest.BlockUpdate(authenticatedAttributeBytes, 0, authenticatedAttributeBytes.Length);
                        authenticatedAttributeBytes = new byte[digest.GetDigestSize()];
                        digest.DoFinal(authenticatedAttributeBytes, 0);
                        byte[] array = new byte[19]
                        {
                48, 49, 48, 13, 6, 9, 96, 134, 72, 1,
                101, 3, 4, 2, 1, 5, 0, 4, 32
                        };
                        byte[] array2 = new byte[array.Length + authenticatedAttributeBytes.Length];
                        array.CopyTo(array2, 0);
                        authenticatedAttributeBytes.CopyTo(array2, array.Length);
                        list2.Add(new PdfDocToHashResult
                        {
                            DocumentId = doc.DocumentId,
                            Digest = array2,
                            Sh = sh,
                            DocumentHash = preSigning.GetHash(),
                            DocumentBytes = memoryStream.ToArray(),
                            SignatureName = doc.SignatureInfo.SignatureName
                        });
                        pdfStamper.Dispose();
                    }
                }
            }

            return list2;

            //Convert.ToBase64String(list2[0].DocumentHash)
        }

        private static List<XmlDocToHashResult> PreSignXml(List<XmlDocToHash> xmlDocs, byte[] certificateData)
        {
            X509Certificate2 x509Certificate = new X509Certificate2(certificateData);
            List<XmlDocToHashResult> list = new List<XmlDocToHashResult>();
            foreach (XmlDocToHash xmlDoc in xmlDocs)
            {
                XmlDocument xmlDocument = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                xmlDocument.LoadXml(xmlDoc.FileToSign);
                XmlElement xmlElement = xmlDocument.CreateElement("Signature");
                xmlElement.SetAttribute("Id", xmlDoc.SignatureInfo.Id);
                xmlElement.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
                XmlElement xmlElement2 = xmlDocument.CreateElement("SignatureProperties");
                xmlElement2.SetAttribute("xmlns", "");
                XmlElement xmlElement3 = null;
                if (xmlDoc.SignatureInfo.ShowSigningTime)
                {
                    xmlElement3 = xmlDocument.CreateElement("Object");
                    xmlElement3.SetAttribute("Id", xmlDoc.SignatureInfo.TimeNodeId);
                    xmlElement3.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
                    xmlElement3.AppendChild(xmlElement2);
                    XmlElement xmlElement4 = xmlDocument.CreateElement("SigningTime");
                    xmlElement4.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    XmlElement xmlElement5 = xmlDocument.CreateElement("SignatureProperty");
                    xmlElement5.SetAttribute("Target", "#" + xmlDoc.SignatureInfo.Id);
                    xmlElement5.AppendChild(xmlElement4);
                    xmlElement2.AppendChild(xmlElement5);
                    xmlElement.PrependChild(xmlElement3);
                }

                XmlElement xmlElement6 = xmlDocument.CreateElement("SignedInfo");
                xmlElement6.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
                XmlElement newChild = xmlDocument.CreateElement("SignatureValue");
                xmlElement.AppendChild(xmlElement6);
                xmlElement.AppendChild(newChild);
                XmlElement xmlElement7 = xmlDocument.CreateElement("KeyInfo");
                XmlElement xmlElement8 = xmlDocument.CreateElement("X509Data");
                XmlElement xmlElement9 = xmlDocument.CreateElement("X509SubjectName");
                xmlElement9.InnerText = x509Certificate.SubjectName.Name;
                XmlElement xmlElement10 = xmlDocument.CreateElement("X509Certificate");
                xmlElement10.InnerText = Convert.ToBase64String(x509Certificate.RawData);
                xmlElement8.AppendChild(xmlElement9);
                xmlElement8.AppendChild(xmlElement10);
                xmlElement7.AppendChild(xmlElement8);
                xmlElement.AppendChild(xmlElement7);
                if (xmlElement3 != null)
                {
                    xmlElement.AppendChild(xmlElement3);
                }

                XmlElement xmlElement11 = xmlDocument.CreateElement("CanonicalizationMethod");
                xmlElement11.SetAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
                xmlElement6.AppendChild(xmlElement11);
                XmlElement xmlElement12 = xmlDocument.CreateElement("SignatureMethod");
                (string, string) xmlSigAlg = GetXmlSigAlg(xmlDoc.SignatureInfo.HashAlgorithm);
                xmlElement12.SetAttribute("Algorithm", xmlSigAlg.Item1);
                xmlElement6.AppendChild(xmlElement12);
                HashAlgorithm hashAlgorithm = HashAlgorithm.Create(xmlDoc.SignatureInfo.HashAlgorithm);
                List<string> digestedNode = new List<string>();
                foreach (string item in xmlDoc.SignatureInfo.NodeToSign)
                {
                    XmlDsigExcC14NTransform val = new XmlDsigExcC14NTransform();
                    XmlElement xmlElement13;
                    if (item.Contains("xpath:"))
                    {
                        string xpath = item.Replace("xpath:", "");
                        xmlElement13 = (XmlElement)xmlDocument.SelectSingleNode(xpath);
                    }
                    else
                    {
                        string xpath2 = $"//*[@id='{item}']";
                        xmlElement13 = (XmlElement)xmlDocument.SelectSingleNode(xpath2);
                        if (xmlElement13 == null)
                        {
                            xpath2 = $"//*[@Id='{item}']";
                            xmlElement13 = (XmlElement)xmlDocument.SelectSingleNode(xpath2);
                        }
                    }

                    if (xmlElement13 == null)
                    {
                        break;
                    }

                    XmlElement xmlElement14 = xmlDocument.CreateElement("Reference");
                    XmlElement xmlElement15 = xmlDocument.CreateElement("DigestMethod");
                    xmlElement15.SetAttribute("Algorithm", xmlSigAlg.Item2);
                    XmlElement xmlElement16 = xmlDocument.CreateElement("DigestValue");
                    XmlElement xmlElement17 = xmlDocument.CreateElement("Transforms");
                    xmlElement14.AppendChild(xmlElement17);
                    XmlDocument xmlDocument2 = new XmlDocument
                    {
                        PreserveWhitespace = true
                    };
                    using (TextReader input = new StringReader(xmlElement13.OuterXml))
                    {
                        XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                        xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
                        xmlReaderSettings.XmlResolver = new XmlSecureResolver(new XmlUrlResolver(), xmlDocument.BaseURI);
                        XmlReader reader = XmlReader.Create(input, xmlReaderSettings, xmlDocument.BaseURI);
                        xmlDocument2.Load(reader);
                    }

                    CanonicalXmlNodeList propagatedAttributes = XmlHelper.GetPropagatedAttributes(xmlDocument.DocumentElement);
                    XmlHelper.AddNamespaces(xmlDocument2.DocumentElement, propagatedAttributes);
                    if (xmlElement13 == xmlDocument.DocumentElement)
                    {
                        xmlElement14.SetAttribute("URI", "");
                    }
                    else
                    {
                        xmlElement14.SetAttribute("URI", "#" + item);
                    }

                    ((Transform)val).LoadInput((object)xmlDocument2);
                    byte[] digestedOutput = ((Transform)val).GetDigestedOutput(hashAlgorithm);
                    xmlElement16.InnerText = Convert.ToBase64String(digestedOutput);
                    XmlElement xmlElement18 = xmlDocument.CreateElement("Transform");
                    xmlElement18.SetAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#enveloped-signature");
                    XmlElement xmlElement19 = xmlDocument.CreateElement("Transform");
                    xmlElement19.SetAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
                    xmlElement17.AppendChild(xmlElement19);
                    xmlElement17.AppendChild(xmlElement18);
                    xmlElement14.AppendChild(xmlElement15);
                    xmlElement14.AppendChild(xmlElement16);
                    xmlElement6.AppendChild(xmlElement14);
                    digestedNode.Add(item);
                }

                xmlDoc.SignatureInfo.NodeToSign.RemoveAll((string item) => digestedNode.Contains(item));
                foreach (string item2 in xmlDoc.SignatureInfo.NodeToSign)
                {
                    XmlDsigC14NTransform val2 = new XmlDsigC14NTransform();
                    XmlElement xmlElement20;
                    if (item2.Contains("xpath:"))
                    {
                        string xpath3 = item2.Replace("xpath:", "");
                        xmlElement20 = (XmlElement)xmlElement.SelectSingleNode(xpath3);
                    }
                    else
                    {
                        string xpath4 = $"//*[@id='{item2}']";
                        xmlElement20 = (XmlElement)xmlElement.SelectSingleNode(xpath4);
                        if (xmlElement20 == null)
                        {
                            xpath4 = $"//*[@Id='{item2}']";
                            xmlElement20 = (XmlElement)xmlElement.SelectSingleNode(xpath4);
                        }
                    }

                    if (xmlElement20 == null)
                    {
                        break;
                    }

                    XmlElement xmlElement21 = xmlDocument.CreateElement("Reference");
                    xmlElement21.SetAttribute("URI", "#" + item2);
                    XmlElement xmlElement22 = xmlDocument.CreateElement("DigestMethod");
                    xmlElement22.SetAttribute("Algorithm", xmlSigAlg.Item2);
                    XmlElement xmlElement23 = xmlDocument.CreateElement("DigestValue");
                    XmlElement xmlElement24 = xmlDocument.CreateElement("Transforms");
                    XmlDocument xmlDocument3 = new XmlDocument
                    {
                        PreserveWhitespace = true
                    };
                    using (TextReader input2 = new StringReader(xmlElement20.OuterXml))
                    {
                        XmlReaderSettings xmlReaderSettings2 = new XmlReaderSettings();
                        xmlReaderSettings2.DtdProcessing = DtdProcessing.Parse;
                        xmlReaderSettings2.XmlResolver = new XmlSecureResolver(new XmlUrlResolver(), xmlDocument.BaseURI);
                        XmlReader reader2 = XmlReader.Create(input2, xmlReaderSettings2, xmlDocument.BaseURI);
                        xmlDocument3.Load(reader2);
                    }

                    CanonicalXmlNodeList propagatedAttributes2 = XmlHelper.GetPropagatedAttributes(xmlDocument.DocumentElement);
                    XmlHelper.AddNamespaces(xmlDocument3.DocumentElement, propagatedAttributes2);
                    ((Transform)val2).LoadInput((object)xmlDocument3);
                    byte[] digestedOutput2 = ((Transform)val2).GetDigestedOutput(hashAlgorithm);
                    xmlElement23.InnerText = Convert.ToBase64String(digestedOutput2);
                    XmlElement xmlElement25 = xmlDocument.CreateElement("Transform");
                    xmlElement25.SetAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
                    xmlElement24.AppendChild(xmlElement25);
                    xmlElement21.AppendChild(xmlElement22);
                    xmlElement21.AppendChild(xmlElement23);
                    xmlElement6.AppendChild(xmlElement21);
                }

                Transform val3 = (Transform)new XmlDsigC14NTransform();
                XmlDocument xmlDocument4 = new XmlDocument();
                using (TextReader input3 = new StringReader(xmlElement6.OuterXml))
                {
                    XmlReaderSettings xmlReaderSettings3 = new XmlReaderSettings();
                    xmlReaderSettings3.DtdProcessing = DtdProcessing.Parse;
                    xmlReaderSettings3.XmlResolver = new XmlSecureResolver(new XmlUrlResolver(), xmlDocument.BaseURI);
                    XmlReader reader3 = XmlReader.Create(input3, xmlReaderSettings3, xmlDocument.BaseURI);
                    xmlDocument4.Load(reader3);
                }

                CanonicalXmlNodeList propagatedAttributes3 = XmlHelper.GetPropagatedAttributes(xmlDocument.DocumentElement);
                XmlHelper.AddNamespaces(xmlDocument4.DocumentElement, propagatedAttributes3);
                val3.LoadInput((object)xmlDocument4);
                byte[] digestedOutput3 = val3.GetDigestedOutput(hashAlgorithm);
                if (!string.IsNullOrEmpty(xmlDoc.SignatureInfo.SignatureLocation))
                {
                    XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(xmlDoc.SignatureInfo.SignatureLocation);
                    if (xmlNode != null)
                    {
                        xmlNode.AppendChild(xmlElement);
                    }
                    else
                    {
                        XmlElement xmlElement26 = xmlDocument.CreateElement(xmlDoc.SignatureInfo.SignatureLocation);
                        xmlElement26.AppendChild(xmlElement);
                        xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlElement26, deep: true));
                    }
                }
                else
                {
                    xmlDocument.DocumentElement.AppendChild(xmlElement);
                }

                byte[] algPrefix = GetAlgPrefix(xmlDoc.SignatureInfo.HashAlgorithm);
                byte[] sh = (byte[])digestedOutput3.Clone();
                byte[] array = new byte[algPrefix.Length + digestedOutput3.Length];
                algPrefix.CopyTo(array, 0);
                digestedOutput3.CopyTo(array, algPrefix.Length);
                list.Add(new XmlDocToHashResult
                {
                    DocumentId = xmlDoc.DocumentId,
                    Document = xmlDocument.OuterXml,
                    Digest = array,
                    SignatureId = xmlDoc.SignatureInfo.Id,
                    Sh = sh
                });
            }

            return list;
        }

        private static List<WordDocToHashResult> PreSignWord(List<WordDocToHash> words, byte[] certData, List<string> certificateChain)
        {
            X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
            foreach (string item2 in certificateChain)
            {
                X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(item2));
                x509Certificate2Collection.Add(certificate);
            }

            X509Certificate2 cert = new X509Certificate2(certData);
            List<WordDocToHashResult> list = new List<WordDocToHashResult>();
            foreach (WordDocToHash word in words)
            {
                Guid guid = Guid.NewGuid();
                OfficeSigningSession officeSigningSession = new OfficeSigningSession(word.FileToSign);
                officeSigningSession._objSigningInfo.CertificateChain = x509Certificate2Collection;
                Guid guid2 = guid;
                if (!officeSigningSession.SetSigLineSettings("{" + guid2.ToString() + "}", null, null, _isTrail: false, null))
                {
                    throw new Exception(officeSigningSession.ErrorDescription.ToString());
                }

                if (word.SignatureImage != null && word.SignatureImage.Length != 0)
                {
                    officeSigningSession.SetSigningImage(word.SignatureImage);
                }

                if (!officeSigningSession.Start(OfficeSigningSession.DocumentFormat.MSOFFICE_WORD, OfficeSigningSession.AscHashingAlgo.SHA256, cert, out var calculatedHash, out var mainDom, out var documentBytes))
                {
                    throw new Exception(officeSigningSession.ErrorDescription.ToString());
                }

                byte[] array = new byte[19]
                {
                48, 49, 48, 13, 6, 9, 96, 134, 72, 1,
                101, 3, 4, 2, 1, 5, 0, 4, 32
                };
                byte[] array2 = new byte[array.Length + calculatedHash.Length];
                array.CopyTo(array2, 0);
                calculatedHash.CopyTo(array2, array.Length);
                WordDocToHashResult item = new WordDocToHashResult
                {
                    DocumentId = word.DocumentId,
                    Digest = array2,
                    DocumentBytes = documentBytes,
                    SignatureId = guid.ToString(),
                    MainDom = mainDom
                };
                list.Add(item);
            }

            return list;
        }

        private static List<ExcelDocToHashResult> PreSignExcel(List<ExcelDocToHash> excels, byte[] certData, List<string> certificateChain)
        {
            X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
            foreach (string item2 in certificateChain)
            {
                X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(item2));
                x509Certificate2Collection.Add(certificate);
            }

            X509Certificate2 cert = new X509Certificate2(certData);
            List<ExcelDocToHashResult> list = new List<ExcelDocToHashResult>();
            foreach (ExcelDocToHash excel in excels)
            {
                Guid guid = Guid.NewGuid();
                OfficeSigningSession officeSigningSession = new OfficeSigningSession(excel.FileToSign);
                officeSigningSession._objSigningInfo.CertificateChain = x509Certificate2Collection;
                Guid guid2 = guid;
                if (!officeSigningSession.SetSigLineSettings("{" + guid2.ToString() + "}", null, null, _isTrail: false, null))
                {
                    throw new Exception(officeSigningSession.ErrorDescription.ToString());
                }

                if (excel.SignatureImage != null && excel.SignatureImage.Length != 0)
                {
                    officeSigningSession.SetSigningImage(excel.SignatureImage);
                }

                if (!officeSigningSession.Start(OfficeSigningSession.DocumentFormat.MSOFFICE_EXCEL, OfficeSigningSession.AscHashingAlgo.SHA256, cert, out var calculatedHash, out var mainDom, out var documentBytes))
                {
                    throw new Exception(officeSigningSession.ErrorDescription.ToString());
                }

                byte[] array = new byte[19]
                {
                48, 49, 48, 13, 6, 9, 96, 134, 72, 1,
                101, 3, 4, 2, 1, 5, 0, 4, 32
                };
                byte[] array2 = new byte[array.Length + calculatedHash.Length];
                array.CopyTo(array2, 0);
                calculatedHash.CopyTo(array2, array.Length);
                ExcelDocToHashResult item = new ExcelDocToHashResult
                {
                    DocumentId = excel.DocumentId,
                    Digest = array2,
                    DocumentBytes = documentBytes,
                    SignatureId = guid.ToString(),
                    MainDom = mainDom
                };
                list.Add(item);
            }

            return list;
        }

        private static byte[] GetAlgPrefix(string hashAlg)
        {
            switch (hashAlg)
            {
                case "SHA-1":
                case "SHA1":
                    return _sha1Prefix;
                default:
                    return _sha256Prefix;
                case "SHA-384":
                case "SHA384":
                    return _sha384Prefix;
                case "SHA-512":
                case "SHA512":
                    return _sha512Prefix;
            }
        }

        private static PdfPTable DrawTable(float width, float height, string allText, bool isLogo, float size, byte[] logo, iTextSharp.text.Font font)
        {
            PdfPTable pdfPTable = new PdfPTable(1);
            pdfPTable.SetTotalWidth(new float[1] { width / 2f });
            pdfPTable.LockedWidth = true;
            pdfPTable.SetWidths(new float[1] { width / 2f });
            Phrase phrase = new Phrase();
            phrase.Add(new Chunk(allText, font));
            PdfPCell pdfPCell = new PdfPCell(phrase)
            {
                HorizontalAlignment = 0,
                VerticalAlignment = 5,
                Border = 0,
                Padding = 0f,
                FixedHeight = height
            };
            if (isLogo)
            {
                ImageBackgroundEvent cellEvent = new ImageBackgroundEvent(iTextSharp.text.Image.GetInstance(logo));
                pdfPCell.CellEvent = cellEvent;
            }

            pdfPCell.SetLeading(size + 2f, 0f);
            pdfPTable.AddCell(pdfPCell);
            return pdfPTable;
        }

        public AttachSignatureResult AttachSignature(AttachSignatureData data)
        {
            AttachSignatureResult attachSignatureResult = new AttachSignatureResult
            {
                DocumentType = data.DocumentType
            };
            switch (data.DocumentType)
            {
                case DocumentType.Pdf:
                    {
                        attachSignatureResult.PdfDocs = new List<AttachSignaturePdfResult>();
                        List<X509Certificate> list = new List<X509Certificate>();
                        List<byte[]> list2 = new List<byte[]>();
                        foreach (string item2 in data.CertififcateChain)
                        {
                            X509Certificate2 x509Certificate = new X509Certificate2(Convert.FromBase64String(item2));
                            X509Certificate item = new X509CertificateParser().ReadCertificate(x509Certificate.GetRawCertData());
                            list2.Add(x509Certificate.GetRawCertData());
                            list.Add(item);
                        }

                        X509Certificate2 x509Certificate2 = new X509Certificate2(data.Certififcate);
                        X509Certificate val = new X509CertificateParser().ReadCertificate(x509Certificate2.GetRawCertData());
                        foreach (AttachSignaturePdfData pdfDoc in data.PdfDocs)
                        {
                            PdfReader val2 = new PdfReader((Stream)new MemoryStream(pdfDoc.DocumentBytes));
                            try
                            {
                                PdfPKCS7 val3 = new PdfPKCS7((ICipherParameters)null, list.ToArray(), (!string.IsNullOrEmpty(data.HashAlgorithm)) ? data.HashAlgorithm : "SHA256", false);
                                val3.SetExternalDigest(pdfDoc.Signature, (byte[])null, "RSA");
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    byte[] encodedPKCS = val3.GetEncodedPKCS7(pdfDoc.DocumentHash, 0, null, null, null);
                                    MakeSignature.SignDeferred(val2, pdfDoc.SignatureName, (Stream)memoryStream, new PostSigning(encodedPKCS, PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED));
                                    attachSignatureResult.PdfDocs.Add(new AttachSignaturePdfResult
                                    {
                                        DocumentId = pdfDoc.DocumentId,
                                        Document = memoryStream.ToArray()
                                    });
                                }

                            }
                            finally
                            {
                                ((IDisposable)val2)?.Dispose();
                            }
                        }

                        break;
                    }
                case DocumentType.Xml:
                    attachSignatureResult.XmlDocs = new List<AttachSignatureXmlResult>();
                    foreach (AttachSignatureXmlData xmlDoc in data.XmlDocs)
                    {
                        XmlDocument xmlDocument3 = new XmlDocument
                        {
                            PreserveWhitespace = true
                        };
                        xmlDocument3.LoadXml(xmlDoc.Document);
                        string xpath2 = $"//*[@id='{xmlDoc.SignatureId}']";
                        XmlElement xmlElement3 = (XmlElement)xmlDocument3.SelectSingleNode(xpath2);
                        if (xmlElement3 == null)
                        {
                            xpath2 = $"//*[@Id='{xmlDoc.SignatureId}']";
                            xmlElement3 = (XmlElement)xmlDocument3.SelectSingleNode(xpath2);
                        }

                        XmlNode xmlNode3 = xmlElement3.GetElementsByTagName("SignatureValue")[0];
                        xmlNode3.InnerText = Convert.ToBase64String(xmlDoc.Signature);
                        attachSignatureResult.XmlDocs.Add(new AttachSignatureXmlResult
                        {
                            DocumentId = xmlDoc.DocumentId,
                            Document = xmlDocument3.OuterXml
                        });
                    }

                    break;
                case DocumentType.Excel:
                    attachSignatureResult.ExcelDocs = new List<AttachSignatureExcelResult>();
                    foreach (AttachSignatureExcelData excelDoc in data.ExcelDocs)
                    {
                        byte[] document2 = PostSignOffice(excelDoc.Signature, excelDoc.MainDom, excelDoc.SignatureId, excelDoc.DocumentBytes);
                        attachSignatureResult.ExcelDocs.Add(new AttachSignatureExcelResult
                        {
                            Document = document2,
                            DocumentId = excelDoc.DocumentId
                        });
                    }

                    break;
                case DocumentType.Word:
                    attachSignatureResult.WordDocs = new List<AttachSignatureWordResult>();
                    foreach (AttachSignatureWordData wordDoc in data.WordDocs)
                    {
                        byte[] document = PostSignOffice(wordDoc.Signature, wordDoc.MainDom, wordDoc.SignatureId, wordDoc.DocumentBytes);
                        attachSignatureResult.WordDocs.Add(new AttachSignatureWordResult
                        {
                            Document = document,
                            DocumentId = wordDoc.DocumentId
                        });
                    }

                    break;
            }

            return attachSignatureResult;
        }

        private static byte[] PostSignOffice(byte[] signature, string mainDom, string setupId, byte[] data)
        {
            string SignatureXML = string.Empty;
            OfficeSigningSession officeSigningSession = new OfficeSigningSession(mainDom, setupId, data);
            officeSigningSession.IntegrateSignatureBytes(signature, ref SignatureXML);
            if (!officeSigningSession.IntegrateSignedXML(SignatureXML, out var SignedDocument))
            {
                throw new Exception(officeSigningSession.ErrorDescription.ToString());
            }

            return SignedDocument;
        }

        private static (string, string) GetXmlSigAlg(string hashAlg)
        {
            switch (hashAlg)
            {
                case "SHA-1":
                case "SHA1":
                    return ("http://www.w3.org/2000/09/xmldsig#rsa-sha1", "http://www.w3.org/2000/09/xmldsig#sha1");
                default:
                    return ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", "http://www.w3.org/2001/04/xmlenc#sha256");
                case "SHA-384":
                case "SHA384":
                    return ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha384", "http://www.w3.org/2001/04/xmldsig-more#sha384");
                case "SHA-512":
                case "SHA512":
                    return ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha512", "http://www.w3.org/2001/04/xmlenc#sha512");
            }
        }
    }
}
