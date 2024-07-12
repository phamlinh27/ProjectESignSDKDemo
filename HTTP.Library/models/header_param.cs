namespace HTTP.Library.models
{
    public class header_param
    {
        public header_param(string _key,string _value) {
            this.key = _key;
            this.value = _value;
        }

        public string key { get; set; }
        public string value { get; set; }
    }

    public class item : header_param
    {
        public item(string _key, string _value) : base(_key, _value)
        {
        }
    }
}
