using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;
using Engine.Media;
using Engine.Serialization;
using FindDominantColour;
using Game;
using TemplatesDatabase;
using XmlUtilities;

namespace API_WE_Mod
{
	public class API_WE
	{
	    private static int[] arr = new int[16]
		{
			16456, 49224, 81992, 114760, 147528, 180296, 213064, 245832, 278600, 311368,
			344136, 376904, 409672, 442440, 475208, 507976
		};

		public static Engine.Color[] p_c__;
        
        public static string n_w;
        
	    public static void draw_img(bool use_custom_palette, bool mirror, int ofset_paletr, int deep_color, bool on_furn, int size, int furn_resol, bool pos, bool rotate, string pa, Point3 start, SubsystemTerrain subsystemTerrain, ComponentPlayer p)
		{
		    pos = !pos;
			try
			{
				Image image = null;
				try
				{
					image = Image.Load(pa);
				}
				catch (Exception)
				{
					p.ComponentGui.DisplaySmallMessage($"Error when reading image.", Engine.Color.White, blinking: true, playNotificationSound: true);
					return;
				}
				if (!on_furn)
				{
					furn_resol = 1;
				}
				if (!mirror)
				{
				    Image mirrored = new Image(image.Width, image.Height);
					for (int j = 0; j < image.Width; j++)
					{
						for (int k = 0; k < image.Height; k++)
						{
						    mirrored.SetPixel(image.Width-1-j, k, image.GetPixel(j, k));
                        }
                    }
                    image = mirrored;
				}
				if (pos)
				{
					Image rotated = new Image(image.Height, image.Width);
					for (int j = 0; j < rotated.Width; j++)
					{
						for (int k = 0; k < rotated.Height; k++)
						{
						    rotated.SetPixel(j, rotated.Height-1-k, image.GetPixel(k, j));
                        }
                    }
                    image = rotated;
				}
				
				// Scale and invert Y
				Image scaledimage = new Image(image.Width / size, image.Height / size);
				for (int j = 0; j < scaledimage.Width; j++)
				{
					for (int k = 0; k < scaledimage.Height; k++)
					{
		                scaledimage.SetPixel(scaledimage.Width-1-j, k, image.GetPixel(j * size, k * size));
		            }
		        }
		        image = scaledimage;
		        
				int num = 0;
				int maxFurniture = subsystemTerrain.SubsystemFurnitureBlockBehavior.m_furnitureDesigns.Length;
				for (int i = 0; i < maxFurniture; i++)
				{
					if (subsystemTerrain.SubsystemFurnitureBlockBehavior.GetDesign(i) != null)
					{
						num++;
					}
				}
				num = maxFurniture - num;
				int num2 = (image.Width / furn_resol) * (image.Height / furn_resol);
				if (on_furn && num2 > num)
				{
					p.ComponentGui.DisplaySmallMessage($"Too many furniture designs.\n{num2} designs needed.\n{num} available designs.", Color.Red, blinking: true, playNotificationSound: true);
				}
				else
				{
					if (use_custom_palette)
					{
						top_colors(ofset_paletr, deep_color, image, subsystemTerrain.SubsystemPalette);
					}
					new List<int>();
					for (int l = 0; l < image.Width / furn_resol; l++)
					{
						for (int m = 0; m < image.Height / furn_resol; m++)
						{
							if (on_furn)
							{
								List<int> list = new List<int>();
								bool flag = false;
								for (int n = 0; n < furn_resol; n++)
								{
									for (int num3 = 0; num3 < furn_resol; num3++)
									{
										if (get_block_id(GetClosestColor(subsystemTerrain.SubsystemPalette.m_colors, image.GetPixel(l * furn_resol + n, m * furn_resol + num3)), subsystemTerrain.SubsystemPalette) != 0 && !flag)
										{
											flag = true;
										}
										list.Add(get_block_id(GetClosestColor(subsystemTerrain.SubsystemPalette.m_colors, image.GetPixel(l * furn_resol + n, m * furn_resol + num3)), subsystemTerrain.SubsystemPalette));
									}
								}
								if (flag)
								{
									int value = create_dsg(rotate, pos, furn_resol, list, subsystemTerrain, ("x " + l + "  y " + m).ToString());
									if (pos)
									{
										if (rotate)
										{
											subsystemTerrain.ChangeCell(start.X, start.Y + l, start.Z + m, value);
										}
										else
										{
											subsystemTerrain.ChangeCell(start.X + m, start.Y + l, start.Z, value);
										}
									}
									else
									{
										subsystemTerrain.ChangeCell(start.X + m, start.Y, start.Z + l, value);
									}
								}
							}
							else
							{
								int value2 = get_block_id(GetClosestColor(subsystemTerrain.SubsystemPalette.m_colors, image.GetPixel(l, m)), subsystemTerrain.SubsystemPalette);
								if (pos)
								{
									if (rotate)
									{
										subsystemTerrain.ChangeCell(start.X, start.Y + l, start.Z + m, value2);
									}
									else
									{
										subsystemTerrain.ChangeCell(start.X + m, start.Y + l, start.Z, value2);
									}
								}
								else
								{
									subsystemTerrain.ChangeCell(start.X + m, start.Y, start.Z + l, value2);
								}
							}
						}
					}
					p.ComponentGui.DisplaySmallMessage(string.Format("Success " + num2 + " blocks placed"), Color.White, blinking: true, playNotificationSound: true);
				}
			}
			catch (Exception ex2)
			{
				p.ComponentGui.DisplaySmallMessage(string.Format("Failed. reason: " + ex2.Message), Color.White, blinking: true, playNotificationSound: true);
				Log.Error(ex2);
			}
		}
		
