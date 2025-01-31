using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WgNode.Models;

public class Peer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [MaxLength(32)]
    public byte[] PublicKey { get; set; } = new byte[32];
    [MaxLength(32)]
    public byte[] PrivateKey { get; set; } = new byte[32];
    public bool Active { get; set; }
    [MaxLength(4)]
    public byte[] IpAddress { get; set; } = new byte[4];
    public DateTime LatestHandShake { get; set; }
    //TODO Remove creation time
    public DateTime CreatedAt { get; set; }
    public ulong BytesReceived { get; set; }
    public ulong BytesSent { get; set; }
    public Guid ServerId { get; set; }

    public override string ToString() =>
        $"[Peer]\nPublicKey = {Convert.ToBase64String(PublicKey)}\nAllowedIPs = {IpAddressToString()}/32";
    

    public string IpAddressToString() => string.Join(".", IpAddress);
}