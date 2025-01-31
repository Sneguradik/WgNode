using Microsoft.EntityFrameworkCore;
using WgNode.Models;

namespace WgNode.Services;

public class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options)
{
    public DbSet<Peer> Peers { get; set; }
    public DbSet<Server> Servers { get; set; }
}