		private static Engine.Color GetClosestColor(Engine.Color[] colorArray, Engine.Color baseColor)
		{
			if (baseColor.A == 0)
			{
				return new Engine.Color(0, 0, 0, 0);
			}
			Engine.Color minValue = Color.Black;
			int minDiff = int.MaxValue;
			foreach (Engine.Color color in colorArray)
			    if (GetDiff(color, baseColor) < minDiff)
			    {
			        minValue = color; 
                    minDiff = GetDiff(color, baseColor);
			    }
			return minValue;
		}

		private static int GetDiff(Engine.Color color, Engine.Color baseColor)
		{
			int num = color.A - baseColor.A;
			int num2 = color.R - baseColor.R;
			int num3 = color.G - baseColor.G;
			int num4 = color.B - baseColor.B;
			return num * num + num2 * num2 + num3 * num3 + num4 * num4;
		}

		public static void top_colors(int ofs_p, int dc, Engine.Media.Image img, SubsystemPalette p)
		{
			List<Engine.Color> list = new List<Engine.Color>(img.Width * img.Height);
			for (int i = 0; i < img.Width; i++)
			{
				for (int j = 0; j < img.Height; j++)
				{
					list.Add(img.GetPixel(i, j));
				}
			}
			IList<Color> list2 = new KMeansClusteringCalculator().Calculate(dc, list, 6.0);
			
			for (int k = 0; k < list2.Count; k++)
			{
				p.m_colors[k + ofs_p] = new Engine.Color(list2[k].R, list2[k].G, list2[k].B);
			}
			
			// Save palette
			SubsystemGameInfo subsystemGameInfo = p.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            subsystemGameInfo.WorldSettings.Palette.Colors = p.m_colors;
		}
		
		private static XElement GetGameInfoNode(XElement projectNode)
		{
			XElement xElement = (from n in projectNode.Element("Subsystems").Elements("Values")
				where XmlUtils.GetAttributeValue(n, "Name", string.Empty) == "GameInfo"
				select n).FirstOrDefault();
			if (xElement != null)
			{
				return xElement;
			}
			throw new InvalidOperationException("GameInfo node not found in project.");
		}

