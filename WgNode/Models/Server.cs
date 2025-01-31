using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WgNode.Models;

public class Server
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public string Location { get; set; } = string.Empty;
    [MaxLength(32)]
    public byte[] PublicKey { get; set; } = new byte[32];
    [MaxLength(32)]
    public byte[] PrivateKey { get; set; } = new byte[32];
    public int Port { get; set; }
    
    public string Address { get; set; } = string.Empty;
    
    public bool Active { get; set; } = false;
    
    public override string ToString()
    {
        return "[Interface]\n" +
               $"PrivateKey = {Convert.ToBase64String(PrivateKey)}\n" +
               "Address = 10.20.0.1/24\n" +
               $"ListenPort = {Port}\n" +
               "MTU=1500\n" +
               "PostUp =  iptables -t nat -A POSTROUTING -s 10.20.0.0/24 -o eth0 -j MASQUERADE; iptables -A INPUT -p udp -m udp --dport 34567 -j ACCEPT; iptables -A FORWARD -i wg0 -j ACCEPT; iptables -A FORWARD -o wg0 -j ACCEPT;\n" +
               "PostDown =  iptables -t nat -D POSTROUTING -s 10.20.0.0/24 -o eth0 -j MASQUERADE; iptables -D INPUT -p udp -m udp --dport 34567 -j ACCEPT; iptables -D FORWARD -i wg0 -j ACCEPT; iptables -D FORWARD -o wg0 -j ACCEPT;\n";
    }
}