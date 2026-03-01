using System;
using System.Linq;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace CustomStatus;

public class CustomStatus : BasePlugin
{
    public override string ModuleName => "Custom Status Plugin";
    public override string ModuleVersion => "4.0.0";
    public override string ModuleAuthor => "jvnipers";
    public override string ModuleDescription => "Adds a css_status command with detailed server and player info.";

    public override void Load(bool hotReload)
    {
        Console.WriteLine("[CUSTOMSTATUS] Plugin loaded.");
    }

    [ConsoleCommand("css_status", "Displays server information and a list of connected players.")]
    [RequiresPermissions("@css/generic")]
    public void OnStatusCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/generic"))
        {
            player.PrintToChat("You do not have permission to use this command.");
            return;
        }

        // --- Server Information ---
        var hostname = ConVar.Find("hostname")?.StringValue ?? "N/A";
        var os = RuntimeInformation.OSDescription;
        var publicIp = ConVar.Find("ip")?.StringValue ?? "N/A";
        var port = ConVar.Find("hostport")?.GetPrimitiveValue<int>() ?? 0;
        var map = Server.MapName;
        var uptime = TimeSpan.FromSeconds(Server.CurrentTime);
        var players = Utilities.GetPlayers();
        var humanPlayers = players.Count(p => p.IsValid && !p.IsBot && !p.IsHLTV);
        var maxPlayers = Server.MaxPlayers;

        command.ReplyToCommand($"hostname: {hostname}");
        command.ReplyToCommand($"os      : {os}");
        command.ReplyToCommand($"ip      : {publicIp}:{port} (public)");
        command.ReplyToCommand($"uptime  : {uptime.Days}d {uptime.Hours:D2}h:{uptime.Minutes:D2}m:{uptime.Seconds:D2}s");
        command.ReplyToCommand($"map     : {map}");
        command.ReplyToCommand($"players : {humanPlayers} humans, {players.Count(p => p.IsBot)} bots ({maxPlayers} max)");
        
        // --- Player List Header ---
        command.ReplyToCommand("--------------------------------- Player List ---------------------------------");
        command.ReplyToCommand(String.Format("{0,-5} {1,-25} {2,-20} {3,-18} {4,-5}", "Slot", "Player Name", "SteamID", "IP Address", "Ping"));
        
        // --- Player List Body ---
        int count = 0;
        foreach (var p in players)
        {
            if (p == null || !p.IsValid || p.IsBot || p.IsHLTV)
                continue;

            count++;
            var ipAddress = p.IpAddress?.Split(':')[0] ?? "N/A";

            command.ReplyToCommand(String.Format("{0,-5} {1,-25} {2,-20} {3,-18} {4,-5}",
                p.Slot,
                p.PlayerName,
                p.SteamID,
                ipAddress,
                p.Ping));
        }
        command.ReplyToCommand($"Total players: {count}");
        command.ReplyToCommand("-------------------------------------------------------------------------------");
    }
}