		public static int get_block_id(Engine.Color c, SubsystemPalette p)
		{
			if (c.A == 0)
			{
				return 0;
			}
			for (int i = 0; i < p.m_colors.Length; i++)
			{
				if (p.m_colors[i] == c)
				{
					return arr[i];
				}
			}
			return 1;
		}

		public static int create_dsg(bool rot, bool pos, int resol, List<int> color, SubsystemTerrain ter, string name)
		{
			FurnitureDesign furnitureDesign = null;
			Dictionary<Point3, int> dictionary = new Dictionary<Point3, int>();
			Point3 point = new Point3(0, 0, 0);
			new Point3(0, 0, 0);
			for (int i = 0; i < resol; i++)
			{
				for (int j = 0; j < resol; j++)
				{
					if (pos)
					{
						if (rot)
						{
							dictionary.Add(new Point3(0, j, i), color[i + j * resol]);
						}
						else
						{
							dictionary.Add(new Point3(i, j, 0), color[i + j * resol]);
						}
					}
					else
					{
						dictionary.Add(new Point3(i, 0, j), color[i + j * resol]);
					}
				}
			}
			furnitureDesign = new FurnitureDesign(ter);
			int[] array = new int[resol * resol * resol];
			foreach (KeyValuePair<Point3, int> item in dictionary)
			{
				Point3 point2 = item.Key - point;
				array[point2.X + point2.Y * resol + point2.Z * resol * resol] = item.Value;
			}
			furnitureDesign.SetValues(resol, array);
			furnitureDesign.Name = name;
			FurnitureDesign furnitureDesign2 = ter.SubsystemFurnitureBlockBehavior.TryAddDesign(furnitureDesign);
			return Terrain.MakeBlockValue(227, 0, FurnitureBlock.SetDesignIndex(0, furnitureDesign2.Index, furnitureDesign2.ShadowStrengthFactor, isLightEmitter: false));
		}
		
