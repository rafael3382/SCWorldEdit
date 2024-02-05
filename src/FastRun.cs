using System;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	// Token: 0x02000551 RID: 1361
	public class FastRun : Dialog
	{
		// Token: 0x06001FE3 RID: 8163 RVA: 0x000D44B8 File Offset: 0x000D26B8
		public FastRun(ComponentPlayer player)
		{
			this.player = player;
			LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/FastRunDialog"));
			this.m_title = this.Children.Find<LabelWidget>("Dialog.Title", true);
			this.m_speed = this.Children.Find<SliderWidget>("Dialog.Slider1", true);
			this.plusButton = this.Children.Find<ButtonWidget>("Dialog.Button4", true);
			this.minusButton = this.Children.Find<ButtonWidget>("Dialog.Button3", true);
			this.m_okButton = this.Children.Find<ButtonWidget>("Dialog.OK", true);
			this.m_cancelButton = this.Children.Find<ButtonWidget>("Dialog.Cancel", true);
			this.m_speed.MinValue = 0f;
			this.m_speed.MaxValue = 100f;
			this.m_speed.Value = (float)WEUserManager.GetUserByComponent(player).speed;
			this.m_title.Text = "FastRun Settings";
		}

		// Token: 0x06001FE4 RID: 8164 RVA: 0x000D45B8 File Offset: 0x000D27B8
		public override void Update()
		{
			if (this.plusButton.IsClicked)
			{
				this.m_speed.Value = MathUtils.Min(this.m_speed.Value + 1f, (float)((int)this.m_speed.MaxValue));
			}
			if (this.minusButton.IsClicked)
			{
				this.m_speed.Value = MathUtils.Max(this.m_speed.Value - 1f, (float)((int)this.m_speed.MinValue));
			}
			this.Speed = (int)this.m_speed.Value;
			if (this.m_okButton.IsClicked)
			{
				WEUserManager.GetUserByComponent(player).speed = this.Speed;
				DialogsManager.HideDialog(this);
			}
			if (this.m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			this.UpdateControls();
		}

		// Token: 0x06001FE5 RID: 8165 RVA: 0x0001423E File Offset: 0x0001243E
		private void UpdateControls()
		{
			this.m_speed.Text = string.Format("{0}", this.Speed);
		}

		// Token: 0x040017E2 RID: 6114
		private ComponentPlayer player;

		// Token: 0x040017E3 RID: 6115
		private LabelWidget m_title;

		// Token: 0x040017E4 RID: 6116
		private SliderWidget m_speed;

		// Token: 0x040017E5 RID: 6117
		private ButtonWidget plusButton;

		// Token: 0x040017E6 RID: 6118
		private ButtonWidget minusButton;

		// Token: 0x040017E7 RID: 6119
		private ButtonWidget m_okButton;

		// Token: 0x040017E8 RID: 6120
		private ButtonWidget m_cancelButton;

		// Token: 0x040017E9 RID: 6121
		private int Speed = 1;
	}
}
