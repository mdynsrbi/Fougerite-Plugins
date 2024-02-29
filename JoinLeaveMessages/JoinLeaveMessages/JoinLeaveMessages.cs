using System;
using System.IO;
using Fougerite;
using Fougerite.Events;

namespace JoinLeaveMessages
{
    public class JoinLeaveMessages : Fougerite.Module
    {
        public override string Name { get { return "JoinLeaveMessages"; } }
        public override string Author { get { return "Yasin"; } }
        public override string Description { get { return "JoinLeaveMessages"; } }
        public override Version Version { get { return new Version("1.0"); } }

        public string sysname = "JoinLeaveMessages";
        public string pnotice = "Welcome to our Server!";
        public string joinmsg = "{player} has joined the server";
        public string leavemsg = "{player} has left the server";

        public IniParser config;
        public DataStore ds = DataStore.GetInstance();

        public override void Initialize()
        {
            Hooks.OnPlayerConnected += OnPlayerConnected;
            Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            Hooks.OnPlayerSpawned += OnPlayerSpawned;
            LoadConfig();
        }

        public override void DeInitialize()
        {
            Hooks.OnPlayerConnected -= OnPlayerConnected;
            Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
            Hooks.OnPlayerSpawned -= OnPlayerSpawned;
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            ds.Add("JoiningServer", player.UID, "Joined");
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (ds.ContainsKey("JoiningServer", player.UID))
                ds.Remove("JoiningServer", player.UID);
            else
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                Server.GetServer().BroadcastFrom(sysname, leavemsg.Replace("{player}", player.Name));
            }
        }

        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            if (ds.ContainsKey("JoiningServer", player.UID))
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                Server.GetServer().BroadcastFrom(sysname, joinmsg.Replace("{player}", player.Name));
                player.Notice(pnotice);
                ds.Remove("JoiningServer", player.UID);
            }
        }

        private void LoadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Config.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Config.ini")).Dispose();
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));

                config.AddSetting("Configuration", "SysName", "JoinLeaveMessages");
                config.AddSetting("Configuration", "WelcomeNotice", "Welcome to our Server!");
                config.AddSetting("Configuration", "JoinMessage", "{player} has joined the server");
                config.AddSetting("Configuration", "LeaveMessage", "{player} has left the server");
                config.Save();
                Logger.Log("JoinLeaveMessages Plugin: New configuration file generated!");
                LoadConfig();
            }
            else
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                try
                {
                    sysname = config.GetSetting("Configuration", "SysName");
                    pnotice = config.GetSetting("Configuration", "WelcomeNotice");
                    joinmsg = config.GetSetting("Configuration", "JoinMessage");
                    leavemsg = config.GetSetting("Configuration", "LeaveMessage");
                    Logger.Log("JoinLeaveMessages Plugin: Configuration file loaded!");
                }
                catch (Exception ex)
                {
                    Logger.LogError("JoinLeaveMessages Plugin: Detected a problem in the configuration");
                    Logger.Log("ERROR -->" + ex.Message);
                    File.Delete(Path.Combine(ModuleFolder, "Config.ini"));
                    Logger.LogError("JoinLeaveMessages Plugin: Deleted the old configuration file");
                    LoadConfig();
                }
            }
        }
    }
}