		public static void Round(bool s, int radius, int lenght, int pos, int id, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain, ComponentPlayer player)
		{
			Task.Run(delegate()
			{
				Convert.ToString(pos);
				for (int i = 0; i < lenght; i++)
				{
					for (int j = -radius; j <= 0; j++)
					{
						for (int k = -radius; k <= 0; k++)
						{
							if ((int)Math.Sqrt((double)(j * j + k * k)) <= radius && (!s || (int)Math.Sqrt((double)((j - 1) * (j - 1) + k * k)) > radius || (int)Math.Sqrt((double)(j * j + (k - 1) * (k - 1))) > radius))
							{
								int num = i;
								if (pos == 2)
								{
									SubsystemTerrain subsystemTerrain2 = subsystemTerrain;
									TerrainRaycastResult value = Point1.Value;
									int num2 = value.CellFace.Point.X + num;
									value = Point1.Value;
									int num3 = value.CellFace.Point.Y - j;
									value = Point1.Value;
									subsystemTerrain2.ChangeCell(num2, num3, value.CellFace.Point.Z - k, id, true);
									SubsystemTerrain subsystemTerrain3 = subsystemTerrain;
									value = Point1.Value;
									int num4 = value.CellFace.Point.X + num;
									value = Point1.Value;
									int num5 = value.CellFace.Point.Y + j;
									value = Point1.Value;
									subsystemTerrain3.ChangeCell(num4, num5, value.CellFace.Point.Z - k, id, true);
									SubsystemTerrain subsystemTerrain4 = subsystemTerrain;
									value = Point1.Value;
									int num6 = value.CellFace.Point.X + num;
									value = Point1.Value;
									int num7 = value.CellFace.Point.Y - j;
									value = Point1.Value;
									subsystemTerrain4.ChangeCell(num6, num7, value.CellFace.Point.Z + k, id, true);
									SubsystemTerrain subsystemTerrain5 = subsystemTerrain;
									value = Point1.Value;
									int num8 = value.CellFace.Point.X + num;
									value = Point1.Value;
									int num9 = value.CellFace.Point.Y + j;
									value = Point1.Value;
									subsystemTerrain5.ChangeCell(num8, num9, value.CellFace.Point.Z + k, id, true);
								}
								if (pos == 1)
								{
									SubsystemTerrain subsystemTerrain6 = subsystemTerrain;
									TerrainRaycastResult value = Point1.Value;
									int num10 = value.CellFace.Point.X + j;
									value = Point1.Value;
									int num11 = value.CellFace.Point.Y + num;
									value = Point1.Value;
									subsystemTerrain6.ChangeCell(num10, num11, value.CellFace.Point.Z + k, id, true);
									SubsystemTerrain subsystemTerrain7 = subsystemTerrain;
									value = Point1.Value;
									int num12 = value.CellFace.Point.X + j;
									value = Point1.Value;
									int num13 = value.CellFace.Point.Y + num;
									value = Point1.Value;
									subsystemTerrain7.ChangeCell(num12, num13, value.CellFace.Point.Z - k, id, true);
									SubsystemTerrain subsystemTerrain8 = subsystemTerrain;
									value = Point1.Value;
									int num14 = value.CellFace.Point.X - j;
									value = Point1.Value;
									int num15 = value.CellFace.Point.Y + num;
									value = Point1.Value;
									subsystemTerrain8.ChangeCell(num14, num15, value.CellFace.Point.Z + k, id, true);
									SubsystemTerrain subsystemTerrain9 = subsystemTerrain;
									value = Point1.Value;
									int num16 = value.CellFace.Point.X - j;
									value = Point1.Value;
									int num17 = value.CellFace.Point.Y + num;
									value = Point1.Value;
									subsystemTerrain9.ChangeCell(num16, num17, value.CellFace.Point.Z - k, id, true);
								}
								if (pos == 3)
								{
									SubsystemTerrain subsystemTerrain10 = subsystemTerrain;
									TerrainRaycastResult value = Point1.Value;
									int num18 = value.CellFace.Point.X + j;
									value = Point1.Value;
									int num19 = value.CellFace.Point.Y - k;
									value = Point1.Value;
									subsystemTerrain10.ChangeCell(num18, num19, value.CellFace.Point.Z + num, id, true);
									SubsystemTerrain subsystemTerrain11 = subsystemTerrain;
									value = Point1.Value;
									int num20 = value.CellFace.Point.X + j;
									value = Point1.Value;
									int num21 = value.CellFace.Point.Y + k;
									value = Point1.Value;
									subsystemTerrain11.ChangeCell(num20, num21, value.CellFace.Point.Z + num, id, true);
									SubsystemTerrain subsystemTerrain12 = subsystemTerrain;
									value = Point1.Value;
									int num22 = value.CellFace.Point.X - j;
									value = Point1.Value;
									int num23 = value.CellFace.Point.Y + k;
									value = Point1.Value;
									subsystemTerrain12.ChangeCell(num22, num23, value.CellFace.Point.Z + num, id, true);
									SubsystemTerrain subsystemTerrain13 = subsystemTerrain;
									value = Point1.Value;
									int num24 = value.CellFace.Point.X - j;
									value = Point1.Value;
									int num25 = value.CellFace.Point.Y - k;
									value = Point1.Value;
									subsystemTerrain13.ChangeCell(num24, num25, value.CellFace.Point.Z + num, id, true);
								}
							}
						}
					}
				}
			});
		}

		public static void LinePoint(Point3 v, Point3 v2, int id, SubsystemTerrain subsystemTerrain)
		{
			int lengin = Math.Max(Math.Max(Math.Abs(v.X - v2.X), Math.Abs(v.Y - v2.Y)), Math.Abs(v.Z - v2.Z));
			Task.Run(delegate()
			{
				int num = 0;
				for (int i = 0; i <= lengin; i++)
				{
					int cellValueFast = subsystemTerrain.Terrain.GetCellValueFast(v.X + (int)Math.Round((double)i / (double)lengin * (double)(v2.X - v.X)), v.Y + (int)Math.Round((double)i / (double)lengin * (double)(v2.Y - v.Y)), v.Z + (int)Math.Round((double)i / (double)lengin * (double)(v2.Z - v.Z)));
					if (cellValueFast != id && Terrain.ExtractContents(cellValueFast) != id)
					{
						subsystemTerrain.ChangeCell(v.X + (int)Math.Round((double)i / (double)lengin * (double)(v2.X - v.X)), v.Y + (int)Math.Round((double)i / (double)lengin * (double)(v2.Y - v.Y)), v.Z + (int)Math.Round((double)i / (double)lengin * (double)(v2.Z - v.Z)), id, true);
						num++;
					}
				}
			});
		}

