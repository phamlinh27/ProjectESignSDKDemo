using ProjectESignSDKDemo.models;
using HTTP.Library.actions;
using HTTP.Library.models;
using HTTP.Library.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectESignSDKDemo
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            txtUsername.Text = Program.Configuration["UserName"];
            txtPassword.Text = Program.Configuration["PassWord"];

            Program.Username = Program.Configuration["UserName"];
            Program.Password = Program.Configuration["PassWord"];
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            ESignAuthRequest data = new ESignAuthRequest() { username = txtUsername.Text, password = txtPassword.Text};
            response_data<ESignAuthResponse> res = Program.RESTful_Action
                                                          .Call_Request<ESignAuthRequest,ESignAuthResponse>(
                                                               HttpMethod.Post,
                                                               Program.Configuration["URL_LOGIN"],
                                                               new List<header_param>(), 
                                                               data,
                                                               DataType.JSON);

            if(res.code != 200)
            {
                MessageBox.Show("Đăng nhập thất bại","Thông báo",MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Program.TOKEN = res.data.data.remoteSigningAccessToken;

                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                if (txtUsername.Text.Trim() != Program.Username)
                {
                    config.AppSettings.Settings.Remove("UserName");
                    config.AppSettings.Settings.Add("UserName", txtUsername.Text);
                }
                if (txtPassword.Text.Trim() != Program.Password)
                {
                    config.AppSettings.Settings.Remove("PassWord");
                    config.AppSettings.Settings.Add("PassWord", txtPassword.Text);
                }

                config.AppSettings.Settings.Remove("TOKEN");
                config.AppSettings.Settings.Add("TOKEN", Program.TOKEN);
                config.Save(ConfigurationSaveMode.Modified);
                

                frmMain frmMain = new frmMain(this);
                frmMain.Closed += (s, args) => this.Close();
                frmMain.Show();
                this.Hide();
            }
        }

        private void frmLogin_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Program.Configuration["TOKEN"]))
            {
                Program.TOKEN = Program.Configuration["TOKEN"];
                frmMain frmMain = new frmMain(this);
                frmMain.Closed += (s, args) => this.Close();
                frmMain.Show();
                this.Hide();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
