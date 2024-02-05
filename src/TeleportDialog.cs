using System;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	public class TeleportDialog : Dialog
	{
		public TeleportDialog(ComponentPlayer player)
		{
			this.player = player;
			LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/TeleportDialog"));
			EntryX = Children.Find<TextBoxWidget>("Dialog.X", true);
			EntryY = Children.Find<TextBoxWidget>("Dialog.Y", true);
			EntryZ = Children.Find<TextBoxWidget>("Dialog.Z", true);
			EntryX.Text = player.ComponentBody.Position.X.ToString();
			EntryY.Text = player.ComponentBody.Position.Y.ToString();
			EntryZ.Text = player.ComponentBody.Position.Z.ToString();
			
			m_okButton = Children.Find<ButtonWidget>("Dialog.OK", true);
			m_cancelButton = Children.Find<ButtonWidget>("Dialog.Cancel", true);
		}

		public override void Update()
		{
			if (m_okButton.IsClicked)
			{
			    float x = float.Parse(EntryX.Text);
                float y = float.Parse(EntryY.Text);
                float z = float.Parse(EntryZ.Text);
                
				player.ComponentBody.Position = new Vector3(x, y, z);
				DialogsManager.HideDialog(this);
			}
			if (m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
		}

		private ComponentPlayer player;

		private TextBoxWidget EntryX;
		private TextBoxWidget EntryY;
		private TextBoxWidget EntryZ;

		private ButtonWidget m_okButton;
		private ButtonWidget m_cancelButton;
	}
}
