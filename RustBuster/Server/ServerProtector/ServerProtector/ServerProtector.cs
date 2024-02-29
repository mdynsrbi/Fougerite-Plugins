using System;
using System.IO;
using Fougerite;
using Fougerite.Events;
using RustBuster2016Server;

namespace ServerProtector
{
    public class ServerProtector : Fougerite.Module
    {
        public override string Name { get { return "ServerProtector"; } }
        public override string Author { get { return "Yasin"; } }
        public override string Description { get { return "RustBuster Fix Plugin"; } }
        public override Version Version { get { return new Version("1.0"); } }

        public IniParser config;
        public DataStore ds = DataStore.GetInstance();

        public override void Initialize()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Players.ini")))
                File.Create(Path.Combine(ModuleFolder, "Players.ini")).Dispose();
            API.OnRustBusterLogin += OnUserLoggedIn;
            Hooks.OnPlayerConnected += OnPlayerConnected;
            Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            Hooks.OnPlayerSpawned += OnPlayerSpawned;
        }

        public override void DeInitialize()
        {
            API.OnRustBusterLogin -= OnUserLoggedIn;
            Hooks.OnPlayerConnected -= OnPlayerConnected;
            Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
            Hooks.OnPlayerSpawned -= OnPlayerSpawned;
        }

        public void OnUserLoggedIn(API.RustBusterUserAPI user)
        {
            config = new IniParser(Path.Combine(ModuleFolder, "Players.ini"));
            string plhwid = config.GetSetting("PlayerList", user.SteamID);
            if (plhwid != null && plhwid != user.HardwareID)
            {
                user.Player.MessageFrom("ServerProtector", "You can't join this server using other players steamid!");
                user.Player.Disconnect();
            }
            else if (plhwid == user.HardwareID)
                ds.Add("UseRB", user.SteamID, "True");
            else
            {
                config.AddSetting("PlayerList", user.SteamID, user.HardwareID);
                config.Save();
            }
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            if (player.SteamID == "76561197960266962")
            {
                player.Notice("✘", "Please use unban tool!");
                player.Disconnect();
            }
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (ds.ContainsKey("UseRB", player.SteamID))
                ds.Remove("UseRB", player.SteamID);
        }

        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            if (!ds.ContainsKey("UseRB", player.SteamID))
            {
                player.MessageFrom("ServerProtector", "You can't join this server without RustBuster client!");
                player.Disconnect();
            }
        }
    }
}