using Engine;
using Game;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using API_WE_Mod;

public static class WEOperationManager
{
    public static void ReportIfNull(this object obj, string name)
    {
        if (obj == null)
            Log.Error(name + " is null.");
    }
    public static SubsystemTerrain m_subsystemTerrain;
    
    public static Dictionary<Point2,double> PersistentChunks = new Dictionary<Point2,double>();
    
    public static List<Task> RunningOperations = new List<Task>();
    
    public static void MemCopy(TerrainRaycastResult point1, TerrainRaycastResult point2, WEUser user)
    {
        int startX = MathUtils.Min(point1.CellFace.X, point2.CellFace.X);
        int endX = MathUtils.Max(point1.CellFace.X, point2.CellFace.X);
        int startY = MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y);
        int endY = MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y);
        int startZ = MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z);
        int endZ = MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z);
        
        try
        {
            user.BlockMemory = new BlockGrid(endX - startX + 1, endY - startY + 1, endZ - startZ + 1);
            user.BlockMemory.Sign = new Point3((point1.CellFace.X > point2.CellFace.X) ? -1 : 1, (point1.CellFace.Y > point2.CellFace.Y) ? -1 : 1, (point1.CellFace.Z > point2.CellFace.Z) ? -1 : 1);
            user.ShowMessage(((float) user.BlockMemory.BlockCount * sizeof(int) / 1024f / 1024f).ToString("0.0") + "MB allocated. Copying...");
            for (int x = 0; x <= endX - startX; x++)
            {
                for (int y = 0; y <= endY - startY; y++)
                {
                    for (int z = 0; z <= endZ - startZ; z++)
                    {
                        int X, Y, Z;
                        int relX, relY, relZ;
                        if (point1.CellFace.X > point2.CellFace.X)
                        {
                            relX = -x;
                            X = point1.CellFace.X - x;
                        }
                        else
                        {
                            relX = x;
                            X = point1.CellFace.X + x;
                        }
    
                        if (point1.CellFace.Y > point2.CellFace.Y)
                        {
                            relY = -y;
                            Y = point1.CellFace.Y - y;
                        }
                        else
                        {
                            relY = y;
                            Y = point1.CellFace.Y + y;
                        }
    
                        if (point1.CellFace.Z > point2.CellFace.Z)
                        {
                            relZ = - z;
                            Z = point1.CellFace.Z - z;
                        }
                        else
                        {
                            relZ = z;
                            Z = point1.CellFace.Z + z;
                        }
                        
                        user.BlockMemory.SetBlock(relX, relY, relZ, m_subsystemTerrain.Terrain.GetCellValue(X, Y, Z));
                    }
                }
            }
            user.CopySpecialData();
            user.ShowMessage("Copied " + user.BlockMemory.BlockCount + " blocks");
        }
        catch (System.Exception ex)
        {
            Log.Error(ex);
            user.ShowMessage("Error when copying");
        }
        
    }
    
    public static void MemPaste(TerrainRaycastResult point1, TerrainRaycastResult point2, TerrainRaycastResult point3, WEUser user)
    {
        if (user.BlockMemory == null)
        {
            user.ShowMessage("There's nothing currently in memory.");
            return;
        }
        user.BlockMemory.Paste(new Point3(point3.CellFace.X, point3.CellFace.Y, point3.CellFace.Z));
        user.PasteSpecialData();
        user.ShowMessage("Pasted " + user.BlockMemory.BlockCount + " blocks");
    }
    
    public static void Transfer(TerrainRaycastResult point1, TerrainRaycastResult point2, TerrainRaycastResult point3, WEUser user)
    {
        int startX = MathUtils.Min(point1.CellFace.X, point2.CellFace.X);
        int endX = MathUtils.Max(point1.CellFace.X, point2.CellFace.X);
        int startY = MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y);
        int endY = MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y);
        int startZ = MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z);
        int endZ = MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z);
        
        for (int y = 0; y <= endY - startY; y++)
        {
            for (int z = 0; z <= endZ - startZ; z++)
            {
                for (int x = 0; x <= endX - startX; x++)
                {
                    int targetX, targetY, targetZ;
                    int PlaceX, PlaceY, PlaceZ;
                    if (point1.CellFace.X > point2.CellFace.X)
                    {
                        targetX = point1.CellFace.X - x;
                        PlaceX = point3.CellFace.X - x;
                    }
                    else
                    {
                        targetX = point1.CellFace.X + x;
                        PlaceX = point3.CellFace.X + x;
                    }

                    if (point1.CellFace.Y > point2.CellFace.Y)
                    {
                        targetY = point1.CellFace.Y - y;
                        PlaceY = point3.CellFace.Y - y;
                    }
                    else
                    {
                        targetY = point1.CellFace.Y + y;
                        PlaceY = point3.CellFace.Y + y;
                    }
                    
                    if (point1.CellFace.Z > point2.CellFace.Z)
                    {
                        targetZ = point1.CellFace.Z - z;
                        PlaceZ = point3.CellFace.Z - z;
                    }
                    else
                    {
                        targetZ = point1.CellFace.Z + z;
                        PlaceZ = point3.CellFace.Z + z;
                    }
                    
                    int block = m_subsystemTerrain.Terrain.GetCellValue(targetX, targetY, targetZ);
                    if (block != 0)
                        SetCell(PlaceX, PlaceY, PlaceZ, block);
                }
            }
        }
    }
    
    public static void RestoreTerrain(TerrainRaycastResult point1, TerrainRaycastResult point2, WEUser user)
    {
        int startX = MathUtils.Min(point1.CellFace.X, point2.CellFace.X);
        int endX = MathUtils.Max(point1.CellFace.X, point2.CellFace.X);
        int startY = MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y);
        int endY = MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y);
        int startZ = MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z);
        int endZ = MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z);
        
        SubsystemTerrain defaultTerrain = new SubsystemTerrain();
        defaultTerrain.Initialize(GameManager.Project, m_subsystemTerrain.ValuesDictionary);
        defaultTerrain.Terrain = new Terrain();
        defaultTerrain.SubsystemGameInfo = m_subsystemTerrain.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
        defaultTerrain.TerrainUpdater = new TerrainUpdater(defaultTerrain);
        Type terrainGenType = m_subsystemTerrain.TerrainContentsGenerator.GetType();
        ConstructorInfo expectedConstructor = terrainGenType.GetConstructor(new Type[] { typeof(SubsystemTerrain) });
        if (expectedConstructor != null)
        {
            defaultTerrain.TerrainContentsGenerator = (ITerrainContentsGenerator) Activator.CreateInstance(terrainGenType, new object[] { defaultTerrain });
        }
        else
        {
            user.ShowMessage("The current terrain generator does not have the expected constructor.");
            return;
        }
        TerrainChunk previousChunk = null;
        for (int x = 0; x <= endX - startX; x++)
        {
            for (int z = 0; z <= endZ - startZ; z++)
            {
                for (int y = 0; y <= endY - startY; y++)
                {
                    int targetX, targetY, targetZ;
                    if (point1.CellFace.X > point2.CellFace.X)
                    {
                        targetX = point1.CellFace.X - x;
                    }
                    else
                    {
                        targetX = point1.CellFace.X + x;
                    }

                    if (point1.CellFace.Y > point2.CellFace.Y)
                    {
                        targetY = point1.CellFace.Y - y;
                    }
                    else
                    {
                        targetY = point1.CellFace.Y + y;
                    }
                    
                    if (point1.CellFace.Z > point2.CellFace.Z)
                    {
                        targetZ = point1.CellFace.Z - z;
                    }
                    else
                    {
                        targetZ = point1.CellFace.Z + z;
                    }
                    
                    if (defaultTerrain.Terrain.GetChunkAtCell(targetX, targetZ) == null)
                    {
                        TerrainChunk currentChunk = defaultTerrain.Terrain.AllocateChunk(targetX >> 4, targetZ >> 4);
                        if (previousChunk != null)
                            defaultTerrain.Terrain.FreeChunk(previousChunk);
                        
                        currentChunk.ThreadState = TerrainChunkState.InvalidContents1;
                        while (currentChunk.ThreadState < TerrainChunkState.InvalidLight)
                        {
                            defaultTerrain.TerrainUpdater.UpdateChunkSingleStep(currentChunk, 15);
                        }
                        previousChunk = currentChunk;
                    }
                    
                    int block = defaultTerrain.Terrain.GetCellValue(targetX, targetY, targetZ);
                    SetCell(targetX, targetY, targetZ, block, behaviors: false);
                }
            }
        }
    }
    
    public static void Fill(TerrainRaycastResult point1, TerrainRaycastResult point2, WEUser user)
    {
        int startX = MathUtils.Min(point1.CellFace.X, point2.CellFace.X);
        int endX = MathUtils.Max(point1.CellFace.X, point2.CellFace.X);
        int startY = MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y);
        int endY = MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y);
        int startZ = MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z);
        int endZ = MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    SetCell(x, y, z, user.SelectedBlock);
                    if (user.SelectedSpecialData.Data != null)
                        user.SelectedSpecialData.ToPosition(new Point3(x, y, z));
                }
            }
        }
        user.ShowMessage($"Filled {endX - startX + 1}x{endY - startY + 1}x{endZ - startZ + 1} area\nwith {API_WE.GetDisplayName(user.SelectedBlock)} (from set point 1)");
    }
    
    public static void Replace(TerrainRaycastResult point1, TerrainRaycastResult point2, WEUser user)
    {
        int startX = MathUtils.Min(point1.CellFace.X, point2.CellFace.X);
        int endX = MathUtils.Max(point1.CellFace.X, point2.CellFace.X);
        int startY = MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y);
        int endY = MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y);
        int startZ = MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z);
        int endZ = MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    if (m_subsystemTerrain.Terrain.GetCellValue(x, y, z) == user.ReplaceableBlock)
                    {
                        SetCell(x, y, z, user.SelectedBlock);
                        if (user.SelectedSpecialData.Data != null)
                            user.SelectedSpecialData.ToPosition(new Point3(x, y, z));
                    }
                }
            }
        }
        user.ShowMessage($"Replaced all {API_WE.GetDisplayName(user.ReplaceableBlock).ToLower()} (from set point 2) with {API_WE.GetDisplayName(user.SelectedBlock).ToLower()} (from set point 1)\nIn a {endX - startX + 1}x{endY - startY + 1}x{endZ - startZ + 1} area");
    }
    
    public static void Clear(TerrainRaycastResult point1, TerrainRaycastResult point2, WEUser user)
    {
        int startX = MathUtils.Min(point1.CellFace.X, point2.CellFace.X);
        int endX = MathUtils.Max(point1.CellFace.X, point2.CellFace.X);
        int startY = MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y);
        int endY = MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y);
        int startZ = MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z);
        int endZ = MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z);
        
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    SetCell(x, y, z, 0);
                }
            }
        }

        user.ShowMessage($"Cleared {endX - startX + 1}x{endY - startY + 1}x{endZ - startZ+1} area");
    }
    
    public static void FillMemoryBankData(TerrainRaycastResult point1, TerrainRaycastResult point2, WEUser user)
    {
        Point3 start = new Point3(MathUtils.Min(point1.CellFace.X, point2.CellFace.X), MathUtils.Min(point1.CellFace.Y, point2.CellFace.Y), MathUtils.Min(point1.CellFace.Z, point2.CellFace.Z));
        Point3 end = new Point3(MathUtils.Max(point1.CellFace.X, point2.CellFace.X), MathUtils.Max(point1.CellFace.Y, point2.CellFace.Y), MathUtils.Max(point1.CellFace.Z, point2.CellFace.Z));
        
        List<Point3> memCoords = new List<Point3>(); 
        
        for (int x=start.X; x<=end.X; x++)
        {
            for (int y=start.Y; y<=end.Y; y++)
            {
                for (int z=start.Z; z<=end.Z; z++)
                {
                    if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(x, y, z)) == MemoryBankBlock.Index)
                      memCoords.Add(new Point3(x,y,z));
                }
            }
        }
        
        if (memCoords.Count == 0)
        {
           user.ShowMessage("There are no memory banks in the selection.");
           return;
        }
        
       SubsystemMemoryBankBlockBehavior memoryBankBehavior = GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>();
       MemoryBankData memoryBankData = new MemoryBankData();
       if (SettingsManager.UsePrimaryMemoryBank)
       {
           DialogsManager.ShowDialog(user.m_componentPlayer.GuiWidget, new EditMemoryBankDialog(memoryBankData, delegate
           {
               foreach (Point3 position in memCoords)
               {
                   memoryBankBehavior.SetBlockData(position, memoryBankData);
               }
           }));
       }
       else
       {
           DialogsManager.ShowDialog(user.m_componentPlayer.GuiWidget, new EditMemoryBankDialogAPI(memoryBankData, delegate
           {
               foreach (Point3 position in memCoords)
               {
                   memoryBankBehavior.SetBlockData(position, memoryBankData);
               }
           }));
       }
       
       user.ShowMessage(memCoords.Count + " memory banks have been programmed.");
    }
    
    public static void StartOperation(string operationName, WEUser user)
    {
       MethodInfo operationMethod = typeof(WEOperationManager).GetMethod(operationName);
       if (operationMethod == null)
       {
           Log.Error(operationName + " operation not found.");
           return;
       }
       ParameterInfo[] parameterInfos = operationMethod.GetParameters();
       object[] parameters = new object[parameterInfos.Length];
       for (int paramIndex=0; paramIndex<parameterInfos.Length; paramIndex++)
       {
           if (parameterInfos[paramIndex].ParameterType == typeof(TerrainRaycastResult))
           {
               switch (parameterInfos[paramIndex].Name)
               {
                   case "point1":
                       if (!user.Point1Set)
                       {
                            user.ShowMessage("You have not selected point 1");
                            return;
                       }
                       else parameters[paramIndex] = user.Point1;
                       break;
                   case "point2":
                       if (!user.Point2Set)
                       {
                            user.ShowMessage("You have not selected point 2");
                            return;
                       }
                       else parameters[paramIndex] = user.Point2;
                       break;
                   case "point3":
                       if (!user.Point3Set)
                       {
                            user.ShowMessage("You have not selected point 3");
                            return;
                       }
                       else parameters[paramIndex] = user.Point3;
                       break;
               }
           }
           else if (parameterInfos[paramIndex].ParameterType == typeof(WEUser))
           {
               parameters[paramIndex] = user;
           }
           else
           {
               Log.Error("Couldn't figure out the parameter " + parameterInfos[paramIndex].Name);
               return;
           }
       }
       //operationMethod.Invoke(null, parameters);
       RunningOperations.Add(Task.Run(delegate { try { operationMethod.Invoke(null, parameters); /*user.ShowMessage(operationName + " operation has been completed.");*/ } catch (System.Exception ex) { Log.Error(ex); user.ShowMessage("Error while performing action ("+operationName+")"); } } ));
    }
    
    public static void SetCell(int x, int y, int z, int value, bool behaviors=true)
    {
        if (y > 255)
            return;
        
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
		if (chunkAtCell == null || chunkAtCell.ThreadState < TerrainChunkState.InvalidLight)
		{
		    if (chunkAtCell == null)
		    {
		        chunkAtCell = m_subsystemTerrain.Terrain.AllocateChunk(x >> 4, z >> 4);
		    }
		    while (chunkAtCell.ThreadState < TerrainChunkState.InvalidLight)
		    {
		        m_subsystemTerrain.TerrainUpdater.UpdateChunkSingleStep(chunkAtCell, m_subsystemTerrain.TerrainUpdater.m_subsystemSky.SkyLightValue);
		    }
	    	Log.Information(chunkAtCell.Coords + " ready to use.");
		}
		
		try
		{
		    PersistentChunks[chunkAtCell.Coords] = Time.RealTime;
		}
		catch
		{
		}
		
        bool pass = false;
		ModsManager.HookAction("TerrainChangeCell", delegate(ModLoader loader)
		{
			loader.TerrainChangeCell(m_subsystemTerrain, x, y, z, value, out var Skip);
			pass |= Skip;
			return false;
		});
		if (pass)
		{
			return;
		}
		
		
		int relX = x & 0xF;
		int relZ = z & 0xF;
		int cellValueFast = chunkAtCell.GetCellValueFast(relX, y, relZ);
		value = Terrain.ReplaceLight(value, 0);
		cellValueFast = Terrain.ReplaceLight(cellValueFast, 0);
		if (value == cellValueFast)
		{
			return;
		}
		
		chunkAtCell.SetCellValueFast(relX, y, relZ, value);
        chunkAtCell.ModificationCounter++;
        m_subsystemTerrain.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, forceGeometryRegeneration: true);
        if (!behaviors)
          return;
		int num = Terrain.ExtractContents(cellValueFast);
		try
		{
		    if (Terrain.ExtractContents(value) != num)
		    {
			    SubsystemBlockBehavior[] blockBehaviors = m_subsystemTerrain.m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(cellValueFast));
			    for (int i = 0; i < blockBehaviors.Length; i++)
			    {
				    blockBehaviors[i].OnBlockRemoved(cellValueFast, value, x, y, z);
			    }
			    SubsystemBlockBehavior[] blockBehaviors2 = m_subsystemTerrain.m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(value));
			    for (int j = 0; j < blockBehaviors2.Length; j++)
			    {
				    blockBehaviors2[j].OnBlockAdded(value, cellValueFast, x, y, z);
			    }
		    }
		    else
		    {
			    SubsystemBlockBehavior[] blockBehaviors3 = m_subsystemTerrain.m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(value));
			    for (int k = 0; k < blockBehaviors3.Length; k++)
			    {
				    blockBehaviors3[k].OnBlockModified(value, cellValueFast, x, y, z);
			    }
		    }
		    m_subsystemTerrain.m_modifiedCells[new Point3(x, y, z)] = true;
		}
		catch
		{
		}
		
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(x, z) == null)
		    Log.Warning("It is fucked up.");
    }
    
    public static int GetCell(int x, int y, int z)
    {
        return m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
    }
}