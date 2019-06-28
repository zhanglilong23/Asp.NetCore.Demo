using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiddlewaresDemo.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiddlewaresDemo.WebApi.Middlewares
{
    /// <summary>
    ///  Http 请求中间件
    /// </summary>
    public class HttpContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// 计时器
        /// </summary>
        private Stopwatch _stopwatch;
        //加密解密key
        private readonly string securitykey = "0123456789abcdef";

        /// <summary>
        /// 构造 Http 请求中间件
        /// </summary>
        /// <param name="next"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="cacheService"></param>
        public HttpContextMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<HttpContextMiddleware>();
        }

        /// <summary>
        /// 1：将Post方法中Body中的数据进行AES解密
        /// 2：将返回数据进行AES加密
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _logger.LogInformation($"Handling request: " + context.Request.Path);
            var api = new ApiRequestInputViewModel
            {
                HttpType = context.Request.Method,
                Query = context.Request.QueryString.Value,
                RequestUrl = context.Request.Path,
                RequestName = "",
                RequestIP = context.Request.Host.Value
            };

            var request = context.Request.Body;
            var response = context.Response.Body;

            try
            {
                using (var newRequest = new MemoryStream())
                {
                    //替换request流
                    context.Request.Body = newRequest;

                    using (var newResponse = new MemoryStream())
                    {
                        //替换response流
                        context.Response.Body = newResponse;

                        using (var reader = new StreamReader(request))
                        {
                            //读取原始请求流的内容
                            api.Body = await reader.ReadToEndAsync();
                            if (string.IsNullOrEmpty(api.Body))
                                await _next.Invoke(context);
                            //示例加密字符串，使用 AES-ECB-PKCS7 方式加密，密钥为：0123456789abcdef
                            // 加密参数：{"value":"哈哈哈"}
                            // 加密后数据： oedwSKGyfLX8ADtx2Z8k1Q7+pIoAkdqllaOngP4TvQ4=
                            api.Body = SecurityHelper.AESDecrypt(api.Body, securitykey);
                        }
                        using (var writer = new StreamWriter(newRequest))
                        {
                            await writer.WriteAsync(api.Body);
                            await writer.FlushAsync();
                            newRequest.Position = 0;
                            context.Request.Body = newRequest;
                            await _next(context);
                        }

                        using (var reader = new StreamReader(newResponse))
                        {
                            newResponse.Position = 0;
                            api.ResponseBody = await reader.ReadToEndAsync();
                            if (!string.IsNullOrWhiteSpace(api.ResponseBody))
                            {
                                api.ResponseBody = SecurityHelper.AESEncrypt(api.ResponseBody, securitykey);
                            }
                        }
                        using (var writer = new StreamWriter(response))
                        {
                            await writer.WriteAsync(api.ResponseBody);
                            await writer.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" http中间件发生错误: " + ex.ToString());
            }
            finally
            {
                context.Request.Body = request;
                context.Response.Body = response;
            }

            // 响应完成时存入缓存
            context.Response.OnCompleted(() =>
            {
                _stopwatch.Stop();
                api.ElapsedTime = _stopwatch.ElapsedMilliseconds;

                _logger.LogDebug($"RequestLog:{DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(0, 10000)}-{api.ElapsedTime}ms", $"{JsonConvert.SerializeObject(api)}");
                return Task.CompletedTask;
            });

            _logger.LogInformation($"Finished handling request.{_stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
