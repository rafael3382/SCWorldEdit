using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	public class ActionBindingsDialog : Dialog
	{
		public Dictionary<WEAction, BitmapButtonWidget> CurrentActionBindings = new Dictionary<WEAction, BitmapButtonWidget>();

		public ActionBindingsDialog(WEUser user)
		{
			this.user = user;
			LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/ActionBindingsDialog"));

			var content = Children.Find<StackPanelWidget>("Dialog.Content", true);
			content.LoadChildren(content, ContentManager.Get<XElement>("WE/WEMenu"));
			foreach (WEAction action in user.actions)
			{
				if (action.ButtonWidget != null)
					CurrentActionBindings.Add(action, content.Children.Find<BitmapButtonWidget>(action.ButtonWidget.Name, true));
			}

            m_okButton = Children.Find<ButtonWidget>("Dialog.OK", true);
			m_cancelButton = Children.Find<ButtonWidget>("Dialog.Cancel", true);
		}

		public override void Update()
		{
            foreach (KeyValuePair<WEAction, BitmapButtonWidget> binding in CurrentActionBindings)
			{
				if (binding.Value.IsClicked)
				{
					DialogsManager.ShowDialog(user.m_gameWidget, new ListSelectionDialog("", user.actions, 64f, delegate (object item)
					{
						WEAction action = (WEAction)item;
                        XElement node = ContentManager.Get<XElement>("Widgets/SelectExternalContentTypeItem");
                        ContainerWidget obj = (ContainerWidget)Widget.LoadWidget(null, node, null);
                        obj.Children.Find<RectangleWidget>("SelectExternalContentType.Icon").Subtexture = new Subtexture(action.Icon, Vector2.Zero, Vector2.One);
                        obj.Children.Find<LabelWidget>("SelectExternalContentType.Text").Text = action.Name;
                        return obj;
                    }, delegate (object item) 
					{
						var newAct = (WEAction) item;
                        if (CurrentActionBindings.ContainsKey(newAct))
                        {
                            CurrentActionBindings[binding.Key] = CurrentActionBindings[newAct];
                        }
                        else
                        {
                            CurrentActionBindings.Remove(binding.Key);
                        }
                        CurrentActionBindings[newAct] = binding.Value;
                    }
					));
				}
				else
				{
					binding.Key.ApplyOnButton(binding.Value);
				}
			}
			if (m_okButton.IsClicked)
			{
				foreach (WEAction action in user.actions)
				{
					action.ButtonWidget = null;
				}
				foreach (KeyValuePair<WEAction, BitmapButtonWidget> binding in CurrentActionBindings)
				{
					binding.Key.ButtonWidget = user.WEPlace.Children.Find<BitmapButtonWidget>(binding.Value.Name);
				}
                DialogsManager.HideDialog(this);
            }
			if (m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
		}

		private WEUser user;

		private ButtonWidget m_okButton;
		private ButtonWidget m_cancelButton;
	}
}
