using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WgNode.Dto;
using WgNode.Models;
using WgNode.Services;

namespace WgNode.Controllers
{
    [Route("peers")]
    [ApiController]
    public class PeersControllers(MainDbContext context, IPeerService peerService, IServerService serverService, ILogger<PeersControllers> logger) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IEnumerable<Peer>> GetAllPeers()
        {
            logger.LogInformation("Get all peers");
            return await context.Peers.AsNoTracking().ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Peer>> GetPeerById(Guid id)
        {
            var peer = await context.Peers.FindAsync(id);
            logger.LogInformation($"Get peer with id: {id}");
            if (peer is null)
            {
                logger.LogError($"Get peer with id: {id} not found");
                return NotFound("Peer not found");
            }
            return peer;
        }

        [HttpPost("create_peer")]
        public async Task<ActionResult<Peer>> PostPeer([FromBody] CreatePeerDto dto)
        {
            logger.LogInformation($"Create peer: {dto.Name}");
            var lastPeer = await context.Peers.AsNoTracking().OrderBy(e=>e.CreatedAt).LastOrDefaultAsync();
            var peer = peerService.CreatePeer(dto.Name, 
                peerService.CalculateNextPeerAddress(lastPeer?.IpAddress??[10,20,0,1]));
            peer.Active = true;
            await peerService.AddPeerToDb(peer);
            await serverService.UpdateServer();
            return peer;
        }

        [HttpPost("create_peers")]
        public async Task<ActionResult<List<Peer>>> PostPeers([FromBody] CreatePeersDto dto)
        {
            var lastPeerAddress = (await context.Peers
                .AsNoTracking()
                .OrderBy(e=>e.CreatedAt)
                .LastOrDefaultAsync())?.IpAddress??[10,20,0,2];
            List<Peer> peers = new ();
            for (int i = 0; i < dto.Names.Count(); i++)
            {
                peers.Add(peerService.CreatePeer(dto.Names.ElementAt(i), 
                    peerService.CalculateNextPeerAddress(lastPeerAddress, i)));
            }
            foreach (var peer in peers) peer.Active = true;
            await peerService.AddPeerToDb(peers);
            await serverService.UpdateServer();
            return peers;
        }

        [HttpPut]
        public async Task PutPeer([FromBody] UpdatePeerDto dto)
        {
            logger.LogInformation($"Update peer: {dto.Name}");
            var peer = await context.Peers.FindAsync(dto.Id);
            if (peer is null) return;
            peer.Name = dto.Name;
            await context.SaveChangesAsync();
        }

        [HttpDelete("delete_peer")]
        public async Task DeletePeer([FromBody] IdPeerDto dto)
        {
            logger.LogInformation($"Delete peer: {dto.Id}");
            var peer = await context.Peers.FindAsync(dto.Id);
            if (peer is null) return;
            await peerService.RemovePeerFromDb(peer);
            await serverService.UpdateServer();
        }

        [HttpDelete("delete_peers")]
        public async Task DeletePeers([FromBody] IdsPeersDto dto)
        {
            logger.LogInformation("Delete peers");
            var peers = await context.Peers.Where(p=>dto.Ids.Contains(p.Id)).ToListAsync();
            await peerService.RemovePeerFromDb(peers);
            await serverService.UpdateServer();
        }

        [HttpPost("activate_peer")]
        public async Task ActivatePeer([FromBody] IdPeerDto dto)
        {
            logger.LogInformation($"Activate peer: {dto.Id}");
            var peer = await context.Peers.FindAsync(dto.Id);
            if (peer is null) return;
            peer.Active = true;
            await context.SaveChangesAsync();
            await serverService.UpdateServer();
        }
        
        [HttpPost("activate_peers")]
        public async Task ActivatePeers([FromBody] IdsPeersDto dto)
        {
            logger.LogInformation($"Activate peers");
            var peers = await context.Peers.Where(p=>dto.Ids.Contains(p.Id)).ToListAsync();
            foreach (var peer in peers)peer.Active = true;
            await context.SaveChangesAsync();
            await serverService.UpdateServer();
        }
        
        [HttpPost("deactivate_peer")]
        public async Task DeactivatePeer([FromBody] IdPeerDto dto)
        {
            logger.LogInformation($"Activate peer: {dto.Id}");
            var peer = await context.Peers.FindAsync(dto.Id);
            if (peer is null) return;
            peer.Active = false;
            await context.SaveChangesAsync();
            await serverService.UpdateServer();
        }
        
        [HttpPost("deactivate_peers")]
        public async Task DeactivatePeers([FromBody] IdsPeersDto dto)
        {
            logger.LogInformation($"Activate peers");
            var peers = await context.Peers.Where(p=>dto.Ids.Contains(p.Id)).ToListAsync();
            foreach (var peer in peers)peer.Active = false;
            await context.SaveChangesAsync();
            await serverService.UpdateServer();
        }
        
    }
}