		public static void Sphere(bool s, int r, int id, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			Task.Run(delegate()
			{
				int num = 0;
				for (int i = -r; i <= 0; i++)
				{
					for (int j = -r; j <= 0; j++)
					{
						for (int k = -r; k <= 0; k++)
						{
							if ((int)Math.Sqrt((double)(i * i + j * j + k * k)) <= r && (!s || (int)Math.Sqrt((double)((i - 1) * (i - 1) + j * j + k * k)) > r || (int)Math.Sqrt((double)(i * i + (j - 1) * (j - 1) + k * k)) > r || (int)Math.Sqrt((double)(i * i + j * j + (k - 1) * (k - 1))) > r))
							{
								SubsystemTerrain subsystemTerrain2 = subsystemTerrain;
								TerrainRaycastResult value = Point1.Value;
								int num2 = value.CellFace.Point.X + i;
								value = Point1.Value;
								int num3 = value.CellFace.Point.Y + j;
								value = Point1.Value;
								subsystemTerrain2.ChangeCell(num2, num3, value.CellFace.Point.Z + k, id, true);
								SubsystemTerrain subsystemTerrain3 = subsystemTerrain;
								value = Point1.Value;
								int num4 = value.CellFace.Point.X - i;
								value = Point1.Value;
								int num5 = value.CellFace.Point.Y + j;
								value = Point1.Value;
								subsystemTerrain3.ChangeCell(num4, num5, value.CellFace.Point.Z + k, id, true);
								SubsystemTerrain subsystemTerrain4 = subsystemTerrain;
								value = Point1.Value;
								int num6 = value.CellFace.Point.X - i;
								value = Point1.Value;
								int num7 = value.CellFace.Point.Y - j;
								value = Point1.Value;
								subsystemTerrain4.ChangeCell(num6, num7, value.CellFace.Point.Z + k, id, true);
								SubsystemTerrain subsystemTerrain5 = subsystemTerrain;
								value = Point1.Value;
								int num8 = value.CellFace.Point.X - i;
								value = Point1.Value;
								int num9 = value.CellFace.Point.Y + j;
								value = Point1.Value;
								subsystemTerrain5.ChangeCell(num8, num9, value.CellFace.Point.Z - k, id, true);
								SubsystemTerrain subsystemTerrain6 = subsystemTerrain;
								value = Point1.Value;
								int num10 = value.CellFace.Point.X - i;
								value = Point1.Value;
								int num11 = value.CellFace.Point.Y - j;
								value = Point1.Value;
								subsystemTerrain6.ChangeCell(num10, num11, value.CellFace.Point.Z - k, id, true);
								SubsystemTerrain subsystemTerrain7 = subsystemTerrain;
								value = Point1.Value;
								int num12 = value.CellFace.Point.X + i;
								value = Point1.Value;
								int num13 = value.CellFace.Point.Y - j;
								value = Point1.Value;
								subsystemTerrain7.ChangeCell(num12, num13, value.CellFace.Point.Z + k, id, true);
								SubsystemTerrain subsystemTerrain8 = subsystemTerrain;
								value = Point1.Value;
								int num14 = value.CellFace.Point.X + i;
								value = Point1.Value;
								int num15 = value.CellFace.Point.Y + j;
								value = Point1.Value;
								subsystemTerrain8.ChangeCell(num14, num15, value.CellFace.Point.Z - k, id, true);
								SubsystemTerrain subsystemTerrain9 = subsystemTerrain;
								value = Point1.Value;
								int num16 = value.CellFace.Point.X + i;
								value = Point1.Value;
								int num17 = value.CellFace.Point.Y - j;
								value = Point1.Value;
								subsystemTerrain9.ChangeCell(num16, num17, value.CellFace.Point.Z - k, id, true);
								num += 8;
							}
						}
					}
				}
			});
		}

