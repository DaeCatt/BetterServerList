using DaeLib.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace BetterServerList.UI {
	class UIPasswordPrompt : UIState {
		private readonly int PADDING = 8;
		internal static string ValueIDPrefix = $"Mods.{nameof(BetterServerList)}.{nameof(UIPasswordPrompt)}.";

		private readonly UIFocusGroup focusGroup = new UIFocusGroup();

		private UILabeledTextInput Password;
		private UILabeledCheckbox RememberPassword;

		// private Texture2D ShowPasswordTexture;

		public override void OnInitialize() {
			// ShowPasswordTexture = TextureManager.Load("Images/UI/InfoIcon_5");

			float height = 0;
			Password = new UILabeledTextInput(Language.GetText(ValueIDPrefix + "PasswordLabel"));
			Password.Input.Sensitive = true;
			Password.Width.Set(0, 1);

			focusGroup.Add(Password.Input);
			height += UILabeledTextInput.DEFAULT_HEIGHT + PADDING;

			RememberPassword = new UILabeledCheckbox(Language.GetText(ValueIDPrefix + "RememberPasswordLabel"), true);
			RememberPassword.Top.Set(height, 0);

			focusGroup.Add(RememberPassword.Checkbox);
			height += UILabeledCheckbox.DEFAULT_HEIGHT + PADDING * 2; // Add the top padding here as well

			UIPanel Panel = new UIPanel();
			Panel.Width.Set(0, 1);
			Panel.Height.Set(height, 0);
			Panel.SetPadding(PADDING);
			Panel.BackgroundColor = new Color(33, 43, 79) * 0.8f;

			Panel.Append(Password);
			Panel.Append(RememberPassword);

			UILargeButton CancelButton = new UILargeButton(Language.GetText("UI.Cancel"), CancelClick);
			CancelButton.Width.Set(-10, 0.5f);
			CancelButton.VAlign = 1;
			CancelButton.Top.Set(-0, 0);

			UILargeButton SubmitButton = new UILargeButton(Language.GetText("UI.Submit"), SubmitClick);
			SubmitButton.CopyStyle(CancelButton);
			SubmitButton.HAlign = 1;

			UIElement Element = new UIElement();
			Element.Width.Set(0, 0.8f);
			Element.MaxWidth.Set(420, 0);
			Element.Top.Set(220, 0);
			Element.Height.Set(height + 65, 0);
			Element.HAlign = 0.5f;

			Element.Append(Panel);
			Element.Append(CancelButton);
			Element.Append(SubmitButton);

			Append(Element);
		}

		public override void OnActivate() {
			Password.Input.Value = "";
			RememberPassword.Checkbox.Checked = true;
		}

		private void SubmitClick(UIMouseEvent evt, UIElement listeningElement) {
			string password = Password.Input.Value;
			if (RememberPassword.Checkbox.Checked) {
				BetterServerList.ActiveServer.Password = password;
				BetterServerList.SaveServers();
			}

			Main.PlaySound(SoundID.MenuTick);
			Netplay.ServerPassword = password;
			NetMessage.SendData(MessageID.SendPassword);
			Main.menuMode = 14;
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Netplay.disconnect = true;
			Netplay.ServerPassword = "";

			Main.PlaySound(SoundID.MenuClose);
			Main.MenuUI.SetState(BetterServerList.ServerListUI);
			Main.menuMode = 888;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			focusGroup.CheckTab();
			base.Draw(spriteBatch);
		}
	}
}
