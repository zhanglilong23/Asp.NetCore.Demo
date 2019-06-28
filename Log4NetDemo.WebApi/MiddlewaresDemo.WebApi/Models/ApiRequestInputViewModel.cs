using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiddlewaresDemo.WebApi.Models
{
    public class ApiRequestInputViewModel
    {
        /// <summary>
        /// 请求接口名称
        /// </summary>
        public string RequestName { get; set; }

        /// <summary>
        /// 请求来源IP
        /// </summary>
        public string RequestIP { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// 请求类型：GET/POST
        /// </summary>
        public string HttpType { get; set; }

        /// <summary>
        /// 请求参数字符串
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// 请求报文，POST专用
        /// </summary>
        public string Body { get; set; }

        public string RequestTime { get; set; }

        public string ResponseBody { get; set; }

        public long ElapsedTime { get; set; }

        public ApiRequestInputViewModel()
        {
            this.RequestName = string.Empty;
            this.RequestIP = string.Empty;
            this.RequestUrl = string.Empty;
            this.HttpType = string.Empty;
            this.Query = string.Empty;
            this.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            this.Body = string.Empty;
            this.ResponseBody = string.Empty;
            this.ElapsedTime = -1;
        }
    }
}