		public static void Prism(bool s, int r, int blockID, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			int num = 0;
			for (int i = -r; i <= r; i++)
			{
				for (int j = -r; j <= r; j++)
				{
					for (int k = -r; k <= r; k++)
					{
						if (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) <= r && (!s || Math.Abs(i) + Math.Abs(j) + Math.Abs(k) >= r))
						{
							TerrainRaycastResult value = Point1.Value;
							int num2 = value.CellFace.Point.X + i;
							value = Point1.Value;
							int num3 = value.CellFace.Point.Y + j;
							value = Point1.Value;
							subsystemTerrain.ChangeCell(num2, num3, value.CellFace.Point.Z + k, blockID, true);
							num++;
						}
					}
				}
			}
		}

		public static void Square(bool s, int r, int lenght, Position pos, int blockID, TerrainRaycastResult? Point1, SubsystemTerrain subsystemTerrain)
		{
			Task.Run(delegate()
			{
				int num = 0;
				for (int i = 0; i < lenght; i++)
				{
					for (int j = -r; j <= r; j++)
					{
						for (int k = -r; k <= r; k++)
						{
							if (Math.Abs(j) + Math.Abs(k) <= r && (!s || Math.Abs(j) + Math.Abs(k) >= r))
							{
								int num2 = i;
								if (pos == Position.flat)
								{
									SubsystemTerrain subsystemTerrain2 = subsystemTerrain;
									TerrainRaycastResult value = Point1.Value;
									int num3 = value.CellFace.Point.X + num2;
									value = Point1.Value;
									int num4 = value.CellFace.Point.Y + j;
									value = Point1.Value;
									subsystemTerrain2.ChangeCell(num3, num4, value.CellFace.Point.Z + k, blockID, true);
								}
								else if (pos == Position.X)
								{
									SubsystemTerrain subsystemTerrain3 = subsystemTerrain;
									TerrainRaycastResult value = Point1.Value;
									int num5 = value.CellFace.Point.X + j;
									value = Point1.Value;
									int num6 = value.CellFace.Point.Y + num2;
									value = Point1.Value;
									subsystemTerrain3.ChangeCell(num5, num6, value.CellFace.Point.Z + k, blockID, true);
								}
								else
								{
									SubsystemTerrain subsystemTerrain4 = subsystemTerrain;
									TerrainRaycastResult value = Point1.Value;
									int num7 = value.CellFace.Point.X + j;
									value = Point1.Value;
									int num8 = value.CellFace.Point.Y + k;
									value = Point1.Value;
									subsystemTerrain4.ChangeCell(num7, num8, value.CellFace.Point.Z + num2, blockID, true);
								}
								num++;
							}
						}
					}
				}
			});
		}

