using Microsoft.AspNetCore.Mvc;

namespace HttpTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JsonController : ControllerBase
    {
        private readonly ILogger<JsonController> _logger;
        private readonly IJsonService jsonService;

        public JsonController(ILogger<JsonController> logger, IJsonService jsonService)
        {
            _logger = logger;
            this.jsonService = jsonService;
        }

        [HttpGet(Name = "GetJson")]
        public async Task<string> Get()
        {
            var response = await jsonService.GetJsonAsync("json");
            Console.WriteLine(response);
            return response;
        }
    }
}
