using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Log4NetDemo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger; //注入日志
        public ValuesController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValuesController>();
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var result = new string[] { "value1", "value2" };
            _logger.LogDebug($"返回信息：{string.Join(",",result)}");
            return result;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            try
            {
                //do
                throw new Exception($"我要报错,传入ID {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("错误："+ ex.Message);
                throw ex;
            }
        }
    }
}
