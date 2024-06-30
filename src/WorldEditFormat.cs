using Engine;
using System;
using Game;
using System.Linq;
using TemplatesDatabase;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Engine.Serialization;
using API_WE_Mod;

public class WorldEditIntroFormat : IBuildFormat
{
    // Introduction of the new world edit format
    public const string CodeName = "WE.Build1.1";
    
    public bool Test(string path)
    {
        try
        {
            Stream stream = Storage.OpenFile(path, OpenFileMode.Read);
            
            byte[] header = new byte[CodeName.Length];
            stream.Read(header, 0, CodeName.Length);
            stream.Dispose();
            for (int i=0; i<CodeName.Length; i++)
            {
                if ((byte) CodeName[i] != header[i])
                {
                    return false;
                }
            }
        }
        catch
        {
        }
        return true;
    }
    
   /* public Point3 PositionFromPoints(Point3 position, BoundingBox rect)
    {
        return position - point1;
    }*/
        
    public void GetSpecials(Point3 point1, Point3 point2, out Dictionary<Point3, SignData> SignDataList, out Dictionary<Point3, IEditableItemData> EditableDataList, out Dictionary<Point3, ComponentInventoryBase> InventoryDataList)
    {
        SignDataList = new Dictionary<Point3, SignData>();
        EditableDataList = new Dictionary<Point3, IEditableItemData>();
        InventoryDataList = new Dictionary<Point3, ComponentInventoryBase>();
        BoundingBox rect = new BoundingBox(new List<Vector3>() { new Vector3(point1), new Vector3(point2) });
        foreach (KeyValuePair<Point3, MemoryBankData> editableData in GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>().m_blocksData)
        {
            if (!rect.Contains(new Vector3(editableData.Key)))
            {
              continue;
           }
           
           EditableDataList.Add(editableData.Key-point1, editableData.Value);
        }
        foreach (KeyValuePair<Point3, TruthTableData> editableData in GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>().m_blocksData)
        {
            if (!rect.Contains(new Vector3(editableData.Key)))
            {
              continue;
            }
            
            EditableDataList.Add(editableData.Key-point1, editableData.Value);
        }
        foreach (KeyValuePair<Point3, SubsystemSignBlockBehavior.TextData> signData in GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>().m_textsByPoint)
        {
            if (!rect.Contains(new Vector3(signData.Key)))
            {
              continue;
            }
            
            SignDataList.Add(signData.Key-point1, new SignData
            {
                Lines = signData.Value.Lines.ToArray(),
                Colors = signData.Value.Colors.ToArray(),
                Url = signData.Value.Url
            });
        }
        foreach (KeyValuePair<Point3, ComponentBlockEntity> blockEntityData in GameManager.Project.FindSubsystem<SubsystemBlockEntities>().m_blockEntities)
        {
            if (!rect.Contains(new Vector3(blockEntityData.Key)))
            {
              continue;
            }
            
            ComponentInventoryBase inv = blockEntityData.Value.Entity.FindComponent<ComponentInventoryBase>(false);
            if (inv == null) continue;
            
            
            InventoryDataList.Add(blockEntityData.Key-point1, inv);
        }
    }
    
