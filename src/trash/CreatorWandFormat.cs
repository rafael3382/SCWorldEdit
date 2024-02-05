using Engine;
using System;
using System.IO;
using Engine.Serialization;
using API_WE_Mod;

public class CreatorWandFormat : IBuildFormat
{
    public bool Test(string path)
    {
        Stream stream = Storage.OpenFile(path, OpenFileMode.Read);
        EngineBinaryWriter binWriter = new EngineBinaryWriter(stream, leaveOpen: false);
        string header = string.Empty;
        try
        {
            header = binWriter.ReadString();
        }
        catch
        {
        }
        stream.Dispose();
        return header.StartsWith("This is the v") && header.EndsWith(" version of the creator API");
    }
    
    public int SaveToFile(string path, Point3 point1, Point3 point2)
	{
		throw new Exception("Creator Wand saving to file is not supported yet.");
	}

	public int PasteFromFile(string path, Point3 point3)
	{
		Stream fileStream = Storage.OpenFile(path, OpenFileMode.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		string str = binaryReader.ReadString();
        int num3 = binaryReader.ReadInt32();
        binaryReader.ReadChar();
        Point2 PFovB = binaryReader.ReadPoint2();
        binaryReader.ReadPoint3();
        
		binaryReader.Dispose();
		fileStream.Dispose();
		return num;
	}
}