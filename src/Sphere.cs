using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	// Token: 0x02000562 RID: 1378
	public class Sphere : Dialog
	{
		// Token: 0x0600202C RID: 8236 RVA: 0x000D8ACC File Offset: 0x000D6CCC
		public Sphere(ComponentPlayer player, int id, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			this.Point = null;
			this.Point = Point1;
			this.m_subsystemTerrain = subsystemTerrain;
			this.id1 = id;
			this.player = player;
            LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/SphereDialog"));
			this.m_title = this.Children.Find<LabelWidget>("Sphere_Dialog.Title", true);
			this.Mode = this.Children.Find<LabelWidget>("Sphere_Dialog.Mode", true);
			this.Icon_select = this.Children.Find<ButtonWidget>("Sphere_Dialog.Icon_select", true);
			this.m_radius = this.Children.Find<SliderWidget>("Sphere_Dialog.Slider1", true);
			this.plusButton = this.Children.Find<ButtonWidget>("Sphere_Dialog.Button4", true);
			this.minusButton = this.Children.Find<ButtonWidget>("Sphere_Dialog.Button3", true);
			this.mSelect_mode = this.Children.Find<ButtonWidget>("Sphere_Dialog.Select_mode", true);
			this.m_okButton = this.Children.Find<ButtonWidget>("Sphere_Dialog.OK", true);
			this.m_cancelButton = this.Children.Find<ButtonWidget>("Sphere_Dialog.Cancel", true);
			this.m_blockIconWidget = this.Children.Find<BlockIconWidget>("Sphere_Dialog.Icon", true);
			this.m_title.Text = "Sphere";
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

		// Token: 0x0600202D RID: 8237 RVA: 0x000D8CDC File Offset: 0x000D6EDC
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
				API_WE.Sphere(s, this.radius, this.id1, this.Point, this.m_subsystemTerrain);
				DialogsManager.HideDialog(this);
			}
			if (base.Input.Cancel || this.m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			this.UpdateControls();
		}

		// Token: 0x0600202E RID: 8238 RVA: 0x000143B1 File Offset: 0x000125B1
		private void UpdateControls()
		{
			this.m_radius.Text = string.Format("{0} blocks", this.radius + 1);
		}

		// Token: 0x0600202F RID: 8239 RVA: 0x000D8E78 File Offset: 0x000D7078
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

		// Token: 0x0400187E RID: 6270
		private ComponentPlayer player;

		// Token: 0x0400187F RID: 6271
		private LabelWidget m_title;

		// Token: 0x04001880 RID: 6272
		private SliderWidget m_radius;

		// Token: 0x04001881 RID: 6273
		private ButtonWidget plusButton;

		// Token: 0x04001882 RID: 6274
		private ButtonWidget minusButton;

		// Token: 0x04001883 RID: 6275
		private ButtonWidget m_okButton;

		// Token: 0x04001884 RID: 6276
		private ButtonWidget m_cancelButton;

		// Token: 0x04001885 RID: 6277
		private List<string> names = new List<string>();

		// Token: 0x04001886 RID: 6278
		private List<Category> m_categories = new List<Category>();

		// Token: 0x04001887 RID: 6279
		private ButtonWidget mSelect_mode;

		// Token: 0x04001888 RID: 6280
		private ButtonWidget Icon_select;

		// Token: 0x04001889 RID: 6281
		private LabelWidget Mode;

		// Token: 0x0400188A RID: 6282
		private BlockIconWidget m_blockIconWidget;

		// Token: 0x0400188B RID: 6283
		private int radius;

		// Token: 0x0400188C RID: 6284
		private int id1;

		// Token: 0x0400188D RID: 6285
		private TerrainRaycastResult? Point;

		// Token: 0x0400188E RID: 6286
		private SubsystemTerrain m_subsystemTerrain;
	}
}
