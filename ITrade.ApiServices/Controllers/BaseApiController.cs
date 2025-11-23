using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected int GetUserId()
        {
            return 1;
        }
    }
}
