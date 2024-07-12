using HTTP.Library.models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using HTTP.Library.utils;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HTTP.Library.services
{
    internal class RESTful_Action : IRESTful_Action
    {
        /// <summary>
        /// Method RESTful api by palinh
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="method">Method gọi request, VD: HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete, ...</param>
        /// <param name="url">url được gọi</param>
        /// <param name="list_header">>List header được khai báo với object header_param dạng key value</param>
        /// <param name="model">Dữ liệu truyền lên theo Loại dữ liệu đã khai báo</param>
        /// <param name="body_type">Loại dữ liệu truyền lên request</param>
        /// <param name="is_without_execute_return_respone">Có bỏ qua xử lý dữ liệu để trả về trong respone hay không, mặc định là false</param>
        /// <returns></returns>
        public response_data<R> Call_Request<P, R>(HttpMethod method, string url, List<header_param> list_header, P model, DataType body_type = DataType.NONE, bool is_without_execute_return_respone = false) //where P : class where R : class
        {
            response_data<R> res = new response_data<R>();

            CookieContainer cookies = new CookieContainer();
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.CookieContainer = cookies;
                // khởi tạo việc lấy cookie
                using (HttpClient client = new HttpClient(handler))
                {
                    // gán header
                    list_header.ForEach(header =>
                    {
                        client.DefaultRequestHeaders.Add(header.key, header.value);
                    });

                    dynamic _content;
                    HttpRequestMessage request;

                    switch (body_type)
                    {
                        case DataType.FORM_DATA:
                            _content = (MultipartFormDataContent)Convert.ChangeType(model, typeof(MultipartFormDataContent));
                            break;
                        case DataType.X_WWW_FORM_URLENCODED:
                            _content = (FormUrlEncodedContent)Convert.ChangeType(model, typeof(FormUrlEncodedContent));
                            break;
                        case DataType.JSON:
                            _content = new StringContent(
                                JsonConvert.SerializeObject(model),
                                Encoding.UTF8,
                                "application/json"
                            );
                            break;
                        case DataType.ARRAY_AS_JSON:
                            _content = new StringContent(
                                JsonConvert.SerializeObject(model),
                                Encoding.UTF8,
                                "application/json"
                            );
                            break;
                        case DataType.STRING_AS_JSON:
                            _content = new StringContent(
                                (string)Convert.ChangeType(model, typeof(string)),
                                Encoding.UTF8,
                                "application/json"
                            );
                            break;
                        case DataType.NONE:
                        default:
                            _content = null;
                            break;
                    }
                    if (body_type != DataType.NONE)
                    {
                        request = new HttpRequestMessage
                        {
                            RequestUri = new Uri(url),
                            Method = method,
                            Content = _content
                        };
                    }
                    else
                    {
                        request = new HttpRequestMessage
                        {
                            RequestUri = new Uri(url),
                            Method = method
                        };
                    }
                    string content_of_error = "";
                    try
                    {
                        HttpResponseMessage response;
                        try
                        {
                            response = client.SendAsync(request).Result;
                        }
                        catch
                        {
                            res.code = -99;
                            res.data = (R)Convert.ChangeType(null, typeof(R));
                            res.message = Message_Constant.TIMEOUT;
                            throw new Exception();
                        }
                        using (HttpContent content = response.Content)
                        {
                            res.code = (int)response.StatusCode;
                            if (!is_without_execute_return_respone)
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    content_of_error = content.ReadAsStringAsync().GetAwaiter().GetResult().ToString();
                                    if (typeof(R) == typeof(null_model))
                                    {
                                        res.data = new response_data<R>().data;
                                    }
                                    else if (typeof(R) == typeof(string))
                                    {
                                        res.data = (R)Convert.ChangeType(content.ReadAsStringAsync().GetAwaiter().GetResult().ToString(), typeof(R));
                                    }
                                    else
                                    {
                                        res.data = JsonConvert.DeserializeObject<R>(content.ReadAsStringAsync().GetAwaiter().GetResult());
                                    }
                                    res.message = String.Format(Message_Constant.RESPONSE, content.ReadAsStringAsync().GetAwaiter().GetResult());
                                }
                                else
                                {
                                    res.data = new response_data<R>().data;
                                    res.message = String.Format(Message_Constant.NOT_OK, url, content.ReadAsStringAsync().GetAwaiter().GetResult());
                                }
                            }
                            else
                            {
                                res.data = (R)Convert.ChangeType(null, typeof(R));
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    res.message = String.Format(Message_Constant.RESPONSE, content.ReadAsStringAsync().GetAwaiter().GetResult());
                                }
                                else
                                {
                                    res.message = String.Format(Message_Constant.NOT_OK, url, content.ReadAsStringAsync().GetAwaiter().GetResult());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (res.code == 0)
                        {
                            res.code = -90;
                            res.data = (R)Convert.ChangeType(null, typeof(R));
                            res.message = String.Format(Message_Constant.EXCEPTION, url, ex, content_of_error);
                        }
                    }
                }
            }
            res.cookies = cookies;
            return res;
        }
    }
}