    public int SaveToFile(string path, Point3 point1, Point3 point2)
	{
		int startX = MathUtils.Min(point1.X, point2.X);
        int endX = MathUtils.Max(point1.X, point2.X);
        int startY = MathUtils.Min(point1.Y, point2.Y);
        int endY = MathUtils.Max(point1.Y, point2.Y);
        int startZ = MathUtils.Min(point1.Z, point2.Z);
        int endZ = MathUtils.Max(point1.Z, point2.Z);
       
        Point3 size = new Point3(endX - startX + 1, endY - startY + 1, endZ - startZ + 1);
        Point3 sign = new Point3((point1.X > point2.X) ? -1 : 1, (point1.Y > point2.Y) ? -1 : 1, (point1.Z > point2.Z) ? -1 : 1);
        
		try { Storage.OpenFile(path, OpenFileMode.Create)?.Dispose(); } catch {}
		Stream fileStream = Storage.OpenFile(path, OpenFileMode.ReadWrite);
		EngineBinaryWriter bin = new EngineBinaryWriter(fileStream, leaveOpen: true);
		
		foreach (char c in CodeName)
		{
		    bin.Write((byte)c);
		}
		bin.Write(size);
		bin.Write(sign);
		
		bin.Dispose();
		DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
		bin = new EngineBinaryWriter(deflateStream, leaveOpen:true);
		List<int> usedDesigns = new List<int>();
		for (int y = 0; y <= endY - startY; y++)
        {
            for (int z = 0; z <= endZ - startZ; z++)
            {
                for (int x = 0; x <= endX - startX; x++)
                {
                    int X, Y, Z;
                    if (point1.X > point2.X)
                    {
                        X = point1.X - x;
                    }
                    else
                    {
                        X = point1.X + x;
                    }

                    if (point1.Y > point2.Y)
                    {
                        Y = point1.Y - y;
                    }
                    else
                    {
                        Y = point1.Y + y;
                    }

                    if (point1.Z > point2.Z)
                    {
                        Z = point1.Z - z;
                    }
                    else
                    {
                        Z = point1.Z + z;
                    }
                    
                    int value = WEOperationManager.GetCell(X, Y, Z);
                    if (Terrain.ExtractContents(value) == FurnitureBlock.Index)
                    {
                        int design = FurnitureBlock.GetDesignIndex(Terrain.ExtractData(value));
                        if (!usedDesigns.Contains(design))
                            usedDesigns.Add(design);
                    }
					bin.Write(value);
				}
			}
		}
		
		GetSpecials(point1, point2, out Dictionary<Point3, SignData> SignDataList, out Dictionary<Point3, IEditableItemData> EditableDataList, out Dictionary<Point3, ComponentInventoryBase> InventoryDataList);
		
		// Signs
		bin.Write(SignDataList.Count);
		foreach (KeyValuePair<Point3, SignData> signData in SignDataList)
		{
		    bin.Write(signData.Key);
		    for (int i=0; i<4; i++)
		    {
		        bin.Write(signData.Value.Lines[i]);
		        bin.Write(signData.Value.Colors[i].R);
		        bin.Write(signData.Value.Colors[i].G);
		        bin.Write(signData.Value.Colors[i].B);
		    }
		    bin.Write(signData.Value.Url ?? "");
		}
		
		// Editables
		bin.Write(EditableDataList.Count);
		foreach (KeyValuePair<Point3, IEditableItemData> editable in EditableDataList)
		{
		    bin.Write(editable.Key);
		    bin.Write(editable.Value.SaveString());
		    bin.Write(editable.Value is MemoryBankData);
		}
		
		// Inventories
		bin.Write(InventoryDataList.Count);
		foreach (KeyValuePair<Point3, ComponentInventoryBase> inv in InventoryDataList)
		{
		    bin.Write(inv.Key);
		    for (int i=0; i<inv.Value.m_slots.Count; i++)
		    {
		        ComponentInventoryBase.Slot slot = inv.Value.m_slots[i];
		        if (slot.Value == 0 || slot.Count <= 0)
		            continue;
		        bin.Write(i);
		        bin.Write(slot.Value);
		        bin.Write(slot.Count);
		    }
		    bin.Write(-1);
		}
		
		// Furnitures
		var furniture = GameManager.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);
		bin.Write(usedDesigns.Count);
		foreach (int designIndex in usedDesigns)
		{
		    FurnitureDesign design = furniture.GetDesign(designIndex);
		    if (design == null)
		        continue;
		    bin.Write(designIndex);
		    ValuesDictionary save = design.Save();
		    bin.Write(save.Count);
		    foreach (string key in save.Keys)
		    {
		        object value = save[key];
		        bin.Write(key);
		        bin.Write(TypeCache.GetShortTypeName(value.GetType().FullName));
		        bin.Write(HumanReadableConverter.ConvertToString(value));
	    	}
		}
		
