using ProjectESignSDKDemo.action;
using ProjectESignSDKDemo.models;
using ProjectESignSDKDemo.models_esign;
using HTTP.Library.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;
using HTTP.Library.utils;
using System.Threading;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.ComponentModel;
using iText.Layout.Element;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Xml;
using System.Reflection;

namespace ProjectESignSDKDemo
{
    public partial class frmMain : Form
    {
        List<CertificateRespone> list_cert;
        private string _clientId = "";
        private string _clientKey = "";

        private DocumentType TypeFile { get; set; }

        private CommonFunction commonFunction;

        frmLogin frmLogin;
        public frmMain(frmLogin frmLogin)
        {
            InitializeComponent();
            _clientId = Program.Configuration["ClientID"];
            _clientKey = Program.Configuration["ClientKEY"];

            commonFunction = new CommonFunction();
            this.frmLogin = frmLogin;
        }

        private void btnPathChooseSign_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Excel file (*.pdf;*.xml;*.docx;*.xlsx)|*.pdf;*.xml;*.docx;*.xlsx";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPathSign.Text = ofd.FileName;
                    string ext = Path.GetExtension(txtPathSign.Text);
                    switch (ext)
                    {
                        case ".pdf":
                            TypeFile = DocumentType.Pdf;
                            break;
                        case ".xml":
                            TypeFile = DocumentType.Xml;
                            break;
                        case ".docx":
                            TypeFile = DocumentType.Word;
                            break;
                        case ".xlsx":
                            TypeFile = DocumentType.Excel;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// hash file
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        private CalculateLocalHashResult hash_document(CertificateRespone cert)
        {
            // Khởi tạo thông tin của file để ký và thông tin để gắn ảnh chữ ký vào file tài liệu về sau
            List<PdfDocToHash> list_pdf = new List<PdfDocToHash>();
            List<XmlDocToHash> list_xml = new List<XmlDocToHash>();
            List<ExcelDocToHash> list_excel = new List<ExcelDocToHash>();
            List<WordDocToHash> list_word = new List<WordDocToHash>();
            switch (TypeFile)
            {
                case DocumentType.Pdf:
                    list_pdf = new List<PdfDocToHash>(){
                        new PdfDocToHash
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                            // Trong thực tế cần cho người dùng upload file cần ký
                            FileToSign = File.ReadAllBytes(txtPathSign.Text),
                            // Thiết lập chữ ký
                            SignatureInfo = new PdfSignatureInfo(
                                // Vị trí theo trục X, hướng trái phải
                                positionX: 380,
                                // Vị trí theo trục Y, hướng trên dưới
                                positionY: 160,
                                // Chiều rộng của chữ ký
                                width: 100,
                                // Chiều cao của chữ ký
                                height: 50,
                                // Cỡ chữ của phần text
                                fontSize: 10,
                                // Màu chữ của phần text
                                textColor: PDFSignatureColor.Black,
                                // Chế độ hiển thị, hiện đang chọn là có cả ảnh và text
                                renderingMode: RenderingMode.GraphicAndDescription,
                                // Phần hình ảnh chữ ký
                                signatureImage: Convert.ToBase64String(File.ReadAllBytes(@"Samples/signature.png")),
                                signatureDescription: new PdfSignatureDescription()
                                {
                                    DisplayText = "Ký bởi Big Cat\nMình thích thì mình ký\nthôi",
                                },
                                // Trang cần ký, trang đầu tiên là trang 1
                                page: 1,
                                // Tên chữ ký, không trùng với tên chữ ký khác trong file
                                signatureName: Guid.NewGuid().ToString(),
                                // Ảnh logo của đơn vị/người ký, có thể bỏ qua
                                logoImage: File.ReadAllBytes(@"Samples/logo.png")
                            )
                        }
                    };
                    break;
                case DocumentType.Xml:
                    list_xml = new List<XmlDocToHash>(){
                        new XmlDocToHash()
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                            // Đọc file XML, lưu ý cần minify file XML trước khi ký
                            FileToSign = File.ReadAllText(txtPathSign.Text),
                            // Thiết lập chữ ký
                            SignatureInfo = new XmlSignatureInfoDto("seller", new List<string> { "#1VUPFJ59D", "#SignTime_1" },  "//HDon/DSCKS/NBan",true,  "SignTime_1")
                        }
                    };
                    break;
                case DocumentType.Word:
                    list_word = new List<WordDocToHash>(){
                        new WordDocToHash()
                        {
                            FileToSign = File.ReadAllBytes(txtPathSign.Text),
                            DocumentId = Guid.NewGuid().ToString(),
                        }
                    };
                    break;
                case DocumentType.Excel:
                    list_excel = new List<ExcelDocToHash>(){
                        new ExcelDocToHash()
                        {
                            FileToSign = File.ReadAllBytes(txtPathSign.Text),
                            DocumentId = Guid.NewGuid().ToString(),
                        }
                    };
                    break;
            }

