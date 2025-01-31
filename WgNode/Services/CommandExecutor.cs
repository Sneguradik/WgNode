using System.Diagnostics;
using System.Runtime.InteropServices;


namespace WgNode.Services;

public interface ICommandExecutor
{
    Task<int> ExecuteCommand(string command);
}

public class CommandExecutor(ILogger<CommandExecutor> logger) : ICommandExecutor
{
    public async Task<int> ExecuteCommand(string command)
    {
        using var process = new Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) process.StartInfo.FileName = "cmd.exe";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) process.StartInfo.FileName = "bash";
        
        process.StartInfo.Arguments = "-c \" " + command + " \"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.Start();
        await process.WaitForExitAsync();
        
        logger.LogInformation(await process.StandardOutput.ReadToEndAsync());
        logger.LogError(await process.StandardError.ReadToEndAsync());
        return process.ExitCode;
    }
}