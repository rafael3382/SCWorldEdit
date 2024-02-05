using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace API_WE_Mod
{
    public class BlockGrid
    {
        public Point3 Size;
        public Point3 Sign = new Point3(1,1,1);
        private int[] cells;
        
        public int BlockCount => cells.Length;
        
        public BlockGrid(int sizeX, int sizeY, int sizeZ)
        {
            cells = new int[sizeX * sizeY * sizeZ];
            Size = new Point3(sizeX, sizeY, sizeZ);
        }
        
        public void SetBlock(int x, int y, int z, int value)
        {
            cells[PositionToIndex(x, y, z)] = value;
        }
        
        public int PositionToIndex(int x, int y, int z)
        {
            return x * Sign.X + 
                        z * Sign.Z * Size.X + 
                        y * Sign.Y * Size.X * Size.Z;
        }
        
        public void Paste(Point3 startPoint)
        {
            for (int index=0; index<cells.Length; index++)
            {
                Point3 blockPoint = new Point3(index % Size.X, index / (Size.X * Size.Z), index % (Size.X * Size.Z) / Size.X) * Sign + startPoint;
                WEOperationManager.SetCell(blockPoint.X, blockPoint.Y, blockPoint.Z, cells[index]);
            }
        }
    }
}