		bin.Dispose();
		deflateStream.Dispose();
		fileStream.Dispose();
		return size.X * size.Z * size.Y;
	}

	public int PasteFromFile(string path, Point3 point3)
	{
		Stream fileStream = Storage.OpenFile(path, OpenFileMode.Read);
		EngineBinaryReader bin = new EngineBinaryReader(fileStream);
		bin.BaseStream.Position = (long) CodeName.Length;
		Point3 size = bin.ReadPoint3();
		Point3 sign = bin.ReadPoint3();
		int countXZ = size.X * size.Z;
		int totalBlocks = countXZ * size.Y;
		DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
		bin = new EngineBinaryReader(deflateStream);
		Dictionary<int, List<object[]>> furnitureBlocks = new Dictionary<int, List<object[]>>();
		for (int i = 0; i < totalBlocks; i++)
		{
		    Point3 blockPoint = new Point3(i % size.X, i / (countXZ), i % (countXZ) / size.X) * sign + point3;
			int value = bin.ReadInt32();
			if (Terrain.ExtractContents(value) == FurnitureBlock.Index)
            {
                int design = FurnitureBlock.GetDesignIndex(Terrain.ExtractData(value));
                if (!furnitureBlocks.ContainsKey(design))
                    furnitureBlocks[design] = new List<object[]>();
                furnitureBlocks[design].Add(new object[2] { blockPoint, value });
                continue;
            }
			WEOperationManager.SetCell(blockPoint.X, blockPoint.Y, blockPoint.Z, value);
		}
		
		var membhv = GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>();
        var trbhv = GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>();
        var signbhv = GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>();
        var blockentities = GameManager.Project.FindSubsystem<SubsystemBlockEntities>();
        
		int signCount = bin.ReadInt32();
		for (int signIndex=0; signIndex<signCount; signIndex++)
		{
		    Point3 pos = bin.ReadPoint3() + point3;
		    SignData signData = new SignData();
		    for (int i=0; i<4; i++)
		    {
		        signData.Lines[i] = bin.ReadString();
		        signData.Colors[i].R = bin.ReadByte();
		        signData.Colors[i].G = bin.ReadByte();
		        signData.Colors[i].B = bin.ReadByte();
		    }
		    signData.Url = bin.ReadString();
		    signbhv.SetSignData(pos, signData.Lines, signData.Colors, signData.Url);
		}
		
		// Editables
		int editableCount = bin.ReadInt32();
		for (int editableIndex=0; editableIndex<editableCount; editableIndex++)
		{
		    Point3 pos = bin.ReadPoint3() + point3;
		    string data = bin.ReadString();
		    bool isMem = bin.ReadBoolean();
		    if (isMem)
		    {
		        var mem = new MemoryBankData();
                mem.LoadString(data);
		        membhv.SetBlockData(pos, mem);
		    }
		    else
		    {
		        var table = new TruthTableData();
                table.LoadString(data);
		        trbhv.SetBlockData(pos, table);
		    }
		}
		
		// Inventories
		int inventoryCount = bin.ReadInt32();
		for (int invIndex=0; invIndex<inventoryCount; invIndex++)
		{
		    Point3 pos = bin.ReadPoint3() + point3;
		    if (blockentities.m_blockEntities.TryGetValue(pos, out ComponentBlockEntity target))
            {
                ComponentInventoryBase inv = target.Entity.FindComponent<ComponentInventoryBase>(false);
                if (inv == null)
                    continue;
                
		        while (true)
		        {
		            int slotIndex = bin.ReadInt32();
		            if (slotIndex < 0)
		                break;
		            int slotValue = bin.ReadInt32();
		            int slotCount = bin.ReadInt32();
		            if (slotIndex >= inv.m_slots.Count)
		                continue;
		            inv.m_slots[slotIndex].Value = slotValue;
		            inv.m_slots[slotIndex].Count = slotCount;
		        }
		    }
		}
		
		var furniture = GameManager.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);
		int designCount = bin.ReadInt32();
		int newDesignIndex = 0;
		for (int di=0; di<designCount; di++)
		{
		    int previousDesignIndex = bin.ReadInt32();
		    ValuesDictionary save = new ValuesDictionary();
		    int keyCount = bin.ReadInt32();
		    for (int i=0; i<keyCount; i++)
		    {
		        string keyName = bin.ReadString();
		        Type type = TypeCache.FindType(bin.ReadString(), skipSystemAssemblies: false, throwIfNotFound: true);
		        object obj = HumanReadableConverter.ConvertFromString(type, bin.ReadString());
		        save[keyName] = obj;
	    	}
	        
        	FurnitureDesign design = new FurnitureDesign(0, WEOperationManager.m_subsystemTerrain, save);
	        int finalIndex = -1;
	        if (furniture.m_furnitureDesigns.Length > previousDesignIndex)
	        {
	            if (furniture.m_furnitureDesigns[previousDesignIndex] != null && furniture.m_furnitureDesigns[previousDesignIndex].Compare(design))
	                continue;
	        }
	        
            for (int existingDesignIndex=0; existingDesignIndex<furniture.m_furnitureDesigns.Length; existingDesignIndex++)
	        {
	             FurnitureDesign existingDesign = furniture.m_furnitureDesigns[existingDesignIndex];
	             if (existingDesign == null)
	                 continue;
	             if (existingDesign.Compare(design))
	             {
	                 finalIndex = existingDesignIndex;
	                 break;
	             }
 	       }
            
            
            if (finalIndex == -1)
            {
	            while (newDesignIndex<furniture.m_furnitureDesigns.Length && furniture.m_furnitureDesigns[newDesignIndex] != null)
		        {
		            newDesignIndex++;
		        }
		        if (newDesignIndex >= furniture.m_furnitureDesigns.Length)
		        {
		            newDesignIndex = 0;
		            while (newDesignIndex<furniture.m_furnitureDesigns.Length && furniture.m_furnitureDesigns[newDesignIndex] != null)
		            {
		                newDesignIndex++;
		            }
		            if (newDesignIndex >= furniture.m_furnitureDesigns.Length)
		            {
		                throw new Exception("The number of available furniture designs isn't enough to copy the building completely.");
		            }
		        }
		        finalIndex = newDesignIndex;
	        }
	        design.Index = finalIndex;
	        furniture.m_furnitureDesigns[finalIndex] = design;
	        if (furnitureBlocks.ContainsKey(previousDesignIndex))
	        {
	            foreach (object[] furnitureBlock in furnitureBlocks[previousDesignIndex])
	            {
	                Point3 blockPoint = (Point3) furnitureBlock[0];
	                int value = (int) furnitureBlock[1];
	                value = Terrain.ReplaceData(value, FurnitureBlock.SetDesignIndex(Terrain.ExtractData(value), finalIndex, design.ShadowStrengthFactor, design.IsLightEmitter));
	                WEOperationManager.SetCell(blockPoint.X, blockPoint.Y, blockPoint.Z, value);
	            }
	        }
		}
		
		bin.Dispose();
		fileStream.Dispose();
		return totalBlocks;
	}
}