            var calculateLocalHashData = new CalculateLocalHashData
            {
                Certificate = Convert.FromBase64String(cert.certificate),
                CertificateChain = cert.certiticateChain,
                DocumentType = TypeFile,
                PdfDocs = list_pdf,
                XmlDocs = list_xml,
                ExcelDocs = list_excel,
                WordDocs = list_word,

            };

            // Hash file
            CalculateLocalHashResult info_file_hase = commonFunction.CalculateLocalHash(calculateLocalHashData);

            return info_file_hase;
        }

        /// <summary>
        /// ký file đã hash
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="fileName"></param>
        /// <param name="info_file_hash"></param>
        /// <returns></returns>
        private response_data<SignRespone> signing_file_hash(CertificateRespone cert, string fileName, CalculateLocalHashResult info_file_hash)
        {
            List<Document> list_doc = new List<Document>();
            switch (TypeFile)
            {
                case DocumentType.Pdf:
                    list_doc = new List<Document>()
                {
                    new Document(
                        info_file_hash.PdfDocs[0].DocumentId,
                        Convert.ToBase64String(info_file_hash.PdfDocs[0].DocumentHash),
                        fileName
                        )
                };
                    break;
                case DocumentType.Xml:
                    list_doc = new List<Document>()
                {
                    new Document(
                        info_file_hash.XmlDocs[0].DocumentId,
                        Convert.ToBase64String(info_file_hash.XmlDocs[0].Digest),
                        fileName
                        )
                };
                    break;
                case DocumentType.Word:
                    list_doc = new List<Document>()
                {
                    new Document(
                        info_file_hash.WordDocs[0].DocumentId,
                        Convert.ToBase64String(info_file_hash.WordDocs[0].Digest),
                        fileName
                        )
                };
                    break;
                case DocumentType.Excel:
                    list_doc = new List<Document>()
                {
                    new Document(
                        info_file_hash.ExcelDocs[0].DocumentId,
                        Convert.ToBase64String(info_file_hash.ExcelDocs[0].Digest),
                        fileName
                        )
                };
                    break;

            }

            // Khởi tạo param ký file đã hash
            SignModel param = new SignModel(
                $"Ký file {fileName}",
                cert.userId,
                cert.keyAlias,
                list_doc
            );

            List<header_param> header_Params = new List<header_param>();
            header_Params.Add(new header_param("AuthorizationRM", $"Bearer {Program.TOKEN}"));
            header_Params.Add(new header_param("x-clientid", $"{_clientId}"));
            header_Params.Add(new header_param("x-clientkey", $"{_clientKey}"));

            // Gọi api ký
            response_data<SignRespone> signing = Program.RESTful_Action
                .Call_Request<SignModel, SignRespone>(
                HttpMethod.Post,
                 Program.Configuration["URL_SIGNING_HASHED_FILE"],
                header_Params,
                param,
                DataType.JSON);

            return signing;
        }

        /// <summary>
        /// check trạng thái của file đã ký
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        private SignChecker signed_status(string transactionId)
        {
            List<header_param> header_Params = new List<header_param>();
            header_Params.Add(new header_param("AuthorizationRM", $"Bearer {Program.TOKEN}"));
            header_Params.Add(new header_param("x-clientid", $"{_clientId}"));
            header_Params.Add(new header_param("x-clientkey", $"{_clientKey}"));

            // Gọi api ký
            SignChecker signchecker = Program.RESTful_Action
                    .Call_Request<null_model, SignChecker>(HttpMethod.Get,
                    String.Format(Program.Configuration["URL_SIGNING_STATUS"], transactionId),
                    header_Params,
                    null).data;

            return signchecker;
        }

