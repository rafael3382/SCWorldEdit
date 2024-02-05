using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;

namespace API_WE_Mod
{
	public class Img : Dialog
	{
		private ComponentPlayer player;

		private ButtonWidget m_okButton;

		private ButtonWidget m_cancelButton;

		private ButtonWidget m_type_creatingButton;

		private LabelWidget m_type_creatingLabel;

		private SliderWidget m_resizeSlider;

		private SliderWidget m_furniture_resolutionSlider;

		private SliderWidget m_deep_colorSlider;

		private SliderWidget m_color_ofsetSlider;

		private ButtonWidget m_posButton;

		private ButtonWidget m_rotButton;

		private LabelWidget m_posLabel;

		private LabelWidget m_rotLabel;

		private CheckboxWidget m_colors_useBox;

		private CheckboxWidget m_color_saveBox;

		private CheckboxWidget m_mirrorBox;

		private Point3 Point;

		private SubsystemTerrain m_subsystemTerrain;

		private string pos_txt;

		private string rot_txt;

		private string t_c_txt;

		private string path_img;

		private int resize;

		private int furnit_resol;

		private int deep_color;

		private int ofst_color;

		private bool type_cr;

		private bool pos;

		private bool rot;

		public Img(ComponentPlayer player, string path, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			path_img = path;
			int x = Point1.Value.CellFace.Point.X;
			int y = Point1.Value.CellFace.Point.Y;
			Point = new Point3(x, y, Point1.Value.CellFace.Point.Z);
			m_subsystemTerrain = subsystemTerrain;
			this.player = player;
			LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/ImgDialog"));
			m_okButton = Children.Find<ButtonWidget>("Img_Dialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("Img_Dialog.Cancel");
			m_type_creatingButton = Children.Find<ButtonWidget>("Img_Dialog.type_cr_btn");
			m_type_creatingLabel = Children.Find<LabelWidget>("Img_Dialog.type_cr_text");
			m_resizeSlider = Children.Find<SliderWidget>("Img_Dialog.resize_sl");
			m_furniture_resolutionSlider = Children.Find<SliderWidget>("Img_Dialog.furn_res_sl");
			m_deep_colorSlider = Children.Find<SliderWidget>("Img_Dialog.Slider_deep");
			m_color_ofsetSlider = Children.Find<SliderWidget>("Img_Dialog.ofst_sl");
			m_posButton = Children.Find<ButtonWidget>("Img_Dialog.pos_sel");
			m_rotButton = Children.Find<ButtonWidget>("Img_Dialog.rot_sel");
			m_posLabel = Children.Find<LabelWidget>("Img_Dialog.pos_txt");
			m_rotLabel = Children.Find<LabelWidget>("Img_Dialog.rot_txt");
			m_colors_useBox = Children.Find<CheckboxWidget>("Img_Dialog.Line0");
			m_mirrorBox = Children.Find<CheckboxWidget>("Img_Dialog.mirror");
			m_resizeSlider.MinValue = 1f;
			m_resizeSlider.MaxValue = 10f;
			m_furniture_resolutionSlider.MinValue = 1f;
			m_furniture_resolutionSlider.MaxValue = 64f;
			m_color_ofsetSlider.MinValue = 0f;
			m_color_ofsetSlider.MaxValue = 8f;
			m_deep_colorSlider.MinValue = 2f;
			m_deep_colorSlider.MaxValue = 16f;
			t_c_txt = "Furniture";
			m_colors_useBox.IsChecked = false;
			m_resizeSlider.Value = 2f;
			m_furniture_resolutionSlider.Value = 16f;
			m_deep_colorSlider.Value = 16f;
			pos_txt = "Vertical";
			rot_txt = "Front";
		}

		public override void Update()
		{
			resize = (int)m_resizeSlider.Value;
			furnit_resol = (int)m_furniture_resolutionSlider.Value;
			deep_color = (int)m_deep_colorSlider.Value;
			ofst_color = (int)m_color_ofsetSlider.Value;
			if (m_type_creatingButton.IsClicked)
			{
				Select_type_creating();
			}
			if (m_posButton.IsClicked)
			{
				Select_pos();
			}
			if (m_rotButton.IsClicked)
			{
				Select_rot();
			}
			if (m_okButton.IsClicked)
			{
				if (t_c_txt == "Furniture")
				{
					type_cr = true;
				}
				else
				{
					type_cr = false;
				}
				if (pos_txt == "Vertical")
				{
					pos = false;
				}
				else
				{
					pos = true;
				}
				if (rot_txt == "Front")
				{
					rot = true;
				}
				else
				{
					rot = false;
				}
				API_WE.draw_img(m_colors_useBox.IsChecked, m_mirrorBox.IsChecked, ofst_color, deep_color, type_cr, resize, furnit_resol, pos, rot, path_img, Point, m_subsystemTerrain, player);
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
			m_resizeSlider.Text = ((int)m_resizeSlider.Value).ToString();
			m_furniture_resolutionSlider.Text = ((int)m_furniture_resolutionSlider.Value).ToString();
			m_deep_colorSlider.Text = ((int)m_deep_colorSlider.Value).ToString();
			m_color_ofsetSlider.Text = ((int)m_color_ofsetSlider.Value).ToString();
			m_type_creatingLabel.Text = t_c_txt;
			m_posLabel.Text = pos_txt;
			m_rotLabel.Text = rot_txt;
		}

		public void Select_type_creating()
		{
			List<string> names = new List<string>();
			names.Add("Furniture");
			names.Add("Blocks");
			DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, names, 56f, delegate(object c)
			{
				LabelWidget obj = new LabelWidget
				{
					Text = c.ToString(),
					Color = Color.White
				};
				int num = (int)(obj.HorizontalAlignment = WidgetAlignment.Center);
				int num2 = (int)(obj.VerticalAlignment = WidgetAlignment.Center);
				return obj;
			}, delegate(object c)
			{
				if (c != null)
				{
					int index = names.IndexOf(c.ToString());
					string text = names[index];
					t_c_txt = text;
				}
			}));
		}

		public void Select_pos()
		{
			List<string> names = new List<string>();
			names.Add("Vertical");
			names.Add("Horizontally");
			DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, names, 56f, delegate(object c)
			{
				LabelWidget obj = new LabelWidget
				{
					Text = c.ToString(),
					Color = Color.White
				};
				int num = (int)(obj.HorizontalAlignment = WidgetAlignment.Center);
				int num2 = (int)(obj.VerticalAlignment = WidgetAlignment.Center);
				return obj;
			}, delegate(object c)
			{
				if (c != null)
				{
					int index = names.IndexOf(c.ToString());
					string text = names[index];
					pos_txt = text;
				}
			}));
		}

		public void Select_rot()
		{
			List<string> names = new List<string>();
			names.Add("Front");
			names.Add("Side");
			DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, names, 56f, delegate(object c)
			{
				LabelWidget obj = new LabelWidget
				{
					Text = c.ToString(),
					Color = Color.White
				};
				int num = (int)(obj.HorizontalAlignment = WidgetAlignment.Center);
				int num2 = (int)(obj.VerticalAlignment = WidgetAlignment.Center);
				return obj;
			}, delegate(object c)
			{
				if (c != null)
				{
					int index = names.IndexOf(c.ToString());
					string text = names[index];
					rot_txt = text;
				}
			}));
		}
	}
}
