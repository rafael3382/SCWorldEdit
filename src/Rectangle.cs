using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	// Token: 0x02000554 RID: 1364
	public class Rectangle : Dialog
	{
		// Token: 0x06001FF1 RID: 8177 RVA: 0x000D4C94 File Offset: 0x000D2E94
		public Rectangle(ComponentPlayer player, int id, TerrainRaycastResult? Point1, TerrainRaycastResult? Point2, SubsystemTerrain subsystemTerrain)
		{
			this.m_subsystemTerrain = subsystemTerrain;
			this.Point = Point1;
			this.Point2 = Point2;
			this.id1 = id;
			this.player = player;
            
            LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/PillarDialog"));
			this.m_title = this.Children.Find<LabelWidget>("Pillar_Dialog.Title", true);
			this.mPosition = this.Children.Find<LabelWidget>("Pillar_Dialog.Pos", true);
			this.Icon_select = this.Children.Find<ButtonWidget>("Pillar_Dialog.Icon_select", true);
			this.mselect_pos = this.Children.Find<ButtonWidget>("Pillar_Dialog.Select_pos", true);
			this.m_okButton = this.Children.Find<ButtonWidget>("Pillar_Dialog.OK", true);
			this.m_cancelButton = this.Children.Find<ButtonWidget>("Pillar_Dialog.Cancel", true);
			this.m_blockIconWidget = this.Children.Find<BlockIconWidget>("Pillar_Dialog.Icon", true);
			this.m_title.Text = "Frame or box";
			this.mPosition.Text = "Frame";
			this.m_blockIconWidget.Value = id;
			this.names.Add("Soild");
			this.names.Add("Hollow");
			this.names_pos.Add("Frame");
			this.names_pos.Add("Hollow Box");
			this.names_pos.Add("Soild Box");
			foreach (string name in this.names)
			{
				this.m_categories.Add(new Category
				{
					Name = name
				});
			}
			foreach (string name2 in this.names_pos)
			{
				this.m_categories_pos.Add(new Category
				{
					Name = name2
				});
			}
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x000D4ED0 File Offset: 0x000D30D0
		public override void Update()
		{
			this.m_blockIconWidget.Value = this.id1;
			if (this.mselect_pos.IsClicked)
			{
				this.Select_pos(this.m_categories_pos, this.names_pos);
			}
			if (this.Icon_select.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
				    0,
					18,
					92,
					8,
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
				if (this.mPosition.Text == "Frame")
				{
					this.pos1 = 2;
				}
				if (this.mPosition.Text == "Hollow Box")
				{
					this.pos1 = 1;
				}
				if (this.mPosition.Text == "Soild Box")
				{
					this.pos1 = 0;
				}
				Point3 start;
				start.X = this.Point.Value.CellFace.X;
				start.Y = this.Point.Value.CellFace.Y;
				start.Z = this.Point.Value.CellFace.Z;
				Point3 end;
				end.X = this.Point2.Value.CellFace.X;
				end.Y = this.Point2.Value.CellFace.Y;
				end.Z = this.Point2.Value.CellFace.Z;
				API_WE.Rectangle(this.pos1, this.id1, start, end, this.player, this.m_subsystemTerrain);
				DialogsManager.HideDialog(this);
			}
			if (base.Input.Cancel || this.m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			this.UpdateControls();
		}

		// Token: 0x06001FF3 RID: 8179 RVA: 0x000036B1 File Offset: 0x000018B1
		private void UpdateControls()
		{
		}

		// Token: 0x06001FF4 RID: 8180 RVA: 0x000D50CC File Offset: 0x000D32CC
		public void Select_pos(List<Category> m_categories, List<string> a)
		{
			if (WEUserManager.GetUserByComponent(player).m_componentPlayer != null)
			{
				DialogsManager.ShowDialog(WEUserManager.GetUserByComponent(player).m_componentPlayer.GameWidget, new ListSelectionDialog(string.Empty, m_categories, 56f, delegate(object c)
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
					this.mPosition.Text = text;
				}));
			}
		}

		// Token: 0x04001805 RID: 6149
		private ComponentPlayer player;

		// Token: 0x04001806 RID: 6150
		private LabelWidget m_title;

		// Token: 0x04001807 RID: 6151
		private ButtonWidget m_okButton;

		// Token: 0x04001808 RID: 6152
		private ButtonWidget m_cancelButton;

		// Token: 0x04001809 RID: 6153
		private List<string> names = new List<string>();

		// Token: 0x0400180A RID: 6154
		private List<Category> m_categories = new List<Category>();

		// Token: 0x0400180B RID: 6155
		private List<string> names_pos = new List<string>();

		// Token: 0x0400180C RID: 6156
		private List<Category> m_categories_pos = new List<Category>();

		// Token: 0x0400180D RID: 6157
		private ButtonWidget mselect_pos;

		// Token: 0x0400180E RID: 6158
		private ButtonWidget Icon_select;

		// Token: 0x0400180F RID: 6159
		private LabelWidget mPosition;

		// Token: 0x04001810 RID: 6160
		private BlockIconWidget m_blockIconWidget;

		// Token: 0x04001811 RID: 6161
		private int pos1;

		// Token: 0x04001812 RID: 6162
		private int id1;

		// Token: 0x04001813 RID: 6163
		private TerrainRaycastResult? Point;

		// Token: 0x04001814 RID: 6164
		private TerrainRaycastResult? Point2;

		// Token: 0x04001815 RID: 6165
		private SubsystemTerrain m_subsystemTerrain;
	}
}
