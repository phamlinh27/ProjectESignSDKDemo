using iText.Layout.Properties;
using iTextSharp.text.log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProjectESignSDKDemo.action
{
    public class OfficeSigningSession
    {
        public enum AscHashingAlgo
        {
            SHA1 = 1,
            SHA256 = 2,
            SHA384 = 4,
            SHA512 = 8
        }

        public enum AscOSDKError
        {
            INTEGRATION_IN_PROGRESS = -13,
            HASH_IN_PROGRESS,
            SEQUENCE_ERROR,
            INVALID_IMAGE,
            INVALID_CALL,
            INVALID_SESSION_STATE,
            INVALID_EMAIL,
            INVALID_HASH_ALGO,
            IN_PROGRESS,
            INVALID_SETUP_ID,
            INVALID_SIGLINE_CONTENT,
            SIGLINE_NOT_FOUND,
            INVALID_DOCUMENT_STRUCTURE,
            PACKAGE_OPEN_EXCEPTION
        }

        public enum DocumentFormat
        {
            MSOFFICE_WORD = 1,
            MSOFFICE_EXCEL
        }

        public class ProgressEventArgs : EventArgs
        {
            public byte[] HashData { get; private set; }

            public bool isError { get; private set; }

            public ProgressEventArgs(byte[] _hash, bool _error)
            {
                HashData = _hash;
                isError = _error;
            }
        }

        public delegate void MSOfficeHashGenerated(object sender, ProgressEventArgs e);

        public delegate void SigningCompletion(object sender, SignedDocEventArgs e);

        private XmlDocument mainDOMObject;

        private XmlElement mainSignatureElement;

        private XmlElement packageObjElement;

        private XmlElement _elekeyInfo;

        private List<DocPartInfo> _lstDocParts;

        private Package mainOfficeFilePackage;

        private DocProcessingUnit _ObjClsDocProcessingUnit;

        private ExcelProcessingUnit _objClsXlsProcessingUnit;

        private OrigDocInfo _ObjOrigDocInfo;

        internal SigningInfo _objSigningInfo;

        private bool SetSigLineCalled;

        private bool SetSigningImageCalled;

        private bool StartCalled;

        private bool SessionClosedCall;

        private bool IntegrateSignatureCalled;

        private bool IntegrateXMLCalled;

        private DocPartInfo _ObjDocumentPartsInfo;

        private bool _sessionStarted;

        private bool _hashinProgress;

        private string m_strDocumentFormat;

        private List<XmlElement> FinalSignatureNodes = new List<XmlElement>();

        private AscOSDKError _processingError;

        private string _tempFileName;

        private MemoryStream _tempStream;

        private byte[] _finalHash;

        private byte[] _finalDocBytes;

        private byte[] _signedInfoElementBytes;

        public AscOSDKError ErrorDescription { get; private set; }

        public event MSOfficeHashGenerated OnOfficeHashGenerated;

        public event SigningCompletion OnSigningCompletion;

        private void InitializeComponents()
        {
            _ObjOrigDocInfo = new OrigDocInfo();
            _ObjDocumentPartsInfo = new DocPartInfo();
            _objSigningInfo = new SigningInfo();
            _lstDocParts = new List<DocPartInfo>();
            _objSigningInfo.OriginPackagaePrt = null;
            _sessionStarted = (_hashinProgress = false);
            mainDOMObject = new XmlDocument();
            mainDOMObject.InsertBefore(mainDOMObject.CreateXmlDeclaration("1.0", "UTF-8", "no"), mainDOMObject.DocumentElement);
            SetSigLineCalled = (SetSigningImageCalled = (StartCalled = (SessionClosedCall = (IntegrateSignatureCalled = false))));
            IntegrateXMLCalled = false;
        }

        public OfficeSigningSession(Stream DocStream)
        {
            InitializeComponents();
            _ObjOrigDocInfo.docStream = DocStream;
            _sessionStarted = true;
        }

        public OfficeSigningSession(byte[] docRawBytes)
        {
            InitializeComponents();
            _ObjOrigDocInfo.docBytes = docRawBytes;
            _sessionStarted = true;
        }

        public OfficeSigningSession(string mainDomObject, string setupId, byte[] data)
        {
            InitializeComponents();
            _tempStream = new MemoryStream();
            _tempStream.Write(data, 0, data.Length);
            _objSigningInfo = new SigningInfo
            {
                SetupID = setupId
            };
            mainDOMObject = new XmlDocument();
            mainDOMObject.LoadXml(mainDomObject);
            SetSigLineCalled = true;
            SetSigningImageCalled = true;
            StartCalled = true;
            SessionClosedCall = false;
            IntegrateSignatureCalled = false;
            IntegrateXMLCalled = false;
            _hashinProgress = false;
            _sessionStarted = true;
        }

        public bool IntegrateSignatureBytes(byte[] signature, ref string SignatureXML)
        {
            if (!_sessionStarted)
            {
                ErrorDescription = AscOSDKError.INVALID_SESSION_STATE;
                return false;
            }

            if (!StartCalled)
            {
                ErrorDescription = AscOSDKError.SEQUENCE_ERROR;
                return false;
            }

            if (_hashinProgress)
            {
                ErrorDescription = AscOSDKError.HASH_IN_PROGRESS;
                return false;
            }

            if (IntegrateSignatureCalled)
            {
                ErrorDescription = AscOSDKError.INVALID_CALL;
                return false;
            }

            IntegrateSignatureCalled = true;
            _objSigningInfo.Signature = signature;
            mainDOMObject.GetElementsByTagName("SignatureValue")[0].InnerText = Convert.ToBase64String(signature);
            try
            {
                Uri uri = new Uri("/_xmlsignatures/origin.sigs", UriKind.Relative);
                mainOfficeFilePackage = Package.Open((Stream)_tempStream);
                /*if (!mainOfficeFilePackage.PartExists(uri))
                {
                    _objSigningInfo.OriginPackagaePrt = mainOfficeFilePackage.CreatePart(PackUriHelper.CreatePartUri(uri), "application/vnd.openxmlformats-package.digital-signature-origin");
                    mainOfficeFilePackage.CreateRelationship(_objSigningInfo.OriginPackagaePrt.Uri, 0, "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/origin");
                }*/
            }
            catch (Exception ex)
            {
            }

            SignatureXML = mainDOMObject.OuterXml;
            return true;
        }

        public bool Start(DocumentFormat docFormat, AscHashingAlgo _hashAlgo, X509Certificate2 Cert, out byte[] calculatedHash, out string mainDom, out byte[] documentBytes)
        {
            if (!_sessionStarted)
            {
                ErrorDescription = AscOSDKError.INVALID_SESSION_STATE;
                calculatedHash = null;
                mainDom = null;
                documentBytes = null;
                return false;
            }

            if (!SetSigLineCalled)
            {
                ErrorDescription = AscOSDKError.SEQUENCE_ERROR;
                calculatedHash = null;
                mainDom = null;
                documentBytes = null;
                return false;
            }

            if (StartCalled)
            {
                ErrorDescription = AscOSDKError.INVALID_CALL;
                calculatedHash = null;
                mainDom = null;
                documentBytes = null;
                return false;
            }

            StartCalled = true;
            _objSigningInfo.Certificate = Cert;
            _hashinProgress = true;
            try
            {
                _tempFileName = Path.GetTempPath() + Guid.NewGuid();
                _objSigningInfo.docFormat = docFormat;
                FileStream fileStream = new FileStream(_tempFileName, FileMode.Create, FileAccess.ReadWrite);
                if (_ObjOrigDocInfo.docBytes != null)
                {
                    fileStream.Write(_ObjOrigDocInfo.docBytes, 0, _ObjOrigDocInfo.docBytes.Length);
                    fileStream.Close();
                    mainOfficeFilePackage = Package.Open(_tempFileName);
                }
                else
                {
                    _ObjOrigDocInfo.docStream.CopyTo(fileStream);
                    fileStream.Close();
                    mainOfficeFilePackage = Package.Open(_tempFileName);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription = AscOSDKError.PACKAGE_OPEN_EXCEPTION;
                calculatedHash = null;
                mainDom = null;
                documentBytes = null;
                return false;
            }

            mainSignatureElement = mainDOMObject.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
            mainSignatureElement.SetAttribute("Id", "idPackageSignature");
            packageObjElement = mainDOMObject.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");
            packageObjElement.SetAttribute("Id", "idPackageObject");
            packageObjElement.SetAttribute("xmlns:mdssi", "http://schemas.openxmlformats.org/package/2006/digital-signature");
            XmlElement xmlElement = mainDOMObject.CreateElement("Manifest", "http://www.w3.org/2000/09/xmldsig#");
            if (_objSigningInfo.docFormat == DocumentFormat.MSOFFICE_EXCEL)
            {
                _objClsXlsProcessingUnit = new ExcelProcessingUnit(ref mainDOMObject, ref _objSigningInfo, ref _ObjOrigDocInfo, ref _lstDocParts);
            }
            else if (_objSigningInfo.docFormat == DocumentFormat.MSOFFICE_WORD)
            {
                _ObjClsDocProcessingUnit = new DocProcessingUnit( ref mainDOMObject, ref _objSigningInfo, ref _ObjOrigDocInfo, ref _lstDocParts);
            }

            bool flag = false;
            if (_objSigningInfo.docFormat == DocumentFormat.MSOFFICE_EXCEL)
            {
                flag = _objClsXlsProcessingUnit.Process(_hashAlgo, ref _processingError, ref mainOfficeFilePackage);
            }
            else if (_objSigningInfo.docFormat == DocumentFormat.MSOFFICE_WORD)
            {
                flag = _ObjClsDocProcessingUnit.Process(_hashAlgo, ref _processingError, ref mainOfficeFilePackage);
            }

            if (flag)
            {
                for (int i = 0; i < _lstDocParts.Count; i++)
                {
                    xmlElement.AppendChild(_lstDocParts[i].xmlElement);
                }

                packageObjElement.AppendChild(xmlElement);
                XmlElement xmlElement2 = mainDOMObject.CreateElement("SignatureProperties", "http://www.w3.org/2000/09/xmldsig#");
                XmlElement xmlElement3 = mainDOMObject.CreateElement("SignatureProperty", "http://www.w3.org/2000/09/xmldsig#");
                xmlElement3.SetAttribute("Id", "idSignatureTime");
                xmlElement3.SetAttribute("Target", "#idPackageSignature");
                XmlElement xmlElement4 = mainDOMObject.CreateElement("mdssi:SignatureTime", "http://schemas.openxmlformats.org/package/2006/digital-signature");
                XmlElement xmlElement5 = mainDOMObject.CreateElement("mdssi:Format", "http://schemas.openxmlformats.org/package/2006/digital-signature");
                xmlElement5.InnerText = "YYYY-MM-DDThh:mm:ssTZD";
                DateTime utcNow = DateTime.UtcNow;
                string text = utcNow.Year + "-" + PatchZeros(utcNow.Month) + "-" + PatchZeros(utcNow.Day) + "T" + PatchZeros(utcNow.Hour) + ":" + PatchZeros(utcNow.Minute) + ":" + PatchZeros(utcNow.Second) + "Z";
                _objSigningInfo.SigningTime = text;
                XmlElement xmlElement6 = mainDOMObject.CreateElement("mdssi:Value", "http://schemas.openxmlformats.org/package/2006/digital-signature");
                xmlElement6.InnerText = _objSigningInfo.SigningTime;
                xmlElement4.AppendChild(xmlElement5);
                xmlElement4.AppendChild(xmlElement6);
                xmlElement3.AppendChild(xmlElement4);
                xmlElement2.AppendChild(xmlElement3);
                packageObjElement.AppendChild(xmlElement2);
                FinalSignatureNodes.Add(packageObjElement);
                MakeOfficeIfObject();
                MakeIdPackageSingature();
                XmlElement xmlElement7 = mainDOMObject.CreateElement("CanonicalizationMethod", "http://www.w3.org/2000/09/xmldsig#");
                xmlElement7.SetAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
                XmlElement xmlElement8 = mainDOMObject.CreateElement("SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
                xmlElement8.SetAttribute("Algorithm", _objSigningInfo.hashAlgoString2);
                XmlElement xmlElement9 = mainDOMObject.CreateElement("SignedInfo", "http://www.w3.org/2000/09/xmldsig#");
                xmlElement9.AppendChild(xmlElement7);
                xmlElement9.AppendChild(xmlElement8);
                for (int j = 0; j < FinalSignatureNodes.Count; j++)
                {
                    XmlElement newChild = makeReferenceObject(FinalSignatureNodes[j]);
                    xmlElement9.AppendChild(newChild);
                }

                mainSignatureElement.AppendChild(xmlElement9);
                _elekeyInfo = mainDOMObject.CreateElement("KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
                XmlElement xmlElement10 = mainDOMObject.CreateElement("X509Data", "http://www.w3.org/2000/09/xmldsig#");
                XmlElement xmlElement11 = mainDOMObject.CreateElement("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
                xmlElement11.InnerText = Convert.ToBase64String(_objSigningInfo.Certificate.RawData);
                xmlElement10.AppendChild(xmlElement11);
                _elekeyInfo.AppendChild(xmlElement10);
                _finalHash = CanonandHash(xmlElement9);
                mainOfficeFilePackage.Flush();
                mainOfficeFilePackage.Close();
                _tempStream = new MemoryStream();
                XmlElement newChild2 = mainDOMObject.CreateElement("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
                mainSignatureElement.AppendChild(newChild2);
                mainSignatureElement.AppendChild(_elekeyInfo);
                for (int k = 0; k < FinalSignatureNodes.Count; k++)
                {
                    mainSignatureElement.AppendChild(FinalSignatureNodes[k]);
                }

                mainDOMObject.AppendChild(mainSignatureElement);
                mainDom = mainDOMObject.OuterXml;
                FileStream fileStream2 = File.OpenRead(_tempFileName);
                fileStream2.CopyTo(_tempStream);
                fileStream2.Close();
                documentBytes = _tempStream.ToArray();
                _tempStream.Close();
                ProgressEventArgs e = new ProgressEventArgs(_finalHash, _error: false);
                _hashinProgress = false;
                if (this.OnOfficeHashGenerated != null)
                {
                    this.OnOfficeHashGenerated(this, e);
                }

                calculatedHash = _finalHash;
                CloseSession();
                return true;
            }

            ErrorDescription = _processingError;
            calculatedHash = null;
            XmlElement newChild3 = mainDOMObject.CreateElement("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
            mainSignatureElement.AppendChild(newChild3);
            mainSignatureElement.AppendChild(_elekeyInfo);
            for (int l = 0; l < FinalSignatureNodes.Count; l++)
            {
                mainSignatureElement.AppendChild(FinalSignatureNodes[l]);
            }

            mainDOMObject.AppendChild(mainSignatureElement);
            _tempStream = new MemoryStream(File.ReadAllBytes(_tempFileName));
            mainDom = mainDOMObject.OuterXml;
            documentBytes = _tempStream.ToArray();
            _tempStream.Close();
            return false;
        }

        public bool SetSigLineSettings(string setupID, string SigningText, string SigningComments, bool _isTrail, string country)
        {
            if (!_sessionStarted)
            {
                ErrorDescription = AscOSDKError.INVALID_SESSION_STATE;
                return false;
            }

            if (SetSigLineCalled)
            {
                ErrorDescription = AscOSDKError.INVALID_CALL;
                return false;
            }

            if (setupID.Length < 38 || setupID.Length > 38)
            {
                ErrorDescription = AscOSDKError.INVALID_SETUP_ID;
                return false;
            }

            SetSigLineCalled = true;
            _objSigningInfo.SetupID = setupID;
            _objSigningInfo.SigningText = SigningText;
            _objSigningInfo.SigningComments = SigningComments;
            _objSigningInfo.isTrailSignature = _isTrail;
            _objSigningInfo.Country = country;
            return true;
        }

        public bool SetSigningImage(byte[] _image)
        {
            if (!_sessionStarted)
            {
                ErrorDescription = AscOSDKError.INVALID_SESSION_STATE;
                return false;
            }

            if (!SetSigLineCalled)
            {
                ErrorDescription = AscOSDKError.SEQUENCE_ERROR;
                return false;
            }

            if (SetSigningImageCalled)
            {
                ErrorDescription = AscOSDKError.INVALID_CALL;
                return false;
            }

            if (_image.Length == 0)
            {
                ErrorDescription = AscOSDKError.INVALID_IMAGE;
                return false;
            }

            SetSigningImageCalled = true;
            _objSigningInfo.SigningImage = _image;
            return true;
        }


        private XmlElement makeReferenceObject(XmlNode _nodeList)
        {
            XmlNode xmlNode = _nodeList;
            string text = null;
            if (_nodeList.Attributes.Count > 0)
            {
                for (int i = 0; i < _nodeList.Attributes.Count; i++)
                {
                    if (_nodeList.Attributes[i].Name.Contains("Id"))
                    {
                        text = _nodeList.Attributes[i].InnerText;
                        break;
                    }
                }
            }

            if (text == null && xmlNode.ChildNodes[0].Name.Contains("xd:QualifyingProperties"))
            {
                xmlNode = xmlNode.ChildNodes[0].ChildNodes[0];
            }

            XmlElement xmlElement = mainDOMObject.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.SetAttribute("Algorithm", _objSigningInfo.hashAlgoString);
            XmlElement xmlElement2 = mainDOMObject.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(xmlNode.OuterXml);
            streamWriter.Flush();
            memoryStream.Position = 0L;
            XmlDsigC14NTransform val = new XmlDsigC14NTransform();
            (val).LoadInput((object)memoryStream);
            string innerText = Convert.ToBase64String((val).GetDigestedOutput(_objSigningInfo.HashAlgo));
            xmlElement2.InnerText = innerText;
            XmlElement xmlElement3 = mainDOMObject.CreateElement("Reference", "http://www.w3.org/2000/09/xmldsig#");
            if (text == null)
            {
                xmlElement3.SetAttribute("URI", "#idSignedProperties");
                xmlElement3.SetAttribute("Type", "http://uri.etsi.org/01903#SignedProperties");
                XmlElement xmlElement4 = mainDOMObject.CreateElement("Transforms", "http://www.w3.org/2000/09/xmldsig#");
                XmlElement xmlElement5 = mainDOMObject.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
                xmlElement5.SetAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
                xmlElement4.AppendChild(xmlElement5);
                xmlElement3.AppendChild(xmlElement4);
            }
            else
            {
                xmlElement3.SetAttribute("URI", "#" + text);
                xmlElement3.SetAttribute("Type", "http://www.w3.org/2000/09/xmldsig#Object");
            }

            xmlElement3.AppendChild(xmlElement);
            xmlElement3.AppendChild(xmlElement2);
            return xmlElement3;
        }

        private byte[] CanonandHash(XmlElement _signedInfoElem)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(_signedInfoElem.OuterXml);
            streamWriter.Flush();
            memoryStream.Position = 0L;
            XmlDsigC14NTransform val = new XmlDsigC14NTransform();
            (val).LoadInput((object)memoryStream);
            byte[] buffer = (_signedInfoElementBytes = ((MemoryStream)(val).GetOutput()).ToArray());
            return _objSigningInfo.HashAlgo.ComputeHash(buffer);
        }

        private bool MakeIdPackageSingature()
        {
            XmlElement xmlElement = mainDOMObject.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement xmlElement2 = mainDOMObject.CreateElement("xd:QualifyingProperties", "http://uri.etsi.org/01903/v1.3.2#");
            xmlElement2.SetAttribute("Target", "#idPackageSignature");
            XmlElement xmlElement3 = mainDOMObject.CreateElement("xd:SignedProperties", "http://uri.etsi.org/01903/v1.3.2#");
            xmlElement3.SetAttribute("Id", "idSignedProperties");
            xmlElement3.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement2.AppendChild(xmlElement3);
            if (_objSigningInfo.CertificateChain != null && _objSigningInfo.CertificateChain.Count > 0)
            {
                XmlElement xmlElement4 = mainDOMObject.CreateElement("xd:UnsignedProperties", "http://uri.etsi.org/01903/v1.3.2#");
                XmlElement xmlElement5 = mainDOMObject.CreateElement("xd:UnsignedSignatureProperties", "http://uri.etsi.org/01903/v1.3.2#");
                XmlElement xmlElement6 = mainDOMObject.CreateElement("xd:CertificateValues", "http://uri.etsi.org/01903/v1.3.2#");
                X509Certificate2Enumerator enumerator = _objSigningInfo.CertificateChain.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate2 current = enumerator.Current;
                    XmlElement xmlElement7 = mainDOMObject.CreateElement("xd:EncapsulatedX509Certificate", "http://uri.etsi.org/01903/v1.3.2#");
                    xmlElement7.InnerText = Convert.ToBase64String(current.GetRawCertData());
                    xmlElement6.AppendChild(xmlElement7);
                }

                xmlElement5.AppendChild(xmlElement6);
                xmlElement4.AppendChild(xmlElement5);
                xmlElement2.AppendChild(xmlElement4);
            }

            xmlElement.AppendChild(xmlElement2);
            XmlElement xmlElement8 = mainDOMObject.CreateElement("xd:SignedSignatureProperties", "http://uri.etsi.org/01903/v1.3.2#");
            xmlElement3.AppendChild(xmlElement8);
            XmlElement xmlElement9 = mainDOMObject.CreateElement("xd:SigningTime", "http://uri.etsi.org/01903/v1.3.2#");
            xmlElement9.InnerText = _objSigningInfo.SigningTime;
            xmlElement8.AppendChild(xmlElement9);
            if (m_strDocumentFormat != null)
            {
                XmlElement xmlElement10 = mainDOMObject.CreateElement("xd:MimeType", "http://uri.etsi.org/01903/v1.3.2#");
                xmlElement10.InnerText = _objSigningInfo.DocumentFormat;
                xmlElement10.Value = m_strDocumentFormat;
                xmlElement8.AppendChild(xmlElement10);
            }

            XmlElement xmlElement11 = mainDOMObject.CreateElement("xd:SigningCertificate", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement xmlElement12 = mainDOMObject.CreateElement("xd:Cert", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement xmlElement13 = mainDOMObject.CreateElement("xd:CertDigest", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement xmlElement14 = mainDOMObject.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement14.SetAttribute("Algorithm", _objSigningInfo.hashAlgoString);
            XmlElement xmlElement15 = mainDOMObject.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            byte[] inArray = _objSigningInfo.HashAlgo.ComputeHash(_objSigningInfo.Certificate.GetRawCertData());
            xmlElement15.InnerText = Convert.ToBase64String(inArray);
            xmlElement13.AppendChild(xmlElement14);
            xmlElement13.AppendChild(xmlElement15);
            xmlElement12.AppendChild(xmlElement13);
            XmlElement xmlElement16 = mainDOMObject.CreateElement("xd:IssuerSerial", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement xmlElement17 = mainDOMObject.CreateElement("X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement17.InnerText = _objSigningInfo.Certificate.IssuerName.Name;
            XmlElement xmlElement18 = mainDOMObject.CreateElement("X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement18.InnerText = BigInteger.Parse(_objSigningInfo.Certificate.SerialNumber, NumberStyles.HexNumber).ToString();
            xmlElement16.AppendChild(xmlElement17);
            xmlElement16.AppendChild(xmlElement18);
            xmlElement12.AppendChild(xmlElement16);
            xmlElement11.AppendChild(xmlElement12);
            XmlElement xmlElement19 = mainDOMObject.CreateElement("xd:SignaturePolicyIdentifier", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement newChild = mainDOMObject.CreateElement("xd:SignaturePolicyImplied", "http://uri.etsi.org/01903/v1.3.2#");
            xmlElement19.AppendChild(newChild);
            XmlElement xmlElement20 = mainDOMObject.CreateElement("xd:SignatureProductionPlace", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement xmlElement21 = mainDOMObject.CreateElement("xd:CountryName", "http://uri.etsi.org/01903/v1.3.2#");
            xmlElement21.InnerText = _objSigningInfo.Country;
            xmlElement20.AppendChild(xmlElement21);
            xmlElement8.AppendChild(xmlElement20);
            xmlElement8.AppendChild(xmlElement11);
            xmlElement8.AppendChild(xmlElement19);
            FinalSignatureNodes.Add(xmlElement);
            return true;
        }

        private bool MakeOfficeIfObject()
        {
            XmlElement xmlElement = mainDOMObject.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.SetAttribute("Id", "idOfficeObject");
            XmlElement xmlElement2 = mainDOMObject.CreateElement("SignatureProperties", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.AppendChild(xmlElement2);
            XmlElement xmlElement3 = mainDOMObject.CreateElement("SignatureProperty", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement3.SetAttribute("Id", "idOfficeV1Details");
            xmlElement3.SetAttribute("Target", "#idPackageSignature");
            xmlElement2.AppendChild(xmlElement3);
            XmlElement xmlElement4 = mainDOMObject.CreateElement("SignatureInfoV1", "http://schemas.microsoft.com/office/2006/digsig");
            XmlElement xmlElement5 = mainDOMObject.CreateElement("SetupID", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement5.InnerText = _objSigningInfo.SetupID;
            xmlElement4.AppendChild(xmlElement5);
            XmlElement xmlElement6 = mainDOMObject.CreateElement("SignatureText", "http://schemas.microsoft.com/office/2006/digsig");
            if (!string.IsNullOrEmpty(_objSigningInfo.SigningText))
            {
                xmlElement6.InnerText = _objSigningInfo.SigningText.Trim();
            }

            xmlElement4.AppendChild(xmlElement6);
            XmlElement xmlElement7 = mainDOMObject.CreateElement("SignatureImage", "http://schemas.microsoft.com/office/2006/digsig");
            if (_objSigningInfo.SigningImage != null)
            {
                xmlElement7.InnerText = Convert.ToBase64String(_objSigningInfo.SigningImage);
            }

            xmlElement4.AppendChild(xmlElement7);
            XmlElement xmlElement8 = mainDOMObject.CreateElement("SignatureComments", "http://schemas.microsoft.com/office/2006/digsig");
            if (!string.IsNullOrEmpty(_objSigningInfo.SigningComments))
            {
                xmlElement8.InnerText = _objSigningInfo.SigningComments.Trim();
            }

            xmlElement4.AppendChild(xmlElement8);
            XmlElement xmlElement9 = mainDOMObject.CreateElement("ApplicationVersion", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement9.InnerText = "1.0";
            xmlElement4.AppendChild(xmlElement9);
            XmlElement xmlElement10 = mainDOMObject.CreateElement("SignatureProviderId", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement10.InnerText = _objSigningInfo.desiredSigLine?.ProviderId;
            xmlElement4.AppendChild(xmlElement10);
            XmlElement newChild = mainDOMObject.CreateElement("SignatureProviderUrl", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement4.AppendChild(newChild);
            XmlElement xmlElement11 = mainDOMObject.CreateElement("SignatureProviderDetails", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement11.InnerText = "9";
            xmlElement4.AppendChild(xmlElement11);
            XmlElement xmlElement12 = mainDOMObject.CreateElement("ManifestHashAlgorithm", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement12.InnerText = _objSigningInfo.hashAlgoString;
            xmlElement4.AppendChild(xmlElement12);
            XmlElement xmlElement13 = mainDOMObject.CreateElement("SignatureType", "http://schemas.microsoft.com/office/2006/digsig");
            xmlElement13.InnerText = "2";
            xmlElement4.AppendChild(xmlElement13);
            xmlElement3.AppendChild(xmlElement4);
            FinalSignatureNodes.Add(xmlElement);
            return true;
        }

        private string PatchZeros(int value)
        {
            string result = "";
            switch (value)
            {
                case 0:
                    result = "00";
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    result = "0" + value;
                    break;
                default:
                    if (value > 9)
                    {
                        result = value.ToString();
                    }

                    break;
            }

            return result;
        }

        public bool CloseSession()
        {
            if (!_sessionStarted)
            {
                ErrorDescription = AscOSDKError.INVALID_SESSION_STATE;
                return false;
            }

            if (SessionClosedCall)
            {
                ErrorDescription = AscOSDKError.INVALID_CALL;
                return false;
            }

            if (_hashinProgress)
            {
                ErrorDescription = AscOSDKError.HASH_IN_PROGRESS;
                return false;
            }

            if (IntegrateXMLCalled)
            {
                ErrorDescription = AscOSDKError.INTEGRATION_IN_PROGRESS;

                return false;
            }

            _sessionStarted = (_hashinProgress = false);
            SetSigLineCalled = (SetSigningImageCalled = (StartCalled = (SessionClosedCall = (IntegrateSignatureCalled = (IntegrateXMLCalled = false)))));
            if (_ObjClsDocProcessingUnit != null)
            {
                _ObjClsDocProcessingUnit = null;
            }

            if (_objClsXlsProcessingUnit != null)
            {
                _objClsXlsProcessingUnit = null;
            }

            if (_ObjDocumentPartsInfo != null)
            {
                _ObjDocumentPartsInfo = null;
            }

            if (_ObjOrigDocInfo != null)
            {
                _ObjOrigDocInfo = null;
            }

            mainOfficeFilePackage.Close();
            _lstDocParts.Clear();
            _lstDocParts = null;
            File.Delete(_tempFileName);
            return true;
        }

        private XmlElement GetPackageElementbyName(string _name)
        {
            for (int i = 0; i < _lstDocParts.Count; i++)
            {
                if (_lstDocParts[i].partUri.OriginalString.Contains(_name))
                {
                    return _lstDocParts[i].xmlElement;
                }
            }

            return null;
        }

        public bool IntegrateSignedXML(string _signedXML, out byte[] SignedDocument)
        {
            if (!_sessionStarted)
            {
                ErrorDescription = AscOSDKError.INVALID_SESSION_STATE;
                SignedDocument = null;
                return false;
            }

            if (!StartCalled)
            {
                ErrorDescription = AscOSDKError.SEQUENCE_ERROR;
                SignedDocument = null;
                return false;
            }

            if (_hashinProgress)
            {
                ErrorDescription = AscOSDKError.HASH_IN_PROGRESS;
                SignedDocument = null;
                return false;
            }

            if (!IntegrateSignatureCalled)
            {
                ErrorDescription = AscOSDKError.SEQUENCE_ERROR;
                SignedDocument = null;
                return false;
            }

            if (IntegrateXMLCalled)
            {
                ErrorDescription = AscOSDKError.INTEGRATION_IN_PROGRESS;
                SignedDocument = null;
                return false;
            }

            IntegrateXMLCalled = true;
            string text = _objSigningInfo.SetupID.Substring(0, 20).Replace("-", "").Replace("{", "");
            if (_objSigningInfo.OriginPackagaePrt == null)
            {
                GetOriginPart();
            }

            /*PackagePart val = mainOfficeFilePackage.CreatePart(PackUriHelper.CreatePartUri(new Uri("_xmlsignatures/MISA_" + text + ".xml", UriKind.Relative)), "application/vnd.openxmlformats-package.digital-signature-xmlsignature+xml");
            _objSigningInfo.OriginPackagaePrt.CreateRelationship(val.Uri, (TargetMode)0, "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/signature", "rId" + text);
            XmlTextWriter xmlTextWriter = new XmlTextWriter(val.GetStream(FileMode.Create), Encoding.UTF8);
            xmlTextWriter.WriteRaw(_signedXML);
            xmlTextWriter.Flush();
            xmlTextWriter.Close();
            mainOfficeFilePackage.Flush();
            mainOfficeFilePackage.Close();*/
            _finalDocBytes = _tempStream.ToArray();
            SignedDocEventArgs e = new SignedDocEventArgs(_finalDocBytes, _error: false);
            _hashinProgress = false;
            if (this.OnSigningCompletion != null)
            {
                this.OnSigningCompletion(this, e);
            }

            IntegrateXMLCalled = false;
            SignedDocument = _finalDocBytes;
            _tempStream.Close();
            return true;
        }

        private void GetOriginPart()
        {
            //IL_0026: Unknown result type (might be due to invalid IL or missing references)
            //IL_002c: Invalid comparison between Unknown and I4
            foreach (PackageRelationship item in mainOfficeFilePackage.GetRelationshipsByType("http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/origin"))
            {
                if ((int)item.TargetMode > 0)
                {
                    throw new Exception("PackageSignatureCorruption");
                }

                Uri uri = PackUriHelper.ResolvePartUri(item.SourceUri, item.TargetUri);
                if (!mainOfficeFilePackage.PartExists(uri))
                {
                    throw new Exception("SignatureOriginNotFound");
                }

                PackagePart part = mainOfficeFilePackage.GetPart(uri);
                if (part.ContentType.Equals("application/vnd.openxmlformats-package.digital-signature-origin"))
                {
                    _objSigningInfo.OriginPackagaePrt = part;
                }
            }
        }
    }

    internal class OrigDocInfo
    {
        public string docUrl { get; set; }

        public byte[] docBytes { get; set; }

        public Stream docStream { get; set; }

        public X509Certificate2 certToSign { get; set; }
    }

    internal class SigningInfo
    {
        public string SetupID { get; set; }

        public string Email { get; set; }

        public string SignerRole { get; set; }

        public string SigningText { get; set; }

        public byte[] SigningImage { get; set; }

        public string SigningComments { get; set; }

        public HashAlgorithm HashAlgo { get; set; }

        public string hashAlgoString { get; set; }

        public string hashAlgoString2 { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public X509Certificate2Collection CertificateChain { get; set; }

        public byte[] Signature { get; set; }

        public Bitmap SigLineImage { get; set; }

        public SignatureLineContent desiredSigLine { get; set; }

        public string SigningTime { get; set; }

        public string DocumentFormat { get; set; }

        public byte[] ValidImgLine { get; set; }

        public byte[] InvalidImgLine { get; set; }

        public PackagePart OriginPackagaePrt { get; set; }

        public bool isTrailSignature { get; set; }

        public OfficeSigningSession.DocumentFormat docFormat { get; set; }

        public string Country { get; set; }
    }

    internal class SignatureLineContent
    {
        public string OfficeID { get; set; }

        public string SignerEmail { get; set; }

        public string SignerName { get; set; }

        public string ProviderId { get; set; }

        public string SigLineImageID { get; set; }

        public string SigImageUri { get; set; }

        public byte[] SigLineImage { get; set; }

        public byte[] SignerInValidImage { get; set; }

        public byte[] SignerValidImage { get; set; }

        public XmlNode SignatureLineNode { get; set; }

        public string SuggestedSignerRole { get; set; }

        public string domInnerXml { get; set; }
    }

    public class SignedDocEventArgs : EventArgs
    {
        public byte[] DocBytes { get; private set; }

        public bool isError { get; private set; }

        public SignedDocEventArgs(byte[] docBytes, bool _error)
        {
            DocBytes = docBytes;
            isError = _error;
        }
    }

    internal class DocPartInfo
    {
        public Uri partUri { get; set; }

        public string hash { get; set; }

        public string compiledXML { get; set; }

        public bool isImage { get; set; }

        public string originalXml { get; set; }

        public XmlElement xmlElement { get; set; }

        public Stream partStream { get; set; }
    }

    internal class SortXmlNodes : IEquatable<SortXmlNodes>, IComparable<SortXmlNodes>
    {
        public string _xmlNodeID { get; set; }

        public string _xmlNodeStr { get; set; }

        public string _xmlNodeType { get; set; }

        public string _xmlNodeTarget { get; set; }

        public override string ToString()
        {
            return "ID: " + _xmlNodeID + "   Name: " + _xmlNodeStr;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is SortXmlNodes other && Equals(other);
        }

        public int SortByNameAscending(string name1, string name2)
        {
            return name1.CompareTo(name2);
        }

        public int CompareTo(SortXmlNodes comparePart)
        {
            return (comparePart == null) ? 1 : _xmlNodeID.CompareTo(comparePart._xmlNodeID);
        }

        public override int GetHashCode()
        {
            return Convert.ToInt16(_xmlNodeID);
        }

        public bool Equals(SortXmlNodes other)
        {
            return other != null && _xmlNodeID.Equals(other._xmlNodeID);
        }
    }
    internal class DocProcessingUnit
    {
        private XmlDocument _mainDOMObject;

        private HashAlgorithm _ObjHashAlgo;

        private SigningInfo _signInfo;

        private List<DocPartInfo> _lstDocPartsInfo;

        private List<SignatureLineContent> _lstSigLines = new List<SignatureLineContent>();

        private SignatureLineContent _desiredSignLine;

        private OrigDocInfo _docInfo;

        private bool _inProgress;

        private Package _mainOfficePackage;

        private List<Reference> _packageObjReferences = new List<Reference>();


        private XmlElement MakeRefernceObject(string _uri, string _hashVal)
        {
            XmlElement xmlElement = _mainDOMObject.CreateElement("Reference", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.SetAttribute("URI", _uri);
            XmlElement xmlElement2 = _mainDOMObject.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement2.SetAttribute("Algorithm", _signInfo.hashAlgoString);
            XmlNode xmlNode = _mainDOMObject.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            xmlNode.InnerText = _hashVal;
            xmlElement.AppendChild(xmlElement2);
            xmlElement.AppendChild(xmlNode);
            return xmlElement;
        }

        private XmlElement MakeRefernceObjectRelationship(string _uri, string _hashVal, List<string> ids)
        {
            XmlElement xmlElement = _mainDOMObject.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.SetAttribute("Algorithm", "http://schemas.openxmlformats.org/package/2006/RelationshipTransform");
            for (int i = 0; i < ids.Count; i++)
            {
                XmlElement xmlElement2 = _mainDOMObject.CreateElement("mdssi:RelationshipReference", "http://schemas.openxmlformats.org/package/2006/digital-signature");
                xmlElement2.SetAttribute("SourceId", ids[i]);
                xmlElement.AppendChild(xmlElement2);
            }

            XmlElement xmlElement3 = _mainDOMObject.CreateElement("Reference", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement3.SetAttribute("URI", _uri);
            XmlElement xmlElement4 = _mainDOMObject.CreateElement("Transforms", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement4.AppendChild(xmlElement);
            XmlElement xmlElement5 = _mainDOMObject.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement5.SetAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
            xmlElement4.AppendChild(xmlElement5);
            XmlElement xmlElement6 = _mainDOMObject.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement6.SetAttribute("Algorithm", _signInfo.hashAlgoString);
            XmlNode xmlNode = _mainDOMObject.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            xmlNode.InnerText = _hashVal;
            xmlElement3.AppendChild(xmlElement4);
            xmlElement3.AppendChild(xmlElement6);
            xmlElement3.AppendChild(xmlNode);
            return xmlElement3;
        }

        private XmlNode GetWPictElement(XmlNode _parentNode, out bool found)
        {
            if (_parentNode.HasChildNodes)
            {
                XmlNodeList childNodes = _parentNode.ChildNodes;
                if (_parentNode.Name == "w:pict")
                {
                    for (int i = 0; i < _parentNode.ChildNodes.Count; i++)
                    {
                        if (_parentNode.ChildNodes[i].Name == "v:shape")
                        {
                            if (_parentNode.ChildNodes[i].Attributes["type"].Value.Contains("_x0000_t75"))
                            {
                                found = true;
                                return _parentNode;
                            }

                            found = false;
                        }
                    }
                }

                for (int j = 0; j < childNodes.Count; j++)
                {
                    bool found2 = false;
                    XmlNode wPictElement = GetWPictElement(childNodes[j], out found2);
                    if (found2)
                    {
                        found = true;
                        return wPictElement;
                    }
                }
            }

            found = false;
            return null;
        }

        private bool FindSignatureLine(XmlDocument doc, string _refSetuID, ref OfficeSigningSession.AscOSDKError _error, ref bool _refSGFound)
        {
            _refSGFound = true;
            return true;
        }

        private bool GetSigLineImage()
        {
            SignatureLineContent desiredSignLine = _desiredSignLine;
            for (int i = 0; i < _lstDocPartsInfo.Count; i++)
            {
                if (_lstDocPartsInfo[i].isImage && _lstDocPartsInfo[i].partUri.OriginalString.Contains(desiredSignLine.SigImageUri) && desiredSignLine.SigLineImage == null)
                {
                    _signInfo.SigLineImage = new Bitmap((Stream)new MemoryStream(Convert.FromBase64String(_lstDocPartsInfo[i].originalXml)));
                    return true;
                }
            }
            return false;
        }

        private bool MakeValidnInvalidImage()
        {
            Bitmap val = new Bitmap((Image)(object)_signInfo.SigLineImage);
            Graphics val2 = Graphics.FromImage((Image)(object)val);
            val2.TextRenderingHint = (TextRenderingHint)4;
            val2.DrawString("Invalid Signature", new Font("ARIAL", 9f), Brushes.Red, new PointF(150f, 5f));
            if (_signInfo.isTrailSignature)
            {
                val2.DrawString("TRAIL", new Font("ARIAL", 15f), Brushes.Red, new PointF(5f, 5f));
            }

            if (_signInfo.SigningImage != null)
            {
                Bitmap val3 = new Bitmap((Stream)new MemoryStream(_signInfo.SigningImage));
                val2.DrawImage((Image)(object)val3, new Rectangle(40, 20, 122, 55));
            }
            else if (!string.IsNullOrEmpty(_signInfo.SigningText))
            {
                val2.DrawString(_signInfo.SigningText.Trim(), new Font("ARIAL", 12f), Brushes.Black, new PointF(42f, 52f));
            }

            string[] array = _signInfo.Certificate.SubjectName.Name.Split(new char[1] { ',' });
            string text = null;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("CN="))
                {
                    text = array[i];
                    break;
                }
            }

            string text2 = text.Replace("CN=", "");
            text2.Trim();
            val2.DrawString("Signed by: " + text2, new Font("ARIAL", 8f), Brushes.Black, new PointF(8f, 113f));
            MemoryStream memoryStream = new MemoryStream();
            EncoderParameters val4 = new EncoderParameters(1);
            val4.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            ((Image)val).Save((Stream)memoryStream, GetEncoder(ImageFormat.Png), val4);
            _signInfo.InvalidImgLine = memoryStream.ToArray();
            Bitmap val5 = new Bitmap((Image)(object)_signInfo.SigLineImage);
            Graphics val6 = Graphics.FromImage((Image)(object)val5);
            val6.TextRenderingHint = (TextRenderingHint)4;
            if (_signInfo.SigningImage != null)
            {
                Bitmap val7 = new Bitmap((Stream)new MemoryStream(_signInfo.SigningImage));
                val6.DrawImage((Image)(object)val7, new Rectangle(40, 20, 122, 55));
            }
            else if (!string.IsNullOrEmpty(_signInfo.SigningText))
            {
                val6.DrawString(_signInfo.SigningText, new Font("ARIAL", 12f), Brushes.Black, new PointF(42f, 52f));
            }

            val6.DrawString("Signed by: " + text2, new Font("ARIAL", 8f), Brushes.Black, new PointF(8f, 113f));
            if (_signInfo.isTrailSignature)
            {
                val6.DrawString(DateTime.Now.ToString("yyyy-MM-dd"), new Font("ARIAL", 8f), Brushes.Black, new PointF(192f, 10f));
                val6.DrawString("TRAIL", new Font("ARIAL", 15f), Brushes.Red, new PointF(5f, 5f));
            }
            else
            {
                val6.DrawString(DateTime.Now.ToString("yyyy-MM-dd"), new Font("ARIAL", 8f), Brushes.Black, new PointF(192f, 5f));
            }

            MemoryStream memoryStream2 = new MemoryStream();
            new EncoderParameters(1).Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            ((Image)val5).Save((Stream)memoryStream2, GetEncoder(ImageFormat.Png), val4);
            _signInfo.ValidImgLine = memoryStream2.ToArray();
            return true;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] imageDecoders = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo val in imageDecoders)
            {
                if (val.FormatID == format.Guid)
                {
                    return val;
                }
            }

            return null;
        }

        private string GetStringFromStream(Stream stream)
        {
            stream.Position = 0L;
            using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                return streamReader.ReadToEnd();
        }

        private byte[] GetBytesfromStream(Stream stream)
        {
            stream.Position = 0L;
            using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                return Encoding.ASCII.GetBytes(streamReader.ReadToEnd());
        }

        public DocProcessingUnit(ref XmlDocument mDomObj, ref SigningInfo _signInfo, ref OrigDocInfo _oDocInfo, ref List<DocPartInfo> _docParts)
        {
            _mainDOMObject = mDomObj;
            _docInfo = _oDocInfo;
            this._signInfo = _signInfo;
            _lstDocPartsInfo = _docParts;
        }

        public bool Process(OfficeSigningSession.AscHashingAlgo hashAlgo, ref OfficeSigningSession.AscOSDKError _error, ref Package _package)
        {
            if (_inProgress)
            {
                _error = OfficeSigningSession.AscOSDKError.IN_PROGRESS;
                return false;
            }

            switch (hashAlgo)
            {
                case OfficeSigningSession.AscHashingAlgo.SHA1:
                    _ObjHashAlgo = new SHA1CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2000/09/xmldsig#sha1";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                    break;
                case OfficeSigningSession.AscHashingAlgo.SHA256:
                    _ObjHashAlgo = new SHA256CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2001/04/xmlenc#sha256";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                    break;
                case OfficeSigningSession.AscHashingAlgo.SHA384:
                    _ObjHashAlgo = new SHA384CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2001/04/xmldsig-more#sha384";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";
                    break;
                case OfficeSigningSession.AscHashingAlgo.SHA512:
                    _ObjHashAlgo = new SHA512CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2001/04/xmlenc#sha512";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
                    break;
                default:
                    _error = OfficeSigningSession.AscOSDKError.INVALID_HASH_ALGO;
                    return false;
            }

            _inProgress = true;
            _mainOfficePackage = _package;
            foreach (PackagePart part in _package.GetParts())
            {
                if (part.Uri.OriginalString.Contains("/_xmlsignatures/origin.sigs"))
                {
                    _signInfo.OriginPackagaePrt = part;
                }

                if (!part.Uri.OriginalString.Contains("/word/") && !part.Uri.OriginalString.Contains("/_xmlsignatures") && !part.Uri.OriginalString.Contains("_rels/.rels"))
                {
                    continue;
                }

                DocPartInfo docPartInfo = new DocPartInfo();
                docPartInfo.partUri = part.Uri;
                if (part.ContentType.Contains("application/vnd.openxmlformats-package.relationships+xml"))
                {
                    List<string> list = new List<string>();
                    bool flag = false;
                    bool flag2 = false;
                    if (docPartInfo.partUri.OriginalString.Contains("document.xml.rels"))
                    {
                        flag = true;
                    }
                    else if (docPartInfo.partUri.OriginalString.Contains("_rels/.rels"))
                    {
                        flag2 = true;
                    }

                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.PreserveWhitespace = false;
                    Stream stream = part.GetStream();
                    xmlDocument.Load(stream);
                    XmlNodeList childNodes = xmlDocument.DocumentElement.ChildNodes;
                    List<SortXmlNodes> list2 = new List<SortXmlNodes>();
                    XmlElement documentElement = xmlDocument.DocumentElement;
                    bool flag3 = false;
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        XmlNode xmlNode = childNodes[i];
                        string value = xmlNode.Attributes.GetNamedItem("Id").Value;
                        string value2 = xmlNode.Attributes.GetNamedItem("Type").Value;
                        string value3 = xmlNode.Attributes.GetNamedItem("Target").Value;
                        if (flag)
                        {
                            if (xmlNode.NodeType != XmlNodeType.Element)
                            {
                                continue;
                            }

                            if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/customXml"))
                            {
                                flag3 = true;
                            }
                            else if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"))
                            {
                                flag3 = true;
                            }
                            else if (value2.Contains("http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"))
                            {
                                flag3 = true;
                            }
                            else if (value2.Contains("http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/origin"))
                            {
                                flag3 = true;
                            }
                            else if (value2.Contains("http://schemas.openxmlformats.org/package/2006/relationships/metadata/thumbnail"))
                            {
                                flag3 = true;
                            }
                            else if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/presProps"))
                            {
                                flag3 = true;
                            }
                            else if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/viewProps"))
                            {
                                flag3 = true;
                            }

                            if (flag3)
                            {
                                flag3 = false;
                                documentElement.RemoveChild(xmlNode);
                                i--;
                                continue;
                            }

                            for (int j = 0; j < _lstSigLines.Count; j++)
                            {
                                if (_lstSigLines[j].SigLineImageID.Equals(value))
                                {
                                    _lstSigLines[j].SigImageUri = value3;
                                }
                            }

                            list2.Add(new SortXmlNodes
                            {
                                _xmlNodeID = value,
                                _xmlNodeTarget = value3,
                                _xmlNodeType = value2,
                                _xmlNodeStr = xmlNode.OuterXml
                            });
                            list.Add(value);
                        }
                        else if (flag2)
                        {
                            if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"))
                            {
                                list2.Add(new SortXmlNodes
                                {
                                    _xmlNodeID = value,
                                    _xmlNodeTarget = value3,
                                    _xmlNodeType = value2,
                                    _xmlNodeStr = xmlNode.OuterXml
                                });
                                list.Add(value);
                            }
                        }
                        else
                        {
                            list2.Add(new SortXmlNodes
                            {
                                _xmlNodeID = value,
                                _xmlNodeTarget = value3,
                                _xmlNodeType = value2,
                                _xmlNodeStr = xmlNode.OuterXml
                            });
                            list.Add(value);
                        }
                    }

                    xmlDocument.DocumentElement.RemoveAll();
                    list2.Sort();
                    for (int k = 0; k < list2.Count; k++)
                    {
                        XmlElement xmlElement = xmlDocument.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships");
                        xmlElement.SetAttribute("Id", list2[k]._xmlNodeID);
                        xmlElement.SetAttribute("Target", list2[k]._xmlNodeTarget);
                        xmlElement.SetAttribute("Type", list2[k]._xmlNodeType);
                        if (list2[k]._xmlNodeType.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink"))
                        {
                            xmlElement.SetAttribute("TargetMode", "External");
                        }
                        else
                        {
                            xmlElement.SetAttribute("TargetMode", "Internal");
                        }

                        xmlDocument.DocumentElement.AppendChild(xmlElement);
                    }

                    XmlDsigC14NTransform val = new XmlDsigC14NTransform(false);
                    (val).LoadInput((object)xmlDocument);
                    string hash = Convert.ToBase64String((val).GetDigestedOutput(_signInfo.HashAlgo));
                    docPartInfo.hash = hash;
                    XmlElement xmlElement2 = MakeRefernceObjectRelationship(docPartInfo.partUri.OriginalString + "?ContentType=" + part.ContentType, docPartInfo.hash, list);
                    XmlDsigC14NTransform val2 = new XmlDsigC14NTransform();
                    XmlNodeList elementsByTagName = xmlElement2.GetElementsByTagName("Transforms");
                    (val2).LoadInput((object)elementsByTagName);
                    GetStringFromStream((Stream)(val2).GetOutput());
                    docPartInfo.xmlElement = xmlElement2;
                    _lstDocPartsInfo.Add(docPartInfo);
                    continue;
                }

                if (part.Uri.OriginalString.Contains("/word/document.xml"))
                {
                    XmlDocument xmlDocument2 = new XmlDocument();
                    xmlDocument2.PreserveWhitespace = false;
                    Stream stream2 = part.GetStream();
                    xmlDocument2.Load(stream2);
                    OfficeSigningSession.AscOSDKError _error2 = OfficeSigningSession.AscOSDKError.INVALID_SETUP_ID;
                    bool _refSGFound = false;
                    FindSignatureLine(xmlDocument2, _signInfo.SetupID, ref _error2, ref _refSGFound);
                    if (!_refSGFound)
                    {
                        _package.Close();
                        _error = _error2;
                        return false;
                    }

                    stream2.Close();
                }

                Stream stream3 = part.GetStream();
                byte[] array = new byte[stream3.Length];
                stream3.Read(array, 0, Convert.ToInt32(stream3.Length));
                docPartInfo.originalXml = ((!part.Uri.OriginalString.Contains(".xml")) ? Convert.ToBase64String(array) : GetStringFromStream(stream3));
                docPartInfo.isImage = true;
                docPartInfo.hash = Convert.ToBase64String(_ObjHashAlgo.ComputeHash(array));
                XmlElement xmlElement3 = MakeRefernceObject(docPartInfo.partUri.OriginalString + "?ContentType=" + part.ContentType, docPartInfo.hash);
                docPartInfo.xmlElement = xmlElement3;
                _lstDocPartsInfo.Add(docPartInfo);
            }
            return true;
        }
    }

    internal class ExcelProcessingUnit
    {
        private XmlDocument _mainDOMObject;

        private HashAlgorithm _ObjHashAlgo;

        private SigningInfo _signInfo;

        private List<DocPartInfo> _lstDocPartsInfo;

        private List<SignatureLineContent> _lstSigLines = new List<SignatureLineContent>();

        private SignatureLineContent _desiredSignLine;

        private OrigDocInfo _docInfo;

        private bool _inProgress;

        private Package _mainOfficePackage;

        private List<Reference> _packageObjReferences = new List<Reference>();

        private XmlElement MakeRefernceObject(string _uri, string _hashVal)
        {
            XmlElement xmlElement = _mainDOMObject.CreateElement("Reference", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.SetAttribute("URI", _uri);
            XmlElement xmlElement2 = _mainDOMObject.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement2.SetAttribute("Algorithm", _signInfo.hashAlgoString);
            XmlNode xmlNode = _mainDOMObject.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            xmlNode.InnerText = _hashVal;
            xmlElement.AppendChild(xmlElement2);
            xmlElement.AppendChild(xmlNode);
            return xmlElement;
        }

        private XmlElement MakeRefernceObjectRelationship(string _uri, string _hashVal, List<string> ids)
        {
            XmlElement xmlElement = _mainDOMObject.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement.SetAttribute("Algorithm", "http://schemas.openxmlformats.org/package/2006/RelationshipTransform");
            for (int i = 0; i < ids.Count; i++)
            {
                XmlElement xmlElement2 = _mainDOMObject.CreateElement("mdssi:RelationshipReference", "http://schemas.openxmlformats.org/package/2006/digital-signature");
                xmlElement2.SetAttribute("SourceId", ids[i]);
                xmlElement.AppendChild(xmlElement2);
            }

            XmlElement xmlElement3 = _mainDOMObject.CreateElement("Reference", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement3.SetAttribute("URI", _uri);
            XmlElement xmlElement4 = _mainDOMObject.CreateElement("Transforms", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement4.AppendChild(xmlElement);
            XmlElement xmlElement5 = _mainDOMObject.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement5.SetAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
            xmlElement4.AppendChild(xmlElement5);
            XmlElement xmlElement6 = _mainDOMObject.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            xmlElement6.SetAttribute("Algorithm", _signInfo.hashAlgoString);
            XmlNode xmlNode = _mainDOMObject.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            xmlNode.InnerText = _hashVal;
            xmlElement3.AppendChild(xmlElement4);
            xmlElement3.AppendChild(xmlElement6);
            xmlElement3.AppendChild(xmlNode);
            return xmlElement3;
        }

        private XmlNode GetShapeElement(XmlNode _parentNode, out bool found)
        {
            if (_parentNode.HasChildNodes)
            {
                if (_parentNode.Name == "v:shape")
                {
                    found = true;
                    return _parentNode;
                }

                XmlNodeList childNodes = _parentNode.ChildNodes;
                for (int i = 0; i < _parentNode.ChildNodes.Count; i++)
                {
                    if (_parentNode.ChildNodes[i].Name == "v:shape")
                    {
                        if (_parentNode.ChildNodes[i].Attributes["type"].Value.Contains("_x0000_t75"))
                        {
                            found = true;
                            return _parentNode.ChildNodes[i];
                        }

                        found = false;
                    }
                }
            }

            found = false;
            return null;
        }

        private bool FindSignatureLine(string _name, XmlDocument doc, string _refSetuID, ref OfficeSigningSession.AscOSDKError _error, ref bool _refSGFound)
        {
            _refSGFound = true;
            return true;
        }

        private bool GetSigLineImage()
        {
            SignatureLineContent desiredSignLine = _desiredSignLine;
            for (int i = 0; i < _lstDocPartsInfo.Count; i++)
            {
                if (_lstDocPartsInfo[i].partUri.OriginalString.Contains(desiredSignLine.SigImageUri) && desiredSignLine.SigLineImage == null)
                {
                    _signInfo.SigLineImage = new Bitmap((Stream)new MemoryStream(Convert.FromBase64String(_lstDocPartsInfo[i].originalXml)));
                    return true;
                }
            }

            return false;
        }

        private bool MakeValidnInvalidImage()
        {
            Bitmap val = new Bitmap((Image)(object)_signInfo.SigLineImage);
            Graphics val2 = Graphics.FromImage((Image)(object)val);
            val2.TextRenderingHint = (TextRenderingHint)4;
            val2.DrawString("Invalid Signature", new Font("ARIAL", 9f), Brushes.Red, new PointF(150f, 5f));
            if (_signInfo.isTrailSignature)
            {
                val2.DrawString("TRAIL", new Font("ARIAL", 15f), Brushes.Red, new PointF(5f, 5f));
            }

            if (_signInfo.SigningImage != null)
            {
                Bitmap val3 = new Bitmap((Stream)new MemoryStream(_signInfo.SigningImage));
                val2.DrawImage((Image)(object)val3, new Rectangle(50, 10, 180, 60));
            }
            else if (!string.IsNullOrEmpty(_signInfo.SigningText))
            {
                val2.DrawString(_signInfo.SigningText.Trim(), new Font("ARIAL", 12f), Brushes.Black, new PointF(42f, 52f));
            }

            string[] array = _signInfo.Certificate.SubjectName.Name.Split(new char[1] { ',' });
            string text = null;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("CN="))
                {
                    text = array[i];
                    break;
                }
            }

            string text2 = text.Replace("CN=", "");
            text2.Trim();
            val2.DrawString("Signed by " + text2, new Font("ARIAL", 8f), Brushes.Black, new PointF(8f, 113f));
            MemoryStream memoryStream = new MemoryStream();
            EncoderParameters val4 = new EncoderParameters(1);
            val4.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            ((Image)val).Save((Stream)memoryStream, GetEncoder(ImageFormat.Png), val4);
            _signInfo.InvalidImgLine = memoryStream.ToArray();
            Bitmap val5 = new Bitmap((Image)(object)_signInfo.SigLineImage);
            Graphics val6 = Graphics.FromImage((Image)(object)val5);
            val6.TextRenderingHint = (TextRenderingHint)4;
            if (_signInfo.SigningImage != null)
            {
                Bitmap val7 = new Bitmap((Stream)new MemoryStream(_signInfo.SigningImage));
                val6.DrawImage((Image)(object)val7, new Rectangle(50, 10, 180, 60));
            }
            else if (!string.IsNullOrEmpty(_signInfo.SigningText))
            {
                val6.DrawString(_signInfo.SigningText, new Font("ARIAL", 12f), Brushes.Black, new PointF(42f, 52f));
            }

            val6.DrawString("Signed by: " + text2, new Font("ARIAL", 8f), Brushes.Black, new PointF(8f, 113f));
            if (_signInfo.isTrailSignature)
            {
                val6.DrawString(DateTime.Now.ToString("yyyy-MM-dd"), new Font("ARIAL", 8f), Brushes.Black, new PointF(192f, 10f));
                val6.DrawString("TRAIL", new Font("ARIAL", 15f), Brushes.Red, new PointF(5f, 5f));
            }
            else
            {
                val6.DrawString(DateTime.Now.ToString("yyyy-MM-dd"), new Font("ARIAL", 8f), Brushes.Black, new PointF(192f, 5f));
            }

            MemoryStream memoryStream2 = new MemoryStream();
            ((Image)val5).Save((Stream)memoryStream2, GetEncoder(ImageFormat.Png), val4);
            _signInfo.ValidImgLine = memoryStream2.ToArray();
            return true;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] imageDecoders = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo val in imageDecoders)
            {
                if (val.FormatID == format.Guid)
                {
                    return val;
                }
            }

            return null;
        }

        private string GetStringFromStream(Stream stream)
        {
            stream.Position = 0L;
            using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                return streamReader.ReadToEnd();
        }

        private byte[] GetBytesfromStream(Stream stream)
        {
            stream.Position = 0L;
            using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                return Encoding.ASCII.GetBytes(streamReader.ReadToEnd());
        }

        public ExcelProcessingUnit(ref XmlDocument mDomObj, ref SigningInfo _signInfo, ref OrigDocInfo _oDocInfo, ref List<DocPartInfo> _docParts)
        {
            _mainDOMObject = mDomObj;
            _docInfo = _oDocInfo;
            this._signInfo = _signInfo;
            _lstDocPartsInfo = _docParts;
        }

        public bool Process(OfficeSigningSession.AscHashingAlgo hashAlgo, ref OfficeSigningSession.AscOSDKError _error, ref Package _package)
        {
            if (_inProgress)
            {
                _error = OfficeSigningSession.AscOSDKError.IN_PROGRESS;
                return false;
            }

            switch (hashAlgo)
            {
                case OfficeSigningSession.AscHashingAlgo.SHA1:
                    _ObjHashAlgo = new SHA1CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2000/09/xmldsig#sha1";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                    break;
                case OfficeSigningSession.AscHashingAlgo.SHA256:
                    _ObjHashAlgo = new SHA256CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2001/04/xmlenc#sha256";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                    break;
                case OfficeSigningSession.AscHashingAlgo.SHA384:
                    _ObjHashAlgo = new SHA384CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2001/04/xmldsig-more#sha384";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";
                    break;
                case OfficeSigningSession.AscHashingAlgo.SHA512:
                    _ObjHashAlgo = new SHA512CryptoServiceProvider();
                    _signInfo.HashAlgo = _ObjHashAlgo;
                    _signInfo.hashAlgoString = "http://www.w3.org/2001/04/xmlenc#sha512";
                    _signInfo.hashAlgoString2 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
                    break;
                default:
                    _error = OfficeSigningSession.AscOSDKError.INVALID_HASH_ALGO;
                    return false;
            }

            _inProgress = true;
            _mainOfficePackage = _package;
            bool flag = false;
            foreach (PackagePart part in _package.GetParts())
            {
                if (part.Uri.OriginalString.Contains("/_xmlsignatures/origin.sigs"))
                {
                    _signInfo.OriginPackagaePrt = part;
                }

                if (!part.Uri.OriginalString.Contains("/xl/") && !part.Uri.OriginalString.Contains("/_xmlsignatures") && !part.Uri.OriginalString.Contains("_rels/.rels"))
                {
                    continue;
                }

                if (_desiredSignLine != null)
                {
                    flag = part.Uri.OriginalString.Contains(_desiredSignLine.SigLineImageID);
                }

                DocPartInfo docPartInfo = new DocPartInfo();
                docPartInfo.partUri = part.Uri;
                if (part.ContentType.Contains("application/vnd.openxmlformats-package.relationships+xml"))
                {
                    List<string> list = new List<string>();
                    bool flag2 = false;
                    bool flag3 = false;
                    if (docPartInfo.partUri.OriginalString.Contains("workbook.xml.rels"))
                    {
                        flag2 = true;
                    }
                    else if (docPartInfo.partUri.OriginalString.Contains("_rels/.rels"))
                    {
                        flag3 = true;
                    }

                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.PreserveWhitespace = false;
                    Stream stream = part.GetStream();
                    xmlDocument.Load(stream);
                    XmlNodeList childNodes = xmlDocument.DocumentElement.ChildNodes;
                    List<SortXmlNodes> list2 = new List<SortXmlNodes>();
                    XmlElement documentElement = xmlDocument.DocumentElement;
                    bool flag4 = false;
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        XmlNode xmlNode = childNodes[i];
                        string value = xmlNode.Attributes.GetNamedItem("Id").Value;
                        string value2 = xmlNode.Attributes.GetNamedItem("Type").Value;
                        string value3 = xmlNode.Attributes.GetNamedItem("Target").Value;
                        if (flag)
                        {
                            for (int j = 0; j < _lstSigLines.Count; j++)
                            {
                                if (_lstSigLines[j].OfficeID.Contains(_desiredSignLine.OfficeID))
                                {
                                    string sigImageUri = value3.Replace("../", "");
                                    _lstSigLines[j].SigImageUri = sigImageUri;
                                }
                            }

                            flag = false;
                        }

                        if (flag2)
                        {
                            if (xmlNode.NodeType == XmlNodeType.Element)
                            {
                                if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/customXml"))
                                {
                                    flag4 = true;
                                }
                                else if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"))
                                {
                                    flag4 = true;
                                }
                                else if (value2.Contains("http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"))
                                {
                                    flag4 = true;
                                }
                                else if (value2.Contains("http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/origin"))
                                {
                                    flag4 = true;
                                }
                                else if (value2.Contains("http://schemas.openxmlformats.org/package/2006/relationships/metadata/thumbnail"))
                                {
                                    flag4 = true;
                                }
                                else if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/presProps"))
                                {
                                    flag4 = true;
                                }
                                else if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/viewProps"))
                                {
                                    flag4 = true;
                                }

                                if (flag4)
                                {
                                    flag4 = false;
                                    documentElement.RemoveChild(xmlNode);
                                    i--;
                                    continue;
                                }

                                list2.Add(new SortXmlNodes
                                {
                                    _xmlNodeID = value,
                                    _xmlNodeTarget = value3,
                                    _xmlNodeType = value2,
                                    _xmlNodeStr = xmlNode.OuterXml
                                });
                                list.Add(value);
                            }
                        }
                        else if (flag3)
                        {
                            if (value2.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"))
                            {
                                list2.Add(new SortXmlNodes
                                {
                                    _xmlNodeID = value,
                                    _xmlNodeTarget = value3,
                                    _xmlNodeType = value2,
                                    _xmlNodeStr = xmlNode.OuterXml
                                });
                                list.Add(value);
                            }
                        }
                        else
                        {
                            list2.Add(new SortXmlNodes
                            {
                                _xmlNodeID = value,
                                _xmlNodeTarget = value3,
                                _xmlNodeType = value2,
                                _xmlNodeStr = xmlNode.OuterXml
                            });
                            list.Add(value);
                        }
                    }

                    xmlDocument.DocumentElement.RemoveAll();
                    list2.Sort();
                    for (int k = 0; k < list2.Count; k++)
                    {
                        XmlElement xmlElement = xmlDocument.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships");
                        xmlElement.SetAttribute("Id", list2[k]._xmlNodeID);
                        xmlElement.SetAttribute("Target", list2[k]._xmlNodeTarget);
                        xmlElement.SetAttribute("Type", list2[k]._xmlNodeType);
                        if (list2[k]._xmlNodeType.Contains("http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink"))
                        {
                            xmlElement.SetAttribute("TargetMode", "External");
                        }
                        else
                        {
                            xmlElement.SetAttribute("TargetMode", "Internal");
                        }

                        xmlDocument.DocumentElement.AppendChild(xmlElement);
                    }

                    XmlDsigC14NTransform val = new XmlDsigC14NTransform(false);
                    (val).LoadInput((object)xmlDocument);
                    string hash = Convert.ToBase64String((val).GetDigestedOutput(_signInfo.HashAlgo));
                    docPartInfo.hash = hash;
                    XmlElement xmlElement2 = MakeRefernceObjectRelationship(docPartInfo.partUri.OriginalString + "?ContentType=" + part.ContentType, docPartInfo.hash, list);
                    XmlDsigC14NTransform val2 = new XmlDsigC14NTransform();
                    XmlNodeList elementsByTagName = xmlElement2.GetElementsByTagName("Transforms");
                    (val2).LoadInput((object)elementsByTagName);
                    GetStringFromStream((Stream)(val2).GetOutput());
                    docPartInfo.xmlElement = xmlElement2;
                    _lstDocPartsInfo.Add(docPartInfo);
                }
                else
                {
                    if (part.Uri.OriginalString.Contains("/xl/drawings/vmlDrawing") && _desiredSignLine == null)
                    {
                        XmlDocument xmlDocument2 = new XmlDocument();
                        xmlDocument2.PreserveWhitespace = false;
                        Stream stream2 = part.GetStream();
                        xmlDocument2.Load(stream2);
                        OfficeSigningSession.AscOSDKError _error2 = OfficeSigningSession.AscOSDKError.INVALID_SETUP_ID;
                        bool _refSGFound = false;
                        FindSignatureLine(part.Uri.OriginalString.Split(new char[1] { '/' })[1], xmlDocument2, _signInfo.SetupID, ref _error2, ref _refSGFound);
                        stream2.Close();
                    }

                    Stream stream3 = part.GetStream();
                    byte[] array = new byte[stream3.Length];
                    stream3.Read(array, 0, Convert.ToInt32(stream3.Length));
                    docPartInfo.originalXml = ((!part.Uri.OriginalString.Contains(".xml")) ? Convert.ToBase64String(array) : GetStringFromStream(stream3));
                    docPartInfo.isImage = true;
                    docPartInfo.hash = Convert.ToBase64String(_ObjHashAlgo.ComputeHash(array));
                    XmlElement xmlElement3 = MakeRefernceObject(docPartInfo.partUri.OriginalString + "?ContentType=" + part.ContentType, docPartInfo.hash);
                    docPartInfo.xmlElement = xmlElement3;
                    _lstDocPartsInfo.Add(docPartInfo);
                }
            }
            return true;
        }
    }
}
