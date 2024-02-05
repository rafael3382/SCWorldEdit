using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Engine;
using Engine.Media;
using Game;

namespace API_WE_Mod
{
	public class ImageDialog : Dialog
	{
		private ListPanelWidget list_build;

		private ButtonWidget AddButton;

		private ButtonWidget MoreButton;

		private ButtonWidget Cancel;

		private LabelWidget Title;

		private List<Category> m_categories = new List<Category>();

		private List<string> names_item = new List<string>();

		public static readonly string Path_img = Storage.CombinePaths(ZoneDialog.Path_mod, "img");

		private string selected_item;

		private ComponentPlayer player;

		private TerrainRaycastResult Point1 => WEUserManager.GetUserByComponent(player).Point1;

		private TerrainRaycastResult Point2 => WEUserManager.GetUserByComponent(player).Point2;

		private TerrainRaycastResult Point3 => WEUserManager.GetUserByComponent(player).Point3;

		private SubsystemTerrain subsystemTerrain => WEUserManager.GetUserByComponent(player).m_subsystemTerrain;

		public ImageDialog(ComponentPlayer player, TerrainRaycastResult? Point1, TerrainRaycastResult? Point2, TerrainRaycastResult? Point3, SubsystemTerrain subsystemTerrain)
		{
			//this.player = player;
			/*this.Point1 = Point1;
			this.Point2 = Point2;
			this.Point3 = Point3;*/
			//this.subsystemTerrain = subsystemTerrain;
			this.player = player;
			LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/ZoneWidget"));
			list_build = Children.Find<ListPanelWidget>("ListView");
			AddButton = Children.Find<ButtonWidget>("AddButton");
			MoreButton = Children.Find<ButtonWidget>("MoreButton");
			Cancel = Children.Find<ButtonWidget>("Cancel");
			Title = Children.Find<LabelWidget>("Dialog.Title");
			Title.Text = "Image in SC";
			names_item.Add("Delete");
			names_item.Add("Build");
			foreach (string item in names_item)
			{
				m_categories.Add(new Category
				{
					Name = item
				});
			}
			MoreButton.IsEnabled = false;
			update_builds();
		}

		private void update_builds()
		{
			list_build.ClearItems();
			Storage.CreateDirectory(Path_img);
			foreach (string path in Storage.ListFileNames(Path_img))
			{
				if (Path.GetExtension(path) == ".jpg" || Path.GetExtension(path) == ".jpeg" || Path.GetExtension(path) == ".png")
				{
					list_build.AddItem(Path.GetFileName(path));
				}
			}
		}

		public override void Update()
		{
			selected_item = list_build.SelectedItem as string;
			if (Cancel.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			if (MoreButton.IsClicked)
			{
				DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, m_categories, 56f, delegate(object c)
				{
					LabelWidget obj = new LabelWidget
					{
						Text = ((Category)c).Name,
						Color = Color.White
					};
					int num = (int)(obj.HorizontalAlignment = WidgetAlignment.Center);
					int num2 = (int)(obj.VerticalAlignment = WidgetAlignment.Center);
					return obj;
				}, delegate(object c)
				{
					if (c != null)
					{
						int index = m_categories.IndexOf((Category)c);
						string text = names_item[index];
						if (text == "Delete")
						{
							DialogsManager.ShowDialog(player.GameWidget, new MessageDialog("Warning", "Image will deleted ", "Delete", "Cancel", delegate(MessageDialogButton b)
							{
								if (b == MessageDialogButton.Button1)
								{
									Storage.DeleteFile(Storage.CombinePaths(Path_img, selected_item));
									update_builds();
								}
							}));
						}
						if (text == "Build")
						{
							if (!WEUserManager.GetUserByComponent(player).Point3Set)
							{
								DialogsManager.HideDialog(this);
								player.ComponentGui.DisplaySmallMessage("You have not selected point 3", Color.White, blinking: false, playNotificationSound: false);
							}
							else
							{
								Point3 point = default(Point3);
								point.X = Point3.CellFace.X;
								point.Y = Point3.CellFace.Y;
								point.Z = Point3.CellFace.Z;
								//SettingsManager.UIScale = 0.2f;
								DialogsManager.ShowDialog(player.GameWidget, new Img(player, Storage.CombinePaths(Path_img, selected_item), Point3, subsystemTerrain));
								DialogsManager.HideDialog(this);
							}
						}
					}
				}));
			}
			if (list_build.SelectedItem != null)
			{
				MoreButton.IsEnabled = true;
			}
			else
			{
				MoreButton.IsEnabled = false;
			}
			if (AddButton.IsClicked)
			{
			    base.Input.EnterText(this, "URL image", " ", 100, delegate(string s)
			    {
				    if (s != null)
				    {
				        WebManager.Get(s, null, null, null, delegate (byte[] result)
                        {
					        DateTime now = DateTime.Now;
					
					        string ext = Path.GetExtension(s);
				            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") 
                            {
                               if (result.Length > 8 && result[0] == (byte) 0x89 && result[1] == (byte) 0x50 && result[2] == (byte) 0x4E  && result[3] == (byte) 0x47 && result[4] == (byte) 0x0D && result[5] == (byte) 0x0A && result[6] == (byte) 0x1A  && result[7] == (byte) 0x0A)
                                   ext = ".png";
                               else if (result.Length > 4 && result[0] == (byte) 0xFF && result[1] == (byte) 0xD8 && result[result.Length-1] == (byte) 0xFF && result[result.Length-2] == (byte) 0xD9)
                                   ext = ".jpg";
                               else
                               {
                                   DialogsManager.ShowDialog(player.GameWidget, new MessageDialog("Invalid Format", "Image format is not supported.", null, "OK", null));
                                   return;
                               }
                            }
					        string filename = GetNameFromLink(s) + ext;
                            if (filename == "") filename = $"Image {now.Year:D4}-{now.Month:D2}-{now.Day:D2} {now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}" + ext;
					        using (Stream file = Storage.OpenFile(Storage.CombinePaths(Path_img, filename), OpenFileMode.Create))
					        {
					            file.Write(result, 0, result.Length);
					        }
                        },
                        delegate (Exception error)
                        {
                            DialogsManager.ShowDialog(player.GameWidget, new MessageDialog("Error download image", error.ToString(), null, "OK", null));
                            Log.Error("Failed downloading image, Full error:\n"+error.ToString());
                        });
					    update_builds();
				    }
			    });
		    }
	    }
	    
	    public static string UnclutterLink(string address)
        {
            try
            {
                string text = address;
                int num = text.IndexOf('&');
                if (num > 0)
                {
                    text = text.Remove(num);
                }
                int num2 = text.IndexOf('?');
                if (num2 > 0)
                {
                    text = text.Remove(num2);
                }
                return Uri.UnescapeDataString(text);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetNameFromLink(string address)
        {
            try
            {
                string filename = Storage.GetFileNameWithoutExtension(UnclutterLink(address));
                
              bool isValid = !string.IsNullOrEmpty(filename) &&
                                       filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
              if (isValid) return filename;
              return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
	}
}
