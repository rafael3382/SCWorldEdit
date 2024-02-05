using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	public class Square : Dialog
	{
		public Square(ComponentPlayer player, int id, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			m_subsystemTerrain = subsystemTerrain;
			Point = Point1;
			id1 = id;
			this.player = player;
            LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/SquareDialog"));
			m_title = Children.Find<LabelWidget>("Square_Dialog.Title", true);
			Mode = Children.Find<LabelWidget>("Square_Dialog.Mode", true);
			mPosition = Children.Find<LabelWidget>("Square_Dialog.Pos", true);
			Icon_select = Children.Find<ButtonWidget>("Square_Dialog.Icon_select", true);
			mselect_pos = Children.Find<ButtonWidget>("Square_Dialog.Select_pos", true);
			m_radius = Children.Find<SliderWidget>("Square_Dialog.Slider1", true);
			m_lenght = Children.Find<SliderWidget>("Square_Dialog.Slider2", true);
			plusButton = Children.Find<ButtonWidget>("Square_Dialog.Button4", true);
			minusButton = Children.Find<ButtonWidget>("Square_Dialog.Button3", true);
			lenght_plusButton = Children.Find<ButtonWidget>("Square_Dialog.Button2", true);
			lenght_minusButton = Children.Find<ButtonWidget>("Square_Dialog.Button1", true);
			mSelect_mode = Children.Find<ButtonWidget>("Square_Dialog.Select_mode", true);
			m_okButton = Children.Find<ButtonWidget>("Square_Dialog.OK", true);
			m_cancelButton = Children.Find<ButtonWidget>("Square_Dialog.Cancel", true);
			m_blockIconWidget = Children.Find<BlockIconWidget>("Square_Dialog.Icon", true);
			m_title.Text = "Square";
			mPosition.Text = "Flat";
			Mode.Text = "Hollow";
			m_radius.MinValue = 1f;
			m_radius.MaxValue = 100f;
			m_radius.Value = 1f;
			m_lenght.MinValue = 1f;
			m_lenght.MaxValue = 100f;
			m_lenght.Value = 1f;
			m_blockIconWidget.Value = id;
			names.Add("Soild");
			names.Add("Hollow");
			names_pos.Add("Flat");
			names_pos.Add("Pos_X");
			names_pos.Add("Pos_Y");
			foreach (string name in names)
			{
				m_categories.Add(new Category
				{
					Name = name
				});
			}
			foreach (string name2 in names_pos)
			{
				m_categories_pos.Add(new Category
				{
					Name = name2
				});
			}
		}

		public override void Update()
		{
			radius = (int)m_radius.Value - 1;
			lenght = (int)m_lenght.Value - 1;
			m_blockIconWidget.Value = id1;
			if (plusButton.IsClicked)
			{
				m_radius.Value = MathUtils.Min(m_radius.Value + 1f, (float)((int)m_radius.MaxValue));
			}
			if (minusButton.IsClicked)
			{
				m_radius.Value = MathUtils.Max(m_radius.Value - 1f, (float)((int)m_radius.MinValue));
			}
			if (lenght_plusButton.IsClicked)
			{
				m_lenght.Value = MathUtils.Min(m_lenght.Value + 1f, (float)((int)m_lenght.MaxValue));
			}
			if (lenght_minusButton.IsClicked)
			{
				m_lenght.Value = MathUtils.Max(m_lenght.Value - 1f, (float)((int)m_lenght.MinValue));
			}
			if (mSelect_mode.IsClicked)
			{
				Select_mode(m_categories, names);
			}
			if (mselect_pos.IsClicked)
			{
				Select_pos(m_categories_pos, names_pos);
			}
			if (Icon_select.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new int[]
				{
				    AirBlock.Index,
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
					id1 = (int)index;
				}));
			}
			if (m_okButton.IsClicked)
			{
				bool s = Mode.Text == "Hollow";
				int pos = 1;
				if (mPosition.Text == "Flat")
				{
					pos = 1;
				}
				if (mPosition.Text == "Pos_X")
				{
					pos = 0;
				}
				if (mPosition.Text == "Pos_Y")
				{
					pos = 3;
				}
				API_WE.Square(s, radius + 1, lenght + 1, (Position)pos, id1, Point, m_subsystemTerrain);
				DialogsManager.HideDialog(this);
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			UpdateControls();
		}

		private void UpdateControls()
		{
			m_radius.Text = string.Format("{0} blocks", radius + 1);
			m_lenght.Text = string.Format("{0} blocks", lenght + 1);
		}

		public void Select_mode(List<Category> m_categories, List<string> a)
		{
			if (player != null)
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
					Mode.Text = text;
				}));
			}
		}

		public void Select_pos(List<Category> m_categories, List<string> a)
		{
			if (player != null)
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
					mPosition.Text = text;
				}));
			}
		}

		private ComponentPlayer player;

		private LabelWidget m_title;

		private SliderWidget m_radius;

		private SliderWidget m_lenght;

		private ButtonWidget plusButton;

		private ButtonWidget minusButton;

		private ButtonWidget lenght_plusButton;

		private ButtonWidget lenght_minusButton;

		private ButtonWidget m_okButton;

		private ButtonWidget m_cancelButton;

		private List<string> names = new List<string>();

		private List<Category> m_categories = new List<Category>();

		private List<string> names_pos = new List<string>();

		private List<Category> m_categories_pos = new List<Category>();

		private ButtonWidget mSelect_mode;

		private ButtonWidget mselect_pos;

		private ButtonWidget Icon_select;

		private LabelWidget mPosition;

		private LabelWidget Mode;

		private BlockIconWidget m_blockIconWidget;

		private int radius;

		private int lenght;

		private int id1;

		private TerrainRaycastResult? Point;

		private SubsystemTerrain m_subsystemTerrain;
	}
}
