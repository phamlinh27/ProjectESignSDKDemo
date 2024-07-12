namespace ProjectESignSDKDemo.models
{
    public class ESignAuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class ESignAuthResponse
    {
        public Status status { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string accessToken { get; set; }
        public string remoteSigningAccessToken { get; set; }
        public string tokenType { get; set; }
        public int expiresIn { get; set; }
        public string refreshToken { get; set; }
        public User user { get; set; }
        public object @default { get; set; }
        public VerifyUser verifyUser { get; set; }
    }

    public class Status
    {
        public string type { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public bool error { get; set; }
        public int errorCode { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string email { get; set; }
        public object phoneNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public object username { get; set; }
    }

    public class VerifyUser
    {
        public bool emailsVerify { get; set; }
        public bool phoneNumberIsVerify { get; set; }
        public bool isChangePassword { get; set; }
    }

}
