using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	// Token: 0x02000557 RID: 1367
	public class Prism : Dialog
	{
		// Token: 0x06001FFC RID: 8188 RVA: 0x000D51D4 File Offset: 0x000D33D4
		public Prism(ComponentPlayer player, int id, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			this.Point = null;
			this.Point = Point1;
			this.m_subsystemTerrain = subsystemTerrain;
			this.id1 = id;
			this.player = player;
            LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/PrismDialog"));
			this.m_title = this.Children.Find<LabelWidget>("Prism_Dialog.Title", true);
			this.Mode = this.Children.Find<LabelWidget>("Prism_Dialog.Mode", true);
			this.Icon_select = this.Children.Find<ButtonWidget>("Prism_Dialog.Icon_select", true);
			this.m_radius = this.Children.Find<SliderWidget>("Prism_Dialog.Slider1", true);
			this.plusButton = this.Children.Find<ButtonWidget>("Prism_Dialog.Button4", true);
			this.minusButton = this.Children.Find<ButtonWidget>("Prism_Dialog.Button3", true);
			this.mSelect_mode = this.Children.Find<ButtonWidget>("Prism_Dialog.Select_mode", true);
			this.m_okButton = this.Children.Find<ButtonWidget>("Prism_Dialog.OK", true);
			this.m_cancelButton = this.Children.Find<ButtonWidget>("Prism_Dialog.Cancel", true);
			this.m_blockIconWidget = this.Children.Find<BlockIconWidget>("Prism_Dialog.Icon", true);
			this.m_title.Text = "Prism";
			this.Mode.Text = "Hollow";
			this.m_radius.MinValue = 1f;
			this.m_radius.MaxValue = 100f;
			this.m_radius.Value = 1f;
			this.names.Add("Soild");
			this.names.Add("Hollow");
			foreach (string name in this.names)
			{
				this.m_categories.Add(new Category
				{
					Name = name
				});
			}
		}

		// Token: 0x06001FFD RID: 8189 RVA: 0x000D53E4 File Offset: 0x000D35E4
		public override void Update()
		{
			this.radius = (int)this.m_radius.Value - 1;
			this.m_blockIconWidget.Value = this.id1;
			if (this.plusButton.IsClicked)
			{
				this.m_radius.Value = MathUtils.Min(this.m_radius.Value + 1f, (float)((int)this.m_radius.MaxValue));
			}
			if (this.minusButton.IsClicked)
			{
				this.m_radius.Value = MathUtils.Max(this.m_radius.Value - 1f, (float)((int)this.m_radius.MinValue));
			}
			if (this.mSelect_mode.IsClicked)
			{
				this.Select_mode(this.m_categories, this.names);
			}
			if (this.Icon_select.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
					18,
					92,
					2,
					7,
					3,
					67,
					66,
					4,
					5,
					26,
					73,
					21,
					46,
					47,
					15,
					62,
					68,
					126,
					71,
					1
				}, 72f, delegate(object index)
				{
					ContainerWidget containerWidget = (ContainerWidget) LoadWidget(null, ContentManager.Get<XElement>("Widgets/SelectBlockItem"), null);
					containerWidget.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
					containerWidget.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
					return containerWidget;
				}, delegate(object index)
				{
					this.id1 = (int)index;
				}));
			}
			if (this.m_okButton.IsClicked)
			{
				bool s = this.Mode.Text == "Hollow";
				API_WE.Prism(s, this.radius, this.id1, this.Point, this.m_subsystemTerrain);
				DialogsManager.HideDialog(this);
			}
			if (base.Input.Cancel || this.m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			this.UpdateControls();
		}

		// Token: 0x06001FFE RID: 8190 RVA: 0x000142B0 File Offset: 0x000124B0
		private void UpdateControls()
		{
			this.m_radius.Text = string.Format("{0} blocks", this.radius + 1);
		}

		// Token: 0x06001FFF RID: 8191 RVA: 0x000D5580 File Offset: 0x000D3780
		public void Select_mode(List<Category> m_categories, List<string> a)
		{
			DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, m_categories, 56f, delegate(object c)
			{
				LabelWidget labelWidget = new LabelWidget();
				labelWidget.Text = ((Category)c).Name;
				labelWidget.Color = Color.White;
				int horizontalAlignment = 1;
				labelWidget.HorizontalAlignment = (WidgetAlignment) horizontalAlignment;
				int verticalAlignment = 1;
				labelWidget.VerticalAlignment = (WidgetAlignment) verticalAlignment;
				return labelWidget;
			}, delegate(object c)
			{
				if (c == null)
				{
					return;
				}
				int index = m_categories.IndexOf((Category)c);
				string text = a[index];
				this.Mode.Text = text;
			}));
		}

		// Token: 0x0400181C RID: 6172
		private ComponentPlayer player;

		// Token: 0x0400181D RID: 6173
		private LabelWidget m_title;

		// Token: 0x0400181E RID: 6174
		private SliderWidget m_radius;

		// Token: 0x0400181F RID: 6175
		private ButtonWidget plusButton;

		// Token: 0x04001820 RID: 6176
		private ButtonWidget minusButton;

		// Token: 0x04001821 RID: 6177
		private ButtonWidget m_okButton;

		// Token: 0x04001822 RID: 6178
		private ButtonWidget m_cancelButton;

		// Token: 0x04001823 RID: 6179
		private List<string> names = new List<string>();

		// Token: 0x04001824 RID: 6180
		private List<Category> m_categories = new List<Category>();

		// Token: 0x04001825 RID: 6181
		private ButtonWidget mSelect_mode;

		// Token: 0x04001826 RID: 6182
		private ButtonWidget Icon_select;

		// Token: 0x04001827 RID: 6183
		private LabelWidget Mode;

		// Token: 0x04001828 RID: 6184
		private BlockIconWidget m_blockIconWidget;

		// Token: 0x04001829 RID: 6185
		private int radius;

		// Token: 0x0400182A RID: 6186
		private int id1;

		// Token: 0x0400182B RID: 6187
		private TerrainRaycastResult? Point;

		// Token: 0x0400182C RID: 6188
		private SubsystemTerrain m_subsystemTerrain;
	}
}
