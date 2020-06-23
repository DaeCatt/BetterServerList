using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace BetterServerList.UI {
	class UIServerListItem : UIPanel {
		internal static string ValueIDPrefix = $"Mods.{nameof(BetterServerList)}.{nameof(UIServerListItem)}.";

		private readonly UIText primaryButtonLabel;
		private readonly UIText secondaryButtonLabel;

		private readonly Texture2D _buttonFavoriteActiveTexture;
		private readonly Texture2D _buttonFavoriteInactiveTexture;
		private readonly Texture2D _buttonPlayTexture;
		private readonly Texture2D _buttonDeleteTexture;
		private readonly UIImageButton _deleteButton;

		private readonly SavedServer Server;

		public UIServerListItem(SavedServer server) {
			BorderColor = new Color(89, 116, 213) * 0.7f;
			_buttonFavoriteActiveTexture = TextureManager.Load("Images/UI/ButtonFavoriteActive");
			_buttonFavoriteInactiveTexture = TextureManager.Load("Images/UI/ButtonFavoriteInactive");
			_buttonPlayTexture = TextureManager.Load("Images/UI/ButtonPlay");
			_buttonDeleteTexture = TextureManager.Load("Images/UI/ButtonDelete");
			Height.Set(64, 0f);
			Width.Set(0f, 1f);
			SetPadding(6f);
			OnDoubleClick += JoinServer;
			Server = server;

			UIImageButton JoinButton = new UIImageButton(_buttonPlayTexture);
			JoinButton.VAlign = 1f;
			JoinButton.Left.Set(4f, 0f);
			JoinButton.OnClick += JoinServer;
			JoinButton.OnMouseOver += JoinMouseOver;
			JoinButton.OnMouseOut += ButtonMouseOut;
			Append(JoinButton);

			UIImageButton FavoriteButton = new UIImageButton(Server.IsFavorite ? _buttonFavoriteActiveTexture : _buttonFavoriteInactiveTexture);
			FavoriteButton.VAlign = 1f;
			FavoriteButton.Left.Set(28f, 0f);
			FavoriteButton.OnClick += FavoriteButtonClick;
			FavoriteButton.OnMouseOver += FavoriteMouseOver;
			FavoriteButton.OnMouseOut += ButtonMouseOut;
			FavoriteButton.SetVisibility(1f, Server.IsFavorite ? 0.8f : 0.4f);
			Append(FavoriteButton);

			UIImageButton ConfigButton = new UIImageButton(BetterServerList.ConfigButtonTexture);
			ConfigButton.VAlign = 1f;
			ConfigButton.Left.Set(28f + 24, 0f);
			ConfigButton.OnClick += ConfigButtonClick;
			ConfigButton.OnMouseOver += ConfigMouseOver;
			ConfigButton.OnMouseOut += ButtonMouseOut;
			Append(ConfigButton);

			UIImageButton ForgetButton = new UIImageButton(_buttonDeleteTexture);
			ForgetButton.VAlign = 1f;
			ForgetButton.HAlign = 1f;
			ForgetButton.OnClick += DeleteButtonClick;
			ForgetButton.OnMouseOver += DeleteMouseOver;
			ForgetButton.OnMouseOut += DeleteMouseOut;
			_deleteButton = ForgetButton;
			if (!Server.IsFavorite)
				Append(ForgetButton);

			int buttonLabelLeft = 80;
			primaryButtonLabel = new UIText("");
			primaryButtonLabel.VAlign = 1f;
			primaryButtonLabel.Left.Set(buttonLabelLeft, 0f);
			primaryButtonLabel.Top.Set(-3f, 0f);
			Append(primaryButtonLabel);

			secondaryButtonLabel = new UIText("");
			secondaryButtonLabel.VAlign = 1f;
			secondaryButtonLabel.HAlign = 1f;
			secondaryButtonLabel.Left.Set(-30f, 0f);
			secondaryButtonLabel.Top.Set(-3f, 0f);
			Append(secondaryButtonLabel);
		}

		private void JoinMouseOver(UIMouseEvent evt, UIElement listeningElement) {
			primaryButtonLabel.SetText(Language.GetTextValue(ValueIDPrefix + "Connect").Trim());
		}

		private void ConfigMouseOver(UIMouseEvent evt, UIElement listeningElement) {
			primaryButtonLabel.SetText(Language.GetTextValue(ValueIDPrefix + "Edit").Trim());
		}
		private void FavoriteMouseOver(UIMouseEvent evt, UIElement listeningElement) {
			if (Server.IsFavorite) {
				primaryButtonLabel.SetText(Language.GetTextValue("UI.Unfavorite"));
			} else {
				primaryButtonLabel.SetText(Language.GetTextValue("UI.Favorite"));
			}
		}

		private void DeleteMouseOver(UIMouseEvent evt, UIElement listeningElement) {
			secondaryButtonLabel.SetText(Language.GetTextValue(ValueIDPrefix + "Remove").Trim());
		}

		private void DeleteMouseOut(UIMouseEvent evt, UIElement listeningElement) {
			secondaryButtonLabel.SetText("");
		}

		private void ButtonMouseOut(UIMouseEvent evt, UIElement listeningElement) {
			primaryButtonLabel.SetText("");
		}

		private void JoinServer(UIMouseEvent evt, UIElement listeningElement) {
			BetterServerList.ActiveServer = Server;

			Main.autoPass = false;
			Netplay.ListenPort = Server.Port;
			Main.getIP = Server.Address;
			if (Netplay.SetRemoteIP(Main.getIP)) {
				Main.menuMode = 10;
				Netplay.StartTcpClient();
			}
		}

		private void ConfigButtonClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			BetterServerList.ActiveServer = Server;
			Main.MenuUI.SetState(BetterServerList.ServerEditorUI);
			Main.menuMode = 888;
		}

		private void FavoriteButtonClick(UIMouseEvent evt, UIElement listeningElement) {
			Server.IsFavorite = !Server.IsFavorite;

			((UIImageButton) evt.Target).SetImage(Server.IsFavorite ? _buttonFavoriteActiveTexture : _buttonFavoriteInactiveTexture);
			((UIImageButton) evt.Target).SetVisibility(1f, Server.IsFavorite ? 0.8f : 0.4f);

			if (Server.IsFavorite) {
				primaryButtonLabel.SetText(Language.GetTextValue("UI.Unfavorite"));
				RemoveChild(_deleteButton);
			} else {
				primaryButtonLabel.SetText(Language.GetTextValue("UI.Favorite"));
				Append(_deleteButton);
			}

			(Parent.Parent as UIList)?.UpdateOrder();
			BetterServerList.SaveServers();
		}
		private void DeleteButtonClick(UIMouseEvent evt, UIElement listeningElement) {
			BetterServerList.SavedServers.Remove(Server);
			(Parent.Parent as UIList).RemoveChild(this);
			BetterServerList.SaveServers();
		}

		public override int CompareTo(object obj) {
			UIServerListItem b = obj as UIServerListItem;
			if (b == null)
				return base.CompareTo(obj);

			if (Server.IsFavorite != b.Server.IsFavorite)
				return Server.IsFavorite ? -1 : 1;

			return Server.Name.CompareTo(b.Server.Name);
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			BackgroundColor = new Color(73, 94, 171);
			BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			BackgroundColor = new Color(63, 82, 151) * 0.7f;
			BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();

			string name = Server.Name;
			name += " (" + (Server.HideAddress ? Language.GetTextValue(ValueIDPrefix + "AddressHidden").Trim() : Server.Address + (Server.Port != 7777 ? ":" + Server.Port.ToString() : "")) + ")";

			Utils.DrawBorderString(spriteBatch, name, new Vector2(innerDimensions.X + 6f, innerDimensions.Y + 2f), Color.White);
		}
	}
}
