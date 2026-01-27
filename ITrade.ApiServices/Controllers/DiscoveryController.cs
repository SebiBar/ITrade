using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("discovery")]
    public class DiscoveryController(
        IMatchingService matchingService,
        ISearchingService searchingService): ControllerBase
    {
    }
}
