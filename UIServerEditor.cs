using DaeLib.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace BetterServerList.UI {
	class UIServerEditor : UIState {
		private readonly float PADDING = 8;
		private readonly float PORT_WIDTH = 80;

		internal static string ValueIDPrefix = $"Mods.{nameof(BetterServerList)}.{nameof(UIServerEditor)}.";

		private readonly UIFocusGroup focusGroup = new UIFocusGroup();

		private UILabeledTextInput Name;
		private UILabeledTextInput Address;
		private UILabeledTextInput Port;
		private UILabeledCheckbox HideAddress;

		private bool AddServer = false;

		public override void OnInitialize() {
			float height = 0;

			Name = new UILabeledTextInput(Language.GetText(ValueIDPrefix + "NameLabel"));
			focusGroup.Add(Name.Input);
			Name.Width.Set(0, 1);
			height += UILabeledTextInput.DEFAULT_HEIGHT + PADDING;

			Address = new UILabeledTextInput(Language.GetText(ValueIDPrefix + "AddressLabel"));
			Address.Top.Set(height, 0);
			Address.Width.Set(0 - PORT_WIDTH - PADDING, 1);
			focusGroup.Add(Address.Input);

			Port = new UILabeledTextInput(Language.GetText(ValueIDPrefix + "PortLabel"));
			Port.Top.Set(height, 0);
			Port.Width.Set(PORT_WIDTH, 0);
			Port.HAlign = 1;
			Port.Input.Filter = (value) => {
				string cleaned = Regex.Replace(value, "[^0-9]", "");

				try {
					return Math.Max(0, Math.Min(65535, int.Parse(cleaned))).ToString();
				} catch { }

				return "";
			};

			focusGroup.Add(Port.Input);
			height += UILabeledTextInput.DEFAULT_HEIGHT + PADDING;

			HideAddress = new UILabeledCheckbox(Language.GetText(ValueIDPrefix + "HideAddressLabel"), true);
			HideAddress.Top.Set(height, 0);
			focusGroup.Add(HideAddress.Checkbox);
			height += UILabeledCheckbox.DEFAULT_HEIGHT + PADDING * 2; // Add the top padding here as well

			UIPanel Panel = new UIPanel();
			Panel.Width.Set(0, 1);
			Panel.Height.Set(height, 0);
			Panel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			Panel.SetPadding(PADDING);
			Panel.Append(Name);
			Panel.Append(Port);
			Panel.Append(Address);
			Panel.Append(HideAddress);

			UIElement Element = new UIElement();
			Element.Width.Set(0, 0.8f);
			Element.MaxWidth.Set(420, 0);
			Element.Top.Set(220, 0);
			Element.Height.Set(height + 65 * 2, 0); // 65 * 2 = 2 Buttons high
			Element.HAlign = 0.5f;
			Element.Append(Panel);

			UILargeButton BackButton = new UILargeButton(Language.GetText("UI.Back"), GoBackClick);
			BackButton.Top.Set(-0, 0);
			BackButton.Width.Set(-10, 0.5f);
			BackButton.VAlign = 1;
			Element.Append(BackButton);

			UILargeButton SaveButton = new UILargeButton(Language.GetText(ValueIDPrefix + "Save"), SaveServerClick);
			SaveButton.CopyStyle(BackButton);
			SaveButton.HAlign = 1;
			Element.Append(SaveButton);

			UILargeButton SaveAndConnectButton = new UILargeButton(Language.GetText(ValueIDPrefix + "SaveAndConnect"), SaveAndConnectClick);
			SaveAndConnectButton.Top.Set(-65, 0);
			SaveAndConnectButton.VAlign = 1;
			Element.Append(SaveAndConnectButton);

			Append(Element);

			// BackButton.SetSnapPoint("Back", 0);
			// SaveButton.SetSnapPoint("Save", 0);
			// SaveAndConnectButton.SetSnapPoint("Save&Connect", 0);
		}

		public override void OnActivate() {
			Name.Input.Value = BetterServerList.ActiveServer.Name;
			Address.Input.Value = BetterServerList.ActiveServer.Address;
			Port.Input.Value = BetterServerList.ActiveServer.Port.ToString();
			HideAddress.Checkbox.Checked = BetterServerList.ActiveServer.HideAddress;
		}

		private void SaveServer() {
			BetterServerList.ActiveServer.Name = Name.Input.Value;
			BetterServerList.ActiveServer.Address = Address.Input.Value;
			try {
				BetterServerList.ActiveServer.Port = int.Parse(Port.Input.Value);
			} catch { }
			BetterServerList.ActiveServer.HideAddress = HideAddress.Checkbox.Checked;

			if (AddServer)
				BetterServerList.SavedServers.Add(BetterServerList.ActiveServer);

			BetterServerList.SaveServers();
		}

		private void SaveAndConnectClick(UIMouseEvent evt, UIElement listeningElement) {
			SaveServer();
			Main.autoPass = false;
			Netplay.ListenPort = BetterServerList.ActiveServer.Port;
			Main.getIP = BetterServerList.ActiveServer.Address;
			if (Netplay.SetRemoteIP(Main.getIP)) {
				Main.menuMode = 10;
				Netplay.StartTcpClient();
			}
		}

		private void SaveServerClick(UIMouseEvent evt, UIElement listeningElement) {
			SaveServer();
			GoBackClick(evt, listeningElement);
		}

		private void GoBackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.MenuUI.SetState(BetterServerList.ServerListUI);
			Main.menuMode = 888;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			focusGroup.CheckTab();
			Address.Input.Sensitive = Port.Input.Sensitive = HideAddress.Checkbox.Checked;
			base.Draw(spriteBatch);
		}
	}
}
