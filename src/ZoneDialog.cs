using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using Engine;
using Game;

namespace API_WE_Mod
{
	public class ZoneDialog : Dialog
	{
	    public static List<IBuildFormat> BuildFormats = new List<IBuildFormat>();
	
	    public List<IBuildFormat> ListUsedFormats = new List<IBuildFormat>();
	
	    static ZoneDialog()
        {
            BuildFormats.Add(new OneKeyFormat());
            BuildFormats.Add(new WorldEditIntroFormat());
        }
	    public ZoneDialog(ComponentPlayer player, TerrainRaycastResult Point1, TerrainRaycastResult Point2, TerrainRaycastResult Point3, SubsystemTerrain subsystemTerrain)
		{
			//player = player;
			//Point1 = Point1;
			//Point2 = Point2;
			//Point3 = Point3;
		    //subsystemTerrain = subsystemTerrain;
			this.player = player;
			LoadContents(this, ContentManager.Get<XElement>("WE/DialogsWE/ZoneWidget"));
			list_build = Children.Find<ListPanelWidget>("ListView", true);
			AddButton = Children.Find<ButtonWidget>("AddButton", true);
			MoreButton = Children.Find<ButtonWidget>("MoreButton", true);
			Cancel = Children.Find<ButtonWidget>("Cancel", true);
			names_item.Add("Delete");
			names_item.Add("Paste");
			foreach (string name in names_item)
			{
				m_categories.Add(new Category
				{
					Name = name
				});
			}
			MoreButton.IsEnabled = false;
			update_builds();
		}

		private void update_builds()
		{
		    try
		    {
			    list_build.ClearItems();
    			ListUsedFormats.Clear();
			    Storage.CreateDirectory(ZoneDialog.Path_mod);
			    foreach (string file in Storage.ListFileNames(ZoneDialog.Path_mod))
			    {
				    foreach (IBuildFormat format in BuildFormats)
				    {
				        if (format.Test(Storage.CombinePaths(ZoneDialog.Path_mod, file)))
				        {
				            list_build.AddItem(file);
				            ListUsedFormats.Add(format);
				            break;
				        }
			    	}
			    }
			}
			catch (Exception e)
			{
			   Log.Information("World Edit Path: "+Path_mod);
			    Log.Error(e);
			}
		}

		public override void Update()
		{
			selected_item = (list_build.SelectedItem as string);
			if (player == null) Log.Error("ZoneDialog.player is null!");
			if (Cancel.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			if (MoreButton.IsClicked)
			{
				DialogsManager.ShowDialog(player.GameWidget, new ListSelectionDialog(string.Empty, m_categories, 56f, itemWidgetFactory: delegate(object c)
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
				    try
                    {
					if (c == null)
					{
						return;
					}
					string a = ((Category) c).Name;
					if (a == "Delete")
					{
						DialogsManager.ShowDialog(player.GameWidget, new MessageDialog("Warning", "The build will be deleted ", "Delete", "Cancel", delegate(MessageDialogButton b)
						{
							Storage.DeleteFile(Path.Combine(ZoneDialog.Path_mod, selected_item));
							update_builds();
						}));
					}
					if (a == "Rename")
					{
						base.Input.EnterText(ParentWidget, "Edit Build Name", selected_item, 20, delegate(string s)
						{
						});
					}
					if (a == "Paste")
					{
						if (!WEUserManager.GetUserByComponent(player).Point3Set)
						{
							DialogsManager.HideDialog(this);
							player.ComponentGui.DisplaySmallMessage("You have not selected point 3", Color.White, false, false);
							return;
						}
						Point3 point = new Point3(Point3.CellFace.X, Point3.CellFace.Y, Point3.CellFace.Z);
						ListUsedFormats[list_build.SelectedIndex.Value].PasteFromFile(Path.Combine(ZoneDialog.Path_mod, selected_item), point);
						DialogsManager.HideDialog(this);
					}
					} 
                   catch (Exception e)
                   {
                       Log.Error(e);
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
				if (!(WEUserManager.GetUserByComponent(player).Point1Set && WEUserManager.GetUserByComponent(player).Point2Set))
				{
					player.ComponentGui.DisplaySmallMessage("You have not selected points 1,2", Color.White, false, false);
					DialogsManager.HideDialog(this);
					return;
				}
				DialogsManager.ShowDialog(this, new TextBoxDialog("Enter build name", "New build", 20, delegate(string s)
				{
				    try
                    {
					    if (s == null)
					    {
						    return;
					    }
					    name_file = s;
					    Point3 point;
					    point.X = Point1.CellFace.X;
					    point.Y = Point1.CellFace.Y;
					    point.Z = Point1.CellFace.Z;
					    Point3 point2;
					    point2.X = Point2.CellFace.X;
					    point2.Y = Point2.CellFace.Y;
					    point2.Z = Point2.CellFace.Z;
					    BuildFormats[BuildFormats.Count-1].SaveToFile(Storage.CombinePaths(ZoneDialog.Path_mod, name_file + ".scbuild"), point, point2);
					    update_builds();
					}
                    catch (Exception ex)
                    {
                       Log.Error(ex);
                    }
				}));
			}
		}

		private ListPanelWidget list_build;

		private ButtonWidget AddButton;

		private ButtonWidget MoreButton;

		private ButtonWidget Cancel;

		private List<Category> m_categories = new List<Category>();

		private List<string> names_item = new List<string>();

		public static string Path_mod
		{
		    get
            {
                FieldInfo ExternelPathField = typeof(ModsManager).GetField("ExternelPath", BindingFlags.Public | BindingFlags.Static);
                string ExternalPath = (ExternelPathField == null) ? "app:" : (string) ExternelPathField.GetValue(null);
                return Storage.CombinePaths(ExternalPath, "WorldEdit");
            }
		}

		private string selected_item;

		private string name_file;

		private ComponentPlayer player;

		private TerrainRaycastResult Point1 => WEUserManager.GetUserByComponent(player).Point1;

		private TerrainRaycastResult Point2 => WEUserManager.GetUserByComponent(player).Point2;

		private TerrainRaycastResult Point3 => WEUserManager.GetUserByComponent(player).Point3;

		private SubsystemTerrain subsystemTerrain => WEUserManager.GetUserByComponent(player).m_subsystemTerrain;
	}
}