        /// <summary>
        /// Gắn chữ ký số vào file đã ký và lưu file
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="info_file_hash"></param>
        /// <param name="signchecker"></param>
        private void attach_digital_signature(CertificateRespone cert, CalculateLocalHashResult info_file_hash, SignChecker signchecker)
        {
            var attachPdfDatas = new List<AttachSignaturePdfData>();
            var attachXmlDatas = new List<AttachSignatureXmlData>();
            var attachWordDatas = new List<AttachSignatureWordData>();
            var attachExcelDatas = new List<AttachSignatureExcelData>();

            int _count = 0;
            string _filter = "";
            switch (TypeFile)
            {
                case DocumentType.Pdf:
                    _filter = "PDF File|*.pdf";
                    _count = info_file_hash.PdfDocs.Count;
                    attachPdfDatas.Add(new AttachSignaturePdfData()
                    {
                        Digest = info_file_hash.PdfDocs[0].Digest,
                        DocumentHash = info_file_hash.PdfDocs[0].DocumentHash,
                        Sh = info_file_hash.PdfDocs[0].Sh,
                        Signature = Encoding.ASCII.GetBytes(signchecker.signatures[0].signature),
                        SignatureName = info_file_hash.PdfDocs[0].SignatureName,
                        DocumentBytes = info_file_hash.PdfDocs[0].DocumentBytes,
                        DocumentId = info_file_hash.PdfDocs[0].DocumentId
                    });
                    break;
                case DocumentType.Xml:
                    _filter = "XML File|*.xml";
                    _count = info_file_hash.XmlDocs.Count;
                    attachXmlDatas.Add(new AttachSignatureXmlData()
                    {
                        Digest = info_file_hash.XmlDocs[0].Digest,
                        Sh = info_file_hash.XmlDocs[0].Sh,
                        Signature = Encoding.ASCII.GetBytes(signchecker.signatures[0].signature),
                        DocumentId = info_file_hash.XmlDocs[0].DocumentId,
                        Document = info_file_hash.XmlDocs[0].Document,
                        SignatureId = info_file_hash.XmlDocs[0].SignatureId
                    });
                    break;
                case DocumentType.Word:
                    _filter = "DOCX File|*.docx";
                    _count = info_file_hash.WordDocs.Count;
                    attachWordDatas.Add(new AttachSignatureWordData()
                    {
                        Digest = info_file_hash.WordDocs[0].Digest,
                        Signature = Encoding.ASCII.GetBytes(signchecker.signatures[0].signature),
                        DocumentId = info_file_hash.WordDocs[0].DocumentId,
                        SignatureId = info_file_hash.WordDocs[0].SignatureId,
                        DocumentBytes = info_file_hash.WordDocs[0].DocumentBytes,
                        MainDom = info_file_hash.WordDocs[0].MainDom
                    });
                    break;
                case DocumentType.Excel:
                    _filter = "XLSX File|*.xlsx";
                    _count = info_file_hash.ExcelDocs.Count;
                    attachExcelDatas.Add(new AttachSignatureExcelData()
                    {
                        Digest = info_file_hash.ExcelDocs[0].Digest,
                        Signature = Encoding.ASCII.GetBytes(signchecker.signatures[0].signature),
                        DocumentId = info_file_hash.ExcelDocs[0].DocumentId,
                        SignatureId = info_file_hash.ExcelDocs[0].SignatureId,
                        DocumentBytes = info_file_hash.ExcelDocs[0].DocumentBytes,
                        MainDom = info_file_hash.ExcelDocs[0].MainDom
                    });
                    break;

            }

            for (int i = 0; i < _count; i++)
            {
                
            }
            //Gọi API gắn chữ ký vào file
            var attachSignatureResult = commonFunction.AttachSignature(new AttachSignatureData()
            {
                PdfDocs = attachPdfDatas,
                XmlDocs = attachXmlDatas,
                WordDocs = attachWordDatas,
                ExcelDocs = attachExcelDatas,
                Certififcate = Convert.FromBase64String(cert.certificate),
                CertififcateChain = cert.certiticateChain,
                DocumentType = TypeFile,
            });

            // mở cửa sổ lưu file
            // Tạo 1 thread để thực hiện việc mở SaveFileDialog
            var t = new Thread((ThreadStart)(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = _filter;
                saveFileDialog.Title = "Lưu tài liệu";
                saveFileDialog.FileName = "Test";

                DialogResult result = saveFileDialog.ShowDialog();
                saveFileDialog.RestoreDirectory = true;

                if (result == DialogResult.OK && saveFileDialog.FileName != "")
                {
                    try
                    {
                        if (saveFileDialog.CheckPathExists)
                        {
                            switch (TypeFile)
                            {
                                case DocumentType.Xml:
                                    File.WriteAllText(saveFileDialog.FileName, attachSignatureResult.XmlDocs[0].Document);
                                    break;
                                case DocumentType.Pdf:
                                    File.WriteAllBytes(saveFileDialog.FileName, attachSignatureResult.PdfDocs[0].Document);
                                    break;
                                case DocumentType.Word:
                                    File.WriteAllBytes(saveFileDialog.FileName, attachSignatureResult.WordDocs[0].Document);
                                    break;
                                case DocumentType.Excel:
                                    File.WriteAllBytes(saveFileDialog.FileName, attachSignatureResult.ExcelDocs[0].Document);
                                    break;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Given Path does not exist");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        private void run_backgroud(Action action)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    action.Invoke();
                });
            bw.RunWorkerAsync();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmLogin.Close();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove("TOKEN");
            config.AppSettings.Settings.Add("TOKEN", "");
            config.Save(ConfigurationSaveMode.Modified);
            Program.TOKEN = string.Empty;

            frmLogin.Show();
            this.Hide();
        }

        private void btnImgSignature_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image file (*.png;*.jpg;*.jpeg;)|*.png;*.jpg;*.jpeg;";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImgSignature.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnImgLogo_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image file (*.png;*.jpg;*.jpeg;)|*.png;*.jpg;*.jpeg;";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImgLogo.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        response_data<SignRespone> _res_signing = new response_data<SignRespone>();
        CertificateRespone _cert;
        CalculateLocalHashResult _file_to_sign;
        private void btnSign_Click(object sender, EventArgs e)
        {
            string message_sign = "Gọi api ký thất bại!"; // Thông báo ký
            lblNotification.Text = "Đang thực hiện ký!";
            // Lấy filename
            string fileName = Path.GetFileName(txtPathSign.Text);

            // Lấy chữ ký số được chọn để ký
            _cert = list_cert[cbbList_Cert.SelectedIndex];

            btnLogout.Enabled = false;
            btnPathChooseSign.Enabled = false;
            btnImgLogo.Enabled = false;
            btnImgSignature.Enabled = false;
            btnSign.Enabled = false;
            cbbList_Cert.Enabled = false;
            txtPathSign.Enabled = false;
            txtImgLogo.Enabled = false;
            txtImgSignature.Enabled = false;
            txtContentSign.Enabled = false;

            run_backgroud(() =>
            {
                string _trasid = "";
                bool _tras_button = false;
                try
                {
                    // Hash file
                    _file_to_sign = hash_document(_cert);
                    // Gọi api ký
                    _res_signing = signing_file_hash(_cert, fileName, _file_to_sign);

                    if (_res_signing.code == (int)HttpStatusCode.OK && !string.IsNullOrEmpty(_res_signing.data.transactionId))
                    {
                        _trasid = _res_signing.data.transactionId;
                        _tras_button = true;
                    }
                    else
                    {
                        _trasid = "";
                        MessageBox.Show(message_sign, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show( "Đã có lỗi sảy ra: " + ex.ToString(), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Action action_loader = () =>
                {
                    txtTransID.Text = _trasid;
                    btnSeachTransID.Enabled = _tras_button;
                };
                lblNotification.Invoke(action_loader);
            });
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            GetCer();
        }

        private void btnSeachTransID_Click(object sender, EventArgs e)
        {
            string message_sign = "Gọi api ký thất bại!"; // Thông báo ký
            message_sign = "";
            SignRespone signing = _res_signing.data;

            bool checkout = false; // biến điều khiển vòng lặp trong 120s
            bool is_signed = false; // biến check xem đã ký hay bị từ chối
            SignChecker signchecker = new SignChecker(); // Khởi tạo kết quả gọi api check trạng thái ký

            btnSeachTransID.Enabled = false;
            run_backgroud(() =>
            {
                string _trasid = "";
                bool _tras_button = false;
                try
                {
                    // vòng lặp check ký trong thời gian định trước
                    while (!checkout)
                    {
                        // gọi api check trạng thái ký 
                        signchecker = signed_status(signing.transactionId);

                        if (signchecker.status == "SUCCESS") // ký thành công
                        {
                            is_signed = true;
                            checkout = true;
                            message_sign = "Ký thành công!";
                        }
                        else if (signchecker.status == "FAILED") // ký thất bại
                        {
                            checkout = true;
                            message_sign = "Phiên ký bị từ chối!";
                        }
                        Thread.Sleep(2000);
                    }
                    if (is_signed) // nếu ký thành công
                    {
                        MessageBox.Show(message_sign, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        attach_digital_signature(_cert, _file_to_sign, signchecker);
                    }
                    else
                    {
                        MessageBox.Show(message_sign, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã có lỗi sảy ra: " + ex.ToString(), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Action action_loader = () =>
                {
                    lblNotification.Text = message_sign;
                    btnLogout.Enabled = true;
                    btnPathChooseSign.Enabled = true;
                    btnImgSignature.Enabled = true;
                    btnImgLogo.Enabled = true;
                    btnSign.Enabled = true;
                    cbbList_Cert.Enabled = true;
                    txtImgLogo.Enabled = true;
                    txtImgSignature.Enabled = true;
                    txtPathSign.Enabled = true;
                    txtContentSign.Enabled = true;
                    btnSeachTransID.Enabled = true;
                };
                lblNotification.Invoke(action_loader);
            });

            
        }

        private void btnGetCer_Click(object sender, EventArgs e)
        {
            GetCer();
        }

        private void GetCer()
        {
            run_backgroud(() =>
            {
                Action action_lblNotification = () =>
                {
                    lblNotification.Text = "Đang lấy danh sách chữ ký số!";
                };
                lblNotification.Invoke(action_lblNotification);

                List<header_param> header_Params = new List<header_param>();
                header_Params.Add(new header_param("Authorization", $"Bearer {Program.TOKEN}"));
                header_Params.Add(new header_param("x-clientid", $"{_clientId}"));
                header_Params.Add(new header_param("x-clientkey", $"{_clientKey}"));
                response_data<List<CertificateRespone>> certs = Program.RESTful_Action
                                                                       .Call_Request<null_model, List<CertificateRespone>>(
                                                                            HttpMethod.Get,
                                                                            Program.Configuration["URL_GET_CERT"],
                                                                            header_Params,
                                                                            null);
                if (certs.code == (int)HttpStatusCode.OK && certs.data.Count > 0)
                {
                    list_cert = certs.data;
                    list_cert.ForEach(item => {
                        item.certificate = item.certificate.Replace("\r\n", "");
                    });
                    Action action_loader = () =>
                    {
                        foreach (var item in list_cert)
                        {
                            cbbList_Cert.Items.Add(item.certKey);
                        }
                        cbbList_Cert.SelectedIndex = 0;

                        btnLogout.Enabled = true;
                        btnImgLogo.Enabled = true;
                        btnImgSignature.Enabled = true;
                        btnPathChooseSign.Enabled = true;
                        btnSign.Enabled = true;
                    };
                    cbbList_Cert.Invoke(action_loader);
                    action_lblNotification = () =>
                    {
                        lblNotification.Text = "";
                    };
                    lblNotification.Invoke(action_lblNotification);
                }
                else
                {
                    if (certs.code == (int)HttpStatusCode.OK)
                    {
                        action_lblNotification = () =>
                        {
                            lblNotification.Text = "Không chữ ký số nào trong tài khoản!";
                        };
                        lblNotification.Invoke(action_lblNotification);
                    }
                    else
                    {
                        action_lblNotification = () =>
                        {
                            lblNotification.Text = "Gọi api lấy danh sách chữ ký số bị lỗi, ấn lấy CTS lại!";
                        };
                        lblNotification.Invoke(action_lblNotification);
                    }
                }
            });
        }
    }
}
