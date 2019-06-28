using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MiddlewaresDemo.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        [HttpPost]
        public IActionResult Post([FromBody]JObject jObject)
        {
            var name = jObject["name"]?.ToString();
            var u = new User();
            if (!string.IsNullOrWhiteSpace(name))
                u.name = name;
            return new JsonResult(u);
        }
    }

    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public User()
        {
            this.id = 1001;
            this.name = "张三";
            this.age = 18;
        }
    }
}