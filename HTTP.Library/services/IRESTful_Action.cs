using HTTP.Library.models;
using HTTP.Library.utils;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HTTP.Library.services
{
    public interface IRESTful_Action
    {
        /// <summary>
        /// Hàm call RESTful
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
        response_data<R> Call_Request<P, R>(HttpMethod method, string url, List<header_param> list_header,  P model, DataType body_type = DataType.NONE, bool is_without_execute_return_respone = false);
    }
}
