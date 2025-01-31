using IniParser;
using IniParser.Parser;
using Microsoft.EntityFrameworkCore;
using WgNode.Models;

namespace WgNode.Services;

public interface IServerService
{
    Task CreateServer(string host, int port);
    Task LoadServer();
    Task UpdateServer();
    Task LaunchServer();
    Task StopServer();
}

public class ServerService(ILogger<ServerService> logger, MainDbContext context, ICommandExecutor commandExecutor, IServerStorage serverStorage) : IServerService
{

    public const string WgPath = @"/etc/wireguard/wg0.conf";
    
    public async Task CreateServer(string host, int port)
    {
        using var alg = new Curve25519.NetCore.Curve25519();
        var privateKey = alg.CreateRandomPrivateKey();
        var publicKey = alg.GetPublicKey(privateKey);
        var sharedKey = alg.GetSharedSecret(privateKey,publicKey);

        if (sharedKey is null || publicKey is null)
        {
            logger.LogError("Cannot create server. Private and Public key are invalid.");
            throw new ApplicationException("Cannot create server. Private and Public key are invalid.");
        }
        await commandExecutor.ExecuteCommand("touch /etc/wireguard/wg0.conf");
        
        serverStorage.Server = new Server()
        {
            Id = Guid.NewGuid(),
            PrivateKey = privateKey,
            PublicKey = publicKey,
            Address = host,
            Port = port
        };
        
        await Task.WhenAll( AddServerToConf(),AddServerToDb());

        
        
        logger.LogInformation("Created new server.");

    }

    public async Task LoadServer() => serverStorage.Server = 
        await context.Servers.FirstOrDefaultAsync() ?? 
            throw new InvalidOperationException("Server has not been initialized.");

    private async Task AddServerToDb()
    {
        if (await context.Servers.AnyAsync()) throw new InvalidOperationException("Server already exists.");
        await context.AddAsync(serverStorage.Server);
        await context.SaveChangesAsync();
    }

    private async Task AddServerToConf()
    {
        await using var stream = new FileStream(WgPath, Path.Exists(WgPath) ? FileMode.Truncate : FileMode.CreateNew,
            FileAccess.Write);
        await using var sw = new StreamWriter(stream);

        await sw.WriteAsync(serverStorage.Server.ToString());

        foreach (var peer in await context.Peers.Where(p => p.Active).AsNoTracking().ToListAsync())
        {
            await sw.WriteLineAsync(peer.ToString());
            Console.WriteLine(peer.ToString());
        }

    }

    public async Task UpdateServer()
    {
        await AddServerToConf();
        await SyncConf();
    }

    private async Task SyncConf()
    { 
        var code = await commandExecutor.ExecuteCommand("wg syncconf wg0 <(wg-quick strip wg0)");
        if (code != 0)
        {
            logger.LogError("Sync conf failed.");
            throw new ApplicationException("Sync conf failed.");
        }
        logger.LogInformation($"Sync conf from {code}");
    }
    
    public async Task LaunchServer()
    {
        await commandExecutor.ExecuteCommand("ip link add wg0 type wireguard");
        await commandExecutor.ExecuteCommand("wg setconf wg0 /etc/wireguard/wg0.conf");
        await commandExecutor.ExecuteCommand("ip address add 10.0.0.1/24 dev wg0");
        var code = await commandExecutor.ExecuteCommand("ip link set wg0 up");
        logger.LogInformation($"Launched server with code {code}");
        if (code == 0)
        {
            serverStorage.Server.Active = true;
            await UpdateServer();
            await context.SaveChangesAsync();
        }
    }

    public async Task StopServer()
    {
        var code = await commandExecutor.ExecuteCommand("ip link set wg0 down");
        if(code == 0) code = await commandExecutor.ExecuteCommand("ip link delete wg0");
        logger.LogInformation($"Stopped server with code {code}");
        if (code == 0)
        {
            serverStorage.Server.Active = false;
            await context.SaveChangesAsync();
        }
    }
    
}