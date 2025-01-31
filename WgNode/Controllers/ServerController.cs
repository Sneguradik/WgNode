using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WgNode.Dto;
using WgNode.Models;
using WgNode.Services;

namespace WgNode.Controllers
{
    [Route("server")]
    [ApiController]
    public class ServerController(IServerService serverService, IServerStorage serverStorage) : ControllerBase
    {
        [HttpGet]
        public ServerDto GetServer() => new (serverStorage.Server);
        
        [HttpGet("launch")]
        public async Task LaunchServer() => await serverService.LaunchServer();
        
        [HttpGet("stop")]
        public async Task StopServer() => await serverService.StopServer();
    }
}
