using WgNode.Models;

namespace WgNode.Dto;

public record CreatePeerDto(string Name);
public record CreatePeersDto(IEnumerable<string> Names);
public record UpdatePeerDto(string Name, Guid Id);

public record IdPeerDto(Guid Id);

public record IdsPeersDto(IEnumerable<Guid> Ids);

public class ServerDto
{
    public Guid Id { get; set; }
    public string Location { get; set; } = string.Empty;
    
    public string PublicKey { get; set; } = string.Empty;
    
    public int Port { get; set; }
    
    public string Address { get; set; } = string.Empty;
    
    public bool Active { get; set; } = false;

    public ServerDto() { }

    public ServerDto(Server server)
    {
        Id = server.Id;
        Location = server.Location;
        PublicKey = Convert.ToBase64String(server.PublicKey);
        Port = server.Port;
        Address = server.Address;
        Active = server.Active;
        
    }
}