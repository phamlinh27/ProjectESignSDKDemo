namespace HTTP.Library.utils
{
    public class Message_Constant
    {
        /// <summary>
        /// Trả về response của request
        /// </summary>
        public const string RESPONSE = "response: {0}";

        /// <summary>
        /// Gọi đến mà http status không phải 200
        /// </summary>
        public const string NOT_OK = "Gọi tới {0} không thành công\r\nresponse: {1}";

        /// <summary>
        /// Gọi đến mà http status không phải 200
        /// </summary>
        public const string EXCEPTION = "Đã có lỗi xảy ra khi call tới {0}\r\nerror: {1}\r\nresponse: {2}";

        /// <summary>
        /// Quá thời gian xử lý của request
        /// </summary>
        public const string TIMEOUT = "Request Timeout!";
    }

    /// <summary>
    /// Enum các loại data đẩy lên request
    /// </summary>
    public enum DataType
    {
        NONE,
        FORM_DATA,
        X_WWW_FORM_URLENCODED,
        JSON,
        ARRAY_AS_JSON,
        STRING_AS_JSON
    }
}
