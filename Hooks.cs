using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using static Mono.Cecil.Cil.OpCodes;

namespace BetterServerList {
	class Hooks {
		private static readonly MethodInfo MainStatusText = typeof(Main).GetMethod("set_" + nameof(Main.statusText));
		private static readonly FieldInfo MainMenuMode = typeof(Main).GetField(nameof(Main.menuMode));

		public static void NetPlay_ClientLoopSetup(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			if (!cursor.TryGotoNext(i => i.MatchCall(MainStatusText)))
				throw new Exception("Could not locate assignment to Main.statusText.");

			if (!cursor.TryGotoPrev(i => i.MatchLdarg(0)))
				throw new Exception("Could not locate call to Language.GetText.");

			cursor.Remove();
			cursor.Remove();

			cursor.EmitDelegate<Func<string>>(() => {
				string name = BetterServerList.ActiveServer?.Name ?? "";
				return name == "" ? "Server" : name;
			});
		}

		public static void Main_DrawMenu(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			cursor.EmitDelegate<Action>(() => {
				Utils.DrawBorderString(Main.spriteBatch, Main.menuMode.ToString(), Vector2.One * 14, Color.White);
			});

			if (!cursor.TryGotoNext(i => i.MatchLdcI4(12) && i.Next.MatchStsfld(MainMenuMode)))
				throw new Exception("Could not locate 'Main.menuMode = 12;' assigment.");

			cursor.Remove();
			cursor.EmitDelegate<Func<int>>(() => {
				Main.menuMultiplayer = true;
				Main.autoJoin = false;
				Main.menuServer = false;
				return 1;
			});

			cursor.Index = 0;

			Action serverListDelegate = () => {
				Main.MenuUI.SetState(BetterServerList.ServerListUI);
				Main.menuMode = 888;
			};

			// Main vanilla multiplayer menu
			HijackNativeMenu(cursor, 12, serverListDelegate);
			// Join via IP menu
			HijackNativeMenu(cursor, 13, serverListDelegate);
			// We've been disconnected
			SpyNativeMenu(cursor, 14, () => {
				if (BetterServerList.JustSentPassword) {
					BetterServerList.PasswordRejected();
					BetterServerList.JustSentPassword = false;
				}
			});
			// Server requires password
			HijackNativeMenu(cursor, 31, () => {
				if (BetterServerList.ActiveServer != null && BetterServerList.ActiveServer.Password != "") {
					Netplay.ServerPassword = BetterServerList.ActiveServer.Password;
					NetMessage.SendData(MessageID.SendPassword);
					Main.menuMode = 14;
					return;
				}

				Main.MenuUI.SetState(BetterServerList.PasswordPromptUI);
				Main.menuMode = 888;
			});
		}
		private static void HijackNativeMenu(ILCursor cursor, int menuMode, Action menuDelegate) {
			cursor.Index = 0;

			if (!cursor.TryGotoNext(i => i.MatchLdsfld(MainMenuMode) && i.Next.MatchLdcI4(menuMode)))
				throw new Exception($"Could not locate 'Main.menuMode == {menuMode}' conditional.");

			cursor.Index += 2;
			object label = cursor.Next.Operand;
			cursor.Index++;

			cursor.EmitDelegate(menuDelegate);
			cursor.Emit(Br, label);
		}

		private static void SpyNativeMenu(ILCursor cursor, int menuMode, Action spyDelegate) {
			cursor.Index = 0;

			if (!cursor.TryGotoNext(i => i.MatchLdsfld(MainMenuMode) && i.Next.MatchLdcI4(menuMode)))
				throw new Exception($"Could not locate 'Main.menuMode == {menuMode}' conditional.");

			cursor.Index += 3;
			cursor.EmitDelegate(spyDelegate);
		}
	}
}
