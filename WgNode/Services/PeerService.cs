using System.Security.Cryptography;
using WgNode.Models;

namespace WgNode.Services;

public interface IPeerService
{
    Peer CreatePeer(string name, byte[] address);
    byte[] CalculateNextPeerAddress(byte[] peerAddress, int increment = 1);
    Task AddPeerToDb(Peer peer);
    Task AddPeerToDb(IEnumerable<Peer> peers);
    Task RemovePeerFromDb(Peer peer);
    Task RemovePeerFromDb(IEnumerable<Peer> peers);
}

public class PeerService(MainDbContext context, ILogger<PeerService> logger, IServerStorage serverStorage) : IPeerService
{
    public Peer CreatePeer(string name, byte[] address)
    {
        using var alg = new Curve25519.NetCore.Curve25519();
        var privateKey = alg.CreateRandomPrivateKey();
        var publicKey = alg.GetPublicKey(privateKey);

        if (privateKey is null || publicKey is null)
        {
            logger.LogError("Cannot create peer. Private and Public key are invalid.");
            throw new ApplicationException("Cannot create peer. Private and Public key are invalid.");
        }
        
        return new Peer()
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            PublicKey = publicKey,
            PrivateKey = privateKey,
            IpAddress = address,
            ServerId = serverStorage.Server.Id
        };
    }
    
    public byte[] CalculateNextPeerAddress(byte[] peerAddress, int increment = 1)
    {
        
        if (peerAddress.Length != 4) throw new ArgumentException("Invalid length of peerAddress");
        if (peerAddress == (byte[])[255,255,255,255]) throw new ArgumentException("Max Peer Reached");
        int carry = increment;
        for (int i = peerAddress.Length - 1; i >= 0 && carry > 0; i--)
        {
            int sum = peerAddress[i] + carry;
            peerAddress[i] = (byte)(sum % 256);
            carry = sum / 256;
        }
        return (byte[])peerAddress.Clone();
    }

    public async Task AddPeerToDb(Peer peer)
    {
        logger.LogInformation("Adding peer to database.");
        await context.Peers.AddAsync(peer);
        await context.SaveChangesAsync();
    }

    public async Task AddPeerToDb(IEnumerable<Peer> peers)
    {
        logger.LogInformation("Adding peers to database.");
        await context.Peers.AddRangeAsync(peers);
        await context.SaveChangesAsync();
    }

    public async Task RemovePeerFromDb(Peer peer)
    {
        logger.LogInformation("Removing peer from database.");
        context.Peers.Remove(peer);
        await context.SaveChangesAsync();
    }

    public async Task RemovePeerFromDb(IEnumerable<Peer> peers)
    {
        logger.LogInformation("Removing peers from database.");
        context.Peers.RemoveRange(peers);
        await context.SaveChangesAsync();
    }
    
}