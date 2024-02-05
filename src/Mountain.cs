using System;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	// Token: 0x02000552 RID: 1362
	public class Mountain : Dialog
	{
		// Token: 0x06001FE6 RID: 8166 RVA: 0x000D4684 File Offset: 0x000D2884
		public Mountain(ComponentPlayer player, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			this.m_subsystemTerrain = subsystemTerrain;
			this.Point = Point1;
			this.id1 = 3;
			this.id2 = 2;
			this.id3 = 8;
			this.player = player;
            LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/MountainDialog"));
			this.m_title = this.Children.Find<LabelWidget>("Mountain_Dialog.Title", true);
			this.Icon_select = this.Children.Find<ButtonWidget>("Mountain_Dialog.Icon_select", true);
			this.Icon_select1 = this.Children.Find<ButtonWidget>("Mountain_Dialog.Icon_select1", true);
			this.Icon_select2 = this.Children.Find<ButtonWidget>("Mountain_Dialog.Icon_select2", true);
			this.m_radius = this.Children.Find<SliderWidget>("Mountain_Dialog.Slider1", true);
			this.m_lenght = this.Children.Find<SliderWidget>("Mountain_Dialog.Slider2", true);
			this.plusButton = this.Children.Find<ButtonWidget>("Mountain_Dialog.Button4", true);
			this.minusButton = this.Children.Find<ButtonWidget>("Mountain_Dialog.Button3", true);
			this.lenght_plusButton = this.Children.Find<ButtonWidget>("Mountain_Dialog.Button2", true);
			this.lenght_minusButton = this.Children.Find<ButtonWidget>("Mountain_Dialog.Button1", true);
			this.m_okButton = this.Children.Find<ButtonWidget>("Mountain_Dialog.OK", true);
			this.m_cancelButton = this.Children.Find<ButtonWidget>("Mountain_Dialog.Cancel", true);
			this.m_blockIconWidget = this.Children.Find<BlockIconWidget>("Mountain_Dialog.Icon", true);
			this.m_blockIconWidget1 = this.Children.Find<BlockIconWidget>("Mountain_Dialog.Icon1", true);
			this.m_blockIconWidget2 = this.Children.Find<BlockIconWidget>("Mountain_Dialog.Icon2", true);
			this.m_title.Text = "Mountain";
			this.m_radius.MinValue = 1f;
			this.m_radius.MaxValue = 100f;
			this.m_radius.Value = 1f;
			this.m_lenght.MinValue = 1f;
			this.m_lenght.MaxValue = 100f;
			this.m_lenght.Value = 1f;
		}

		// Token: 0x06001FE7 RID: 8167 RVA: 0x000D489C File Offset: 0x000D2A9C
		public override void Update()
		{
			this.radius = (int)this.m_radius.Value - 1;
			this.lenght = (int)this.m_lenght.Value - 1;
			this.m_blockIconWidget.Value = this.id1;
			this.m_blockIconWidget1.Value = this.id2;
			this.m_blockIconWidget2.Value = this.id3;
			if (this.plusButton.IsClicked)
			{
				this.m_radius.Value = MathUtils.Min(this.m_radius.Value + 1f, (float)((int)this.m_radius.MaxValue));
			}
			if (this.minusButton.IsClicked)
			{
				this.m_radius.Value = MathUtils.Max(this.m_radius.Value - 1f, (float)((int)this.m_radius.MinValue));
			}
			if (this.lenght_plusButton.IsClicked)
			{
				this.m_lenght.Value = MathUtils.Min(this.m_lenght.Value + 1f, (float)((int)this.m_lenght.MaxValue));
			}
			if (this.lenght_minusButton.IsClicked)
			{
				this.m_lenght.Value = MathUtils.Max(this.m_lenght.Value - 1f, (float)((int)this.m_lenght.MinValue));
			}
			if (this.Icon_select.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
					8,
					2,
					7
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
			if (this.Icon_select1.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
					8,
					2,
					7
				}, 72f, delegate(object index)
				{
					ContainerWidget containerWidget = (ContainerWidget) LoadWidget(null, ContentManager.Get<XElement>("Widgets/SelectBlockItem"), null);
					containerWidget.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
					containerWidget.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
					return containerWidget;
				}, delegate(object index)
				{
					this.id2 = (int)index;
				}));
			}
			if (this.Icon_select2.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
					8,
					2,
					7
				}, 72f, delegate(object index)
				{
					ContainerWidget containerWidget = (ContainerWidget) LoadWidget(null, ContentManager.Get<XElement>("Widgets/SelectBlockItem"), null);
					containerWidget.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
					containerWidget.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
					return containerWidget;
				}, delegate(object index)
				{
					this.id3 = (int)index;
				}));
			}
			if (this.m_okButton.IsClicked)
			{
				Point3 start;
				start.X = this.Point.Value.CellFace.X;
				start.Y = this.Point.Value.CellFace.Y;
				start.Z = this.Point.Value.CellFace.Z;
				API_WE.Mountain(start, this.radius, this.lenght, this.m_subsystemTerrain, this.id1, this.id2, this.id3, this.player);
				DialogsManager.HideDialog(this);
			}
			if (base.Input.Cancel || this.m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			this.UpdateControls();
		}

		// Token: 0x06001FE8 RID: 8168 RVA: 0x000D4BCC File Offset: 0x000D2DCC
		private void UpdateControls()
		{
			this.m_radius.Text = string.Format("{0} blocks", this.radius + 1);
			this.m_lenght.Text = string.Format("{0} blocks", this.lenght + 1);
		}

		// Token: 0x040017EA RID: 6122
		private ComponentPlayer player;

		// Token: 0x040017EB RID: 6123
		private LabelWidget m_title;

		// Token: 0x040017EC RID: 6124
		private SliderWidget m_radius;

		// Token: 0x040017ED RID: 6125
		private SliderWidget m_lenght;

		// Token: 0x040017EE RID: 6126
		private ButtonWidget plusButton;

		// Token: 0x040017EF RID: 6127
		private ButtonWidget minusButton;

		// Token: 0x040017F0 RID: 6128
		private ButtonWidget lenght_plusButton;

		// Token: 0x040017F1 RID: 6129
		private ButtonWidget lenght_minusButton;

		// Token: 0x040017F2 RID: 6130
		private ButtonWidget m_okButton;

		// Token: 0x040017F3 RID: 6131
		private ButtonWidget m_cancelButton;

		// Token: 0x040017F4 RID: 6132
		private ButtonWidget Icon_select;

		// Token: 0x040017F5 RID: 6133
		private ButtonWidget Icon_select1;

		// Token: 0x040017F6 RID: 6134
		private ButtonWidget Icon_select2;

		// Token: 0x040017F7 RID: 6135
		private BlockIconWidget m_blockIconWidget;

		// Token: 0x040017F8 RID: 6136
		private BlockIconWidget m_blockIconWidget1;

		// Token: 0x040017F9 RID: 6137
		private BlockIconWidget m_blockIconWidget2;

		// Token: 0x040017FA RID: 6138
		private int radius;

		// Token: 0x040017FB RID: 6139
		private int lenght;

		// Token: 0x040017FC RID: 6140
		private int id1;

		// Token: 0x040017FD RID: 6141
		private int id2;

		// Token: 0x040017FE RID: 6142
		private int id3;

		// Token: 0x040017FF RID: 6143
		private TerrainRaycastResult? Point;

		// Token: 0x04001800 RID: 6144
		private SubsystemTerrain m_subsystemTerrain;
	}
}
