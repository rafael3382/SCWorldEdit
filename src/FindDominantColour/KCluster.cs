using System;
using System.Collections.Generic;
using Engine;

namespace FindDominantColour
{
	public class KCluster
	{
		private readonly List<Color> _colours;

		public Color Centre { get; set; }

		public int PriorCount { get; set; }

		public KCluster(Color centre)
		{
			Centre = centre;
			_colours = new List<Color>();
		}

		public void Add(Color colour)
		{
			_colours.Add(colour);
		}

		public bool RecalculateCentre(double threshold = 0.0)
		{
			Color color;
			if (_colours.Count > 0)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				foreach (Color colour in _colours)
				{
					num += (float)(int)colour.R;
					num2 += (float)(int)colour.G;
					num3 += (float)(int)colour.B;
				}
				color = new Color(
                    (int)Math.Round(num / (float)_colours.Count), 
                    (int)Math.Round(num2 / (float)_colours.Count), 
                    (int)Math.Round(num3 / (float)_colours.Count)
                );
			}
			else
			{
				color = new Color(0, 0, 0, 0);
			}
			double num4 = EuclideanDistance(Centre, color);
			Centre = color;
			PriorCount = _colours.Count;
			_colours.Clear();
			return num4 > threshold;
		}

		public double DistanceFromCentre(Color colour)
		{
			return EuclideanDistance(colour, Centre);
		}

		public static double EuclideanDistance(Color c1, Color c2)
		{
			return Math.Sqrt(Math.Pow(c1.R - c2.R, 2.0) + Math.Pow(c1.G - c2.G, 2.0) + Math.Pow(c1.B - c2.B, 2.0));
		}
	}
}
