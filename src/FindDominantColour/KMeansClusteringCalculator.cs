using System;
using System.Collections.Generic;
using Engine;
using System.Linq;

namespace FindDominantColour
{
	public class KMeansClusteringCalculator
	{
		public IList<Color> Calculate(int k, IList<Color> colours, double threshold = 0.0)
		{
			List<KCluster> list = new List<KCluster>();
			System.Random random = new System.Random();
			List<int> list2 = new List<int>();
			while (list.Count < k)
			{
				int num = random.Next(0, colours.Count);
				if (!list2.Contains(num))
				{
					list2.Add(num);
					KCluster item = new KCluster(colours[num]);
					list.Add(item);
				}
			}
			bool flag = false;
			do
			{
				flag = false;
				foreach (Color colour in colours)
				{
					double num2 = 3.4028234663852886E+38;
					KCluster kCluster = null;
					foreach (KCluster item2 in list)
					{
						double num3 = item2.DistanceFromCentre(colour);
						if (num3 < num2)
						{
							num2 = num3;
							kCluster = item2;
						}
					}
					kCluster.Add(colour);
				}
				foreach (KCluster item3 in list)
				{
					if (item3.RecalculateCentre(threshold))
					{
						flag = true;
					}
				}
			}
			while (flag);
			return (from c in list
				orderby c.PriorCount descending
				select c.Centre).ToList();
		}
	}
}
