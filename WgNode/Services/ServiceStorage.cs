using WgNode.Models;

namespace WgNode.Services;

public interface IServerStorage
{
    Server Server { get; set; }
}

public class ServerStorage : IServerStorage
{
    private Server? _instance;

    public Server Server
    {
        get => _instance ?? throw new Exception("Server not initialized");
        set => _instance ??= value;
    }
}