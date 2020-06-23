using DaeLib.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Social;
using Terraria.UI;

namespace BetterServerList.UI {
	class UIServerList : UIState {
		internal static string ValueIDPrefix = $"Mods.{nameof(BetterServerList)}.{nameof(UIServerList)}.";
		internal UIList ServerList;
		public override void OnActivate() {
			BetterServerList.ActiveServer = null;
			BetterServerList.LoadServers();
			UpdateServerList();
		}
		public override void OnInitialize() {
			UIElement Element = new UIElement();
			Element.Width.Set(0, 0.8f);
			Element.MaxWidth.Set(650, 0);
			Element.Top.Set(220, 0);
			Element.Height.Set(-220 - 45, 1);
			Element.HAlign = 0.5f;

			UIPanel Panel = new UIPanel();
			Panel.Width.Set(0, 1);
			Panel.Height.Set(-65 * 2, 1);
			Panel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			Element.Append(Panel);

			ServerList = new UIList();
			ServerList.Width.Set(-25f, 1);
			ServerList.Height.Set(0, 1);
			ServerList.ListPadding = 5;
			Panel.Append(ServerList);

			UIScrollbar Scrollbar = new UIScrollbar();
			Scrollbar.SetView(100, 1000);
			Scrollbar.Height.Set(0, 1);
			Scrollbar.HAlign = 1;
			Panel.Append(Scrollbar);
			ServerList.SetScrollbar(Scrollbar);

			UITextPanel<LocalizedText> PanelLabel = new UITextPanel<LocalizedText>(Language.GetText(ValueIDPrefix + "Label"), 0.8f, large: true);
			PanelLabel.HAlign = 0.5f;
			PanelLabel.Top.Set(-35f, 0f);
			PanelLabel.SetPadding(15f);
			PanelLabel.BackgroundColor = new Color(73, 94, 171);
			Element.Append(PanelLabel);

			bool steamEnabled = SocialAPI.Network != null;

			UILargeButton HostAndPlayButton = new UILargeButton(Language.GetText("LegacyMenu.88"), HostAndPlayClick);
			if (steamEnabled) {
				HostAndPlayButton.Width.Set(-10, 0.5f);
			} else {
				HostAndPlayButton.Width.Set(0, 1);
			}

			HostAndPlayButton.VAlign = 1;
			HostAndPlayButton.Top.Set(-65, 0);
			Element.Append(HostAndPlayButton);

			if (steamEnabled) {
				UILargeButton SteamJoinButton = new UILargeButton(Language.GetText("LegacyMenu.145"), SteamJoinClick);
				SteamJoinButton.CopyStyle(HostAndPlayButton);
				SteamJoinButton.HAlign = 1f;
				Element.Append(SteamJoinButton);
			}

			UILargeButton BackButton = new UILargeButton(Language.GetText("UI.Back"), GoBackClick);
			BackButton.Width.Set(-10, 0.5f);
			BackButton.VAlign = 1;
			BackButton.Top.Set(-0, 0);
			Element.Append(BackButton);

			UILargeButton ConnectButton = new UILargeButton(Language.GetText(ValueIDPrefix + "AddServer"), AddServerClick);
			ConnectButton.CopyStyle(BackButton);
			ConnectButton.HAlign = 1;
			Element.Append(ConnectButton);

			Append(Element);
		}
		private void AddServerClick(UIMouseEvent evt, UIElement listeningElement) {
			BetterServerList.ActiveServer = new SavedServer();
			Main.PlaySound(SoundID.MenuOpen);
			Main.MenuUI.SetState(BetterServerList.ServerEditorUI);
			Main.menuMode = 888;
		}
		private void GoBackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMultiplayer = true;
			Main.autoJoin = false;
			Main.menuServer = false;
			Main.menuMode = 1;
		}
		private void HostAndPlayClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Main.menuMultiplayer = true;
			Main.autoJoin = false;
			Main.menuServer = true;
			Main.menuMode = 6;
		}
		private void SteamJoinClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			SocialAPI.Friends.OpenJoinInterface();
		}
		private void UpdateServerList() {
			ServerList.Clear();
			foreach (SavedServer server in BetterServerList.SavedServers)
				ServerList.Add(new UIServerListItem(server));

			ServerList.UpdateOrder();
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (BetterServerList.SavedServers.Count != ServerList.Count)
				UpdateServerList();

			base.Draw(spriteBatch);
		}
	}
}