		public static void CreativeMaze(Point3 Start, Point3 End, int BlockID, SubsystemTerrain subsystemTerrain, ComponentPlayer player)
		{
			if (Start.X < End.X)
			{
				int x = Start.X;
				Start.X = End.X;
				End.X = x;
			}
			if (Start.Y < End.Y)
			{
				int y = Start.Y;
				Start.Y = End.Y;
				End.Y = y;
			}
			if (Start.Z < End.Z)
			{
				int z = Start.Z;
				Start.Z = End.Z;
				End.Z = z;
			}
			if (Start.Z - End.Z < 5 || Start.X - End.X < 5)
			{
				player.ComponentGui.DisplaySmallMessage("Too small", Color.White, true, true);
				return;
			}
			Task.Run(delegate()
			{
				int num = 0;
				int num2 = Start.X - End.X;
				int num3 = Start.Z - End.Z;
				bool[,] boolArray = new Maze(num2 / 2, num3 / 2).GetBoolArray();
				for (int i = 0; i <= ((num2 % 2 != 0) ? (num2 - 1) : num2); i++)
				{
					for (int j = 0; j <= ((num3 % 2 != 0) ? (num3 - 1) : num3); j++)
					{
						if ((i != 1 || j != 0) && (i != ((num2 % 2 != 0) ? (num2 - 1) : num2) || j != ((num3 % 2 != 0) ? (num3 - 1) : num3) - 1) && boolArray[i, j])
						{
							for (int k = 0; k <= Start.Y - End.Y; k++)
							{
								subsystemTerrain.ChangeCell(End.X + i, End.Y + k, End.Z + j, BlockID, true);
								num++;
							}
						}
					}
				}
				player.ComponentGui.DisplaySmallMessage(string.Format("Success, {0} blocks placed", num), Color.White, true, true);
			});
		}

		public static void Rectangle(int pos, int blockId, Point3 Start, Point3 End, ComponentPlayer player, SubsystemTerrain subsystemTerrain)
		{
			if (Start.X < End.X)
			{
				int x = Start.X;
				Start.X = End.X;
				End.X = x;
			}
			if (Start.Y < End.Y)
			{
				int y = Start.Y;
				Start.Y = End.Y;
				End.Y = y;
			}
			if (Start.Z < End.Z)
			{
				int z = Start.Z;
				Start.Z = End.Z;
				End.Z = z;
			}
			Task.Run(delegate()
			{
				int num = 0;
				for (int i = 0; i <= Start.X - End.X; i++)
				{
					for (int j = 0; j <= Start.Y - End.Y; j++)
					{
						for (int k = 0; k <= Start.Z - End.Z; k++)
						{
							if ((pos != 1 || i <= 0 || i >= Start.X - End.X || j <= 0 || j >= Start.Y - End.Y || k <= 0 || k >= Start.Z - End.Z) && (pos != 2 || ((i < 0 || i > Start.X - End.X || j <= 0 || j >= Start.Y - End.Y || k <= 0 || k >= Start.Z - End.Z) && (j < 0 || j > Start.Y - End.Y || i <= 0 || i >= Start.X - End.X || k <= 0 || k >= Start.Z - End.Z) && (k < 0 || k > Start.Z - End.Z || j <= 0 || j >= Start.Y - End.Y || i <= 0 || i >= Start.X - End.X))))
							{
								subsystemTerrain.ChangeCell(End.X + i, End.Y + j, End.Z + k, blockId, true);
								num++;
							}
						}
					}
				}
				player.ComponentGui.DisplaySmallMessage(string.Format("Success, {0} blocks placed", num), Color.White, true, true);
			});
		}

