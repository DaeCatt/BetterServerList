using BetterServerList.UI;
using DaeLib.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BetterServerList {
	class SavedServer {
		public string Name = "Unnamed Server";
		public string Address = "localhost";
		public int Port = 7777;
		public string Password = "";
		public bool HideAddress = true;
		public bool IsFavorite = false;

		public SavedServer(string name = "Unnamed Server", string address = "localhost", int port = 7777, string password = "", bool hideAddress = true, bool isFavorite = false) {
			Name = name;
			Address = address;
			Port = port;
			Password = password;
			HideAddress = hideAddress;
			IsFavorite = isFavorite;
		}
	}

	class BetterServerList : Mod {
		public static readonly string ServersSavePath = Main.SavePath + Path.DirectorySeparatorChar + "daes-servers.json";
		public static readonly string VanillaSavePath = Main.SavePath + Path.DirectorySeparatorChar + "servers.dat";

		static public List<SavedServer> SavedServers = new List<SavedServer>();
		public static UIServerList ServerListUI;
		public static UIServerEditor ServerEditorUI;
		public static UIPasswordPrompt PasswordPromptUI;

		public static SavedServer ActiveServer;

		public static ScalableTexture2D PanelTexture;
		public static Texture2D ConfigButtonTexture;

		public static bool JustSentPassword = false;

		public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7) {
			JustSentPassword = msgType == 38;
			return false;
		}

		public static void PasswordRejected() {
			if (ActiveServer != null) {
				ActiveServer.Password = "";
				SaveServers();
			}
		}

		public override void Load() {
			if (Main.dedServ)
				return;

			// Add hooks
			IL.Terraria.Main.DrawMenu += Hooks.Main_DrawMenu;
			IL.Terraria.Netplay.ClientLoopSetup += Hooks.NetPlay_ClientLoopSetup;
			// TODO: IL Hook into NetPlay.ClientLoopSetup

			// Create our serverlist state
			ServerListUI = new UIServerList();
			ServerEditorUI = new UIServerEditor();
			PasswordPromptUI = new UIPasswordPrompt();

			// Load textures for draw helper
			PanelTexture = new ScalableTexture2D(GetTexture("RichPanel"), 6);
			ConfigButtonTexture = GetTexture("ButtonConfig");
		}

		public static void LoadServers() {
			SavedServers.Clear();

			try {
				if (File.Exists(ServersSavePath)) {
					string jsonString = File.ReadAllText(ServersSavePath);
					SavedServers.AddRange(JsonConvert.DeserializeObject<List<SavedServer>>(jsonString));
				} else if (File.Exists(VanillaSavePath)) {
					using (FileStream input = new FileStream(VanillaSavePath, FileMode.Open)) {
						using (BinaryReader binaryReader = new BinaryReader(input)) {
							binaryReader.ReadInt32(); // TODO: Vanilla doesn't verify the number, but it should be 194
							for (int i = 0; i < 10; i++) {
								string name = binaryReader.ReadString();
								string address = binaryReader.ReadString();
								int port = binaryReader.ReadInt32();
								if (address != "" && port != 0) {
									SavedServers.Add(new SavedServer(name, address, port));
								}
							}
						}
					}
				}
			} catch (Exception e) {
				System.Diagnostics.Debug.WriteLine($"Error reading saved servers: {e}");
			}
		}

		public static void SaveServers() {
			Directory.CreateDirectory(Main.SavePath);

			if (File.Exists(ServersSavePath))
				File.SetAttributes(ServersSavePath, FileAttributes.Normal);

			File.WriteAllText(ServersSavePath, JsonConvert.SerializeObject(SavedServers, Formatting.Indented));
		}
	}
}
