using Engine;
using System.IO;
using Engine.Serialization;
using API_WE_Mod;

public class OneKeyFormat : IBuildFormat
{
    public bool Test(string path)
    {
        Stream stream = Storage.OpenFile(path, OpenFileMode.Read);
        byte[] header = new byte[7];
        stream.Read(header, 0, 7);
        stream.Dispose();
        return header[0] == 'O' && header[1] == 'n' && header[2] == 'e' && header[3] == 'K' && header[4] == 'e' && header[5] == 'y' && header[6] == 0;
    }
    
    public int SaveToFile(string path, Point3 point1, Point3 point2)
	{
		int startX = MathUtils.Min(point1.X, point2.X);
        int endX = MathUtils.Max(point1.X, point2.X);
        int startY = MathUtils.Min(point1.Y, point2.Y);
        int endY = MathUtils.Max(point1.Y, point2.Y);
        int startZ = MathUtils.Min(point1.Z, point2.Z);
        int endZ = MathUtils.Max(point1.Z, point2.Z);
       
		try { Storage.OpenFile(path, OpenFileMode.Create)?.Dispose(); } catch {}
		Stream fileStream = Storage.OpenFile(path, OpenFileMode.ReadWrite);
		EngineBinaryWriter engineBinaryWriter = new EngineBinaryWriter(fileStream);
		
		foreach (char c in "OneKey")
		{
		    engineBinaryWriter.Write((byte)c);
		}
		engineBinaryWriter.Write((byte)0);
		engineBinaryWriter.Write(0);
		int blockCount = 0;
		BlockMem blmem = new BlockMem();
		for (int x = 0; x <= endX - startX; x++)
        {
            for (int y = 0; y <= endY - startY; y++)
            {
                for (int z = 0; z <= endZ - startZ; z++)
                {
                    int X, Y, Z;
                    if (point1.X > point2.X)
                    {
                        blmem.x = -x;
                        X = point1.X - x;
                    }
                    else
                    {
                        blmem.x = x;
                        X = point1.X + x;
                    }

                    if (point1.Y > point2.Y)
                    {
                        blmem.y = -y;
                        Y = point1.Y - y;
                    }
                    else
                    {
                        blmem.y = y;
                        Y = point1.Y + y;
                    }

                    if (point1.Z > point2.Z)
                    {
                        blmem.z = -z;
                        Z = point1.Z - z;
                    }
                    else
                    {
                        blmem.z = z;
                        Z = point1.Z + z;
                    }
                    
					blmem.id = WEOperationManager.GetCell(X, Y, Z);
					engineBinaryWriter.Write(blmem.x);
					engineBinaryWriter.Write(blmem.y);
					engineBinaryWriter.Write(blmem.z);
					engineBinaryWriter.Write(blmem.id);
					blockCount++;
				}
			}
		}
		engineBinaryWriter.BaseStream.Position = 7L;
		engineBinaryWriter.Write(blockCount);
		engineBinaryWriter.Dispose();
		fileStream.Dispose();
		return blockCount;
	}

	public int PasteFromFile(string path, Point3 point3)
	{
		Stream fileStream = Storage.OpenFile(path, OpenFileMode.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		binaryReader.BaseStream.Position = 7L;
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = point3.X + binaryReader.ReadInt32();
			int num3 = point3.Y + binaryReader.ReadInt32();
			int num4 = point3.Z + binaryReader.ReadInt32();
			int num5 = binaryReader.ReadInt32();
			WEOperationManager.SetCell(num2, num3, num4, num5);
		}
		binaryReader.Dispose();
		fileStream.Dispose();
		return num;
	}
}