		public static void Mountain(Point3 Start, int Radius, int Size, SubsystemTerrain subsystemTerrain, int id, int id1, int id2, ComponentPlayer player)
		{
			Task.Run(delegate()
			{
				Game.Random random = new Game.Random();
				double num = 1.5707963267948966;
				float num2 = (float)Size + 0.8f;
				int num3 = 0;
				double num4 = 10.0;
				double num5 = 15.0;
				float num6 = random.Float((float)num4, (float)num5) + 10f;
				for (int i = -Radius; i < Radius; i++)
				{
					for (int j = -Radius; j < Radius; j++)
					{
						double num7 = Math.Cos(num * (double)i / (double)Radius) * Math.Cos(num * (double)j / (double)Radius) * (double)num2;
						double num8 = Math.Sin(num * (double)i * 1.39999997615814 / (double)Radius + 4.0) * Math.Cos(num * (double)j * 1.39999997615814 / (double)Radius + 7.0) * (double)num2 * 0.25;
						double num9 = Math.Cos(num * (double)i * 1.39999997615814 * 2.0 / (double)Radius + 4.0 * (double)num6) * Math.Sin(num * (double)j * 1.39999997615814 * 2.0 / (double)Radius + 8.0 * (double)num6) * (double)num2 * 0.200000002980232;
						double num10 = Math.Sin(num * (double)i * 1.39999997615814 * 4.0 / (double)Radius + 4.0 * (double)num6 * 1.5) * Math.Sin(num * (double)j * 1.39999997615814 * 4.0 / (double)Radius + 8.0 * (double)num6 * 1.5) * (double)num2 * 0.150000005960464;
						double num11 = num8;
						double num12 = num7 - num11 + num9 - num10;
						if (num12 > 0.0)
						{
							int num13 = 0;
							while ((double)num13 <= num12)
							{
								if (num13 > 3)
								{
									subsystemTerrain.ChangeCell(Start.X + i, Start.Y + (int)num12 - num13, Start.Z + j, id, true);
									num3++;
								}
								else if (num13 > 0)
								{
									subsystemTerrain.ChangeCell(Start.X + i, Start.Y + (int)num12 - num13, Start.Z + j, id1, true);
									num3++;
								}
								else if (num13 == 0)
								{
									subsystemTerrain.ChangeCell(Start.X + i, Start.Y + (int)num12, Start.Z + j, id2, true);
									num3++;
								}
								num13++;
							}
						}
					}
				}
				player.ComponentGui.DisplaySmallMessage(string.Format("Success, {0} blocks placed", num3), Color.White, true, true);
			});
		}
		
		public static void ShowBlockDialog()
		{
		/*
		   DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", new object[]
			{
			    AirBlock.Index,
				WaterBlock.Index,
				MagmaBlock.Index,
				GrassBlock.Index,
				DirtBlock.Index,
				SandBlock.Index,
				Granite.Index,
				BasaltBlock.Index,
				LimestoneBlock.Index,
				Sandstone.Index,
				Cobblestone.Index,
				StoneBricksBlock.Index,
				BrickWallBlock.Index,
				PlanksBlock.Index,
				IronBlock.Index,
				CopperBlock.Index,
				GlassBlock.Index,
				IceBlock.Index,
				MarbleBlock.Index,
				DiamondBlock.Index,
				MalachiteBlock.Index,
				BedrockBlock.Index,
				"Other blocks..."
			    
			}, 72f, delegate(object obj)
	        {
				ContainerWidget containerWidget = (ContainerWidget) LoadWidget(null, ContentManager.Get<XElement>("Widgets/SelectBlockItem"), null);
				if (obj is int index)
				{
				    containerWidget.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
				    containerWidget.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
				}
				if (obj is string option)
				{
				    containerWidget.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
				    containerWidget.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
				}
				
				return containerWidget;
			}, delegate(object index)
			{
				this.id1 = (int)index;
			}));*/
		}

		public static string GetDisplayName(int value)
		{
			int contents = Terrain.ExtractContents(value);
			if (contents < 0 || contents > BlocksManager.Blocks.Length)
				return "Invalid block.";
			Block block = BlocksManager.Blocks[contents];
			if (GameManager.Project != null)
				return block.GetDisplayName(GameManager.Project.FindSubsystem<SubsystemTerrain>(), value);
			return block.DefaultDisplayName;
		}
		
		public static T Clone<T>(T obj) where T : new()
        {
            T clone = new T();
        
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(clone, prop.GetValue(obj));
                }
            }
            foreach (var prop in obj.GetType().GetFields())
            {
                prop.SetValue(clone, prop.GetValue(obj));
            }
        
            return clone;
        }

		
		public static bool IsDesktop
        {
            get
            {
                switch (Environment.OSVersion.Platform.ToString())
                {
                    case "Windows":
                    case "windows":
                        return true;
                    case "Android":
                    case "android":
                    case "Unix":
                    case "unix":
                        return false;
                    default:
                        return false;
                }
            }
        }

		internal static int blockCount;

		internal static List<BlockMem> BlockList = new List<BlockMem>();
	}
}
