using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	// Token: 0x0200055A RID: 1370
	public class Round : Dialog
	{
		// Token: 0x06002007 RID: 8199 RVA: 0x000D5648 File Offset: 0x000D3848
		public Round(ComponentPlayer player, int id, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			this.m_subsystemTerrain = subsystemTerrain;
			this.Point = Point1;
			this.id1 = id;
			this.player = player;
            LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/RoundDialog"));
			this.m_title = this.Children.Find<LabelWidget>("Round_Dialog.Title", true);
			this.Mode = this.Children.Find<LabelWidget>("Round_Dialog.Mode", true);
			this.mPosition = this.Children.Find<LabelWidget>("Round_Dialog.Pos", true);
			this.Icon_select = this.Children.Find<ButtonWidget>("Round_Dialog.Icon_select", true);
			this.mselect_pos = this.Children.Find<ButtonWidget>("Round_Dialog.Select_pos", true);
			this.m_radius = this.Children.Find<SliderWidget>("Round_Dialog.Slider1", true);
			this.m_lenght = this.Children.Find<SliderWidget>("Round_Dialog.Slider2", true);
			this.plusButton = this.Children.Find<ButtonWidget>("Round_Dialog.Button4", true);
			this.minusButton = this.Children.Find<ButtonWidget>("Round_Dialog.Button3", true);
			this.lenght_plusButton = this.Children.Find<ButtonWidget>("Round_Dialog.Button2", true);
			this.lenght_minusButton = this.Children.Find<ButtonWidget>("Round_Dialog.Button1", true);
			this.mSelect_mode = this.Children.Find<ButtonWidget>("Round_Dialog.Select_mode", true);
			this.m_okButton = this.Children.Find<ButtonWidget>("Round_Dialog.OK", true);
			this.m_cancelButton = this.Children.Find<ButtonWidget>("Round_Dialog.Cancel", true);
			this.m_blockIconWidget = this.Children.Find<BlockIconWidget>("Round_Dialog.Icon", true);
			this.m_title.Text = "Round";
			this.mPosition.Text = "Flat";
			this.Mode.Text = "Hollow";
			this.m_radius.MinValue = 1f;
			this.m_radius.MaxValue = 100f;
			this.m_radius.Value = 1f;
			this.m_lenght.MinValue = 1f;
			this.m_lenght.MaxValue = 100f;
			this.m_lenght.Value = 1f;
			this.m_blockIconWidget.Value = id;
			this.names.Add("Soild");
			this.names.Add("Hollow");
			this.names_pos.Add("Flat");
			this.names_pos.Add("Pos_X");
			this.names_pos.Add("Pos_Y");
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

		// Token: 0x06002008 RID: 8200 RVA: 0x000D59A4 File Offset: 0x000D3BA4
		public override void Update()
		{
			this.radius = (int)this.m_radius.Value - 1;
			this.lenght = (int)this.m_lenght.Value - 1;
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
			if (this.mSelect_mode.IsClicked)
			{
				this.Select_mode(this.m_categories, this.names);
			}
			if (this.mselect_pos.IsClicked)
			{
				this.Select_pos(this.m_categories_pos, this.names_pos);
			}
			if (this.Icon_select.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
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
			this.m_blockIconWidget.Value = this.id1;
			if (this.m_okButton.IsClicked)
			{
				bool s = this.Mode.Text == "Hollow";
				int pos = 1;
				if (this.mPosition.Text == "Flat")
				{
					pos = 1;
				}
				if (this.mPosition.Text == "Pos_X")
				{
					pos = 2;
				}
				if (this.mPosition.Text == "Pos_Y")
				{
					pos = 3;
				}
				Point3 point;
				point.X = this.Point.Value.CellFace.X;
				point.Y = this.Point.Value.CellFace.Y;
				point.Z = this.Point.Value.CellFace.Z;
				API_WE.Round(s, this.radius + 1, this.lenght + 1, pos, this.id1, this.Point, this.m_subsystemTerrain, this.player);
				DialogsManager.HideDialog(this);
			}
			if (base.Input.Cancel || this.m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			this.UpdateControls();
		}

		// Token: 0x06002009 RID: 8201 RVA: 0x000D5C9C File Offset: 0x000D3E9C
		private void UpdateControls()
		{
			this.m_radius.Text = string.Format("{0} blocks", this.radius + 1);
			this.m_lenght.Text = string.Format("{0} blocks", this.lenght + 1);
		}

		// Token: 0x0600200A RID: 8202 RVA: 0x000D5CF0 File Offset: 0x000D3EF0
		public void Select_mode(List<Category> m_categories, List<string> a)
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
					this.Mode.Text = text;
				}));
			}
		}

		// Token: 0x0600200B RID: 8203 RVA: 0x000D5D74 File Offset: 0x000D3F74
		public void Select_pos(List<Category> m_categories, List<string> a)
		{
			if (WEUserManager.GetUserByComponent(player).m_componentPlayer != null)
			{
				DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, m_categories, 56f, delegate(object c)
				{
					LabelWidget labelWidget = new LabelWidget();
					labelWidget.Text = ((Category)c).Name;
					labelWidget.Color = Color.White;
					int horizontalAlignment = 1;
					labelWidget.HorizontalAlignment = (WidgetAlignment)horizontalAlignment;
					int verticalAlignment = 1;
					labelWidget.VerticalAlignment = (WidgetAlignment)verticalAlignment;
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

		// Token: 0x04001833 RID: 6195
		private ComponentPlayer player;

		// Token: 0x04001834 RID: 6196
		private LabelWidget m_title;

		// Token: 0x04001835 RID: 6197
		private SliderWidget m_radius;

		// Token: 0x04001836 RID: 6198
		private SliderWidget m_lenght;

		// Token: 0x04001837 RID: 6199
		private ButtonWidget plusButton;

		// Token: 0x04001838 RID: 6200
		private ButtonWidget minusButton;

		// Token: 0x04001839 RID: 6201
		private ButtonWidget lenght_plusButton;

		// Token: 0x0400183A RID: 6202
		private ButtonWidget lenght_minusButton;

		// Token: 0x0400183B RID: 6203
		private ButtonWidget m_okButton;

		// Token: 0x0400183C RID: 6204
		private ButtonWidget m_cancelButton;

		// Token: 0x0400183D RID: 6205
		private List<string> names = new List<string>();

		// Token: 0x0400183E RID: 6206
		private List<Category> m_categories = new List<Category>();

		// Token: 0x0400183F RID: 6207
		private List<string> names_pos = new List<string>();

		// Token: 0x04001840 RID: 6208
		private List<Category> m_categories_pos = new List<Category>();

		// Token: 0x04001841 RID: 6209
		private ButtonWidget mSelect_mode;

		// Token: 0x04001842 RID: 6210
		private ButtonWidget mselect_pos;

		// Token: 0x04001843 RID: 6211
		private ButtonWidget Icon_select;

		// Token: 0x04001844 RID: 6212
		private LabelWidget mPosition;

		// Token: 0x04001845 RID: 6213
		private LabelWidget Mode;

		// Token: 0x04001846 RID: 6214
		private BlockIconWidget m_blockIconWidget;

		// Token: 0x04001847 RID: 6215
		private int radius;

		// Token: 0x04001848 RID: 6216
		private int lenght;

		// Token: 0x04001849 RID: 6217
		private int id1;

		// Token: 0x0400184A RID: 6218
		private Position POS;

		// Token: 0x0400184B RID: 6219
		private TerrainRaycastResult? Point;

		// Token: 0x0400184C RID: 6220
		private SubsystemTerrain m_subsystemTerrain;
	}
}
