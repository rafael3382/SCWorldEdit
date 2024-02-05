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
    public class ExtrasOperator
    {
        public bool PlayerCollisionEnabled = true;
        public bool TimeStopped = false;
        public bool SlowCircuitStepMode = false;
        public bool BlockLightingEnabled = true;
        public bool BlockBehaviorEnabled = true;
        public bool DrawChunkBorders = false;
        
        public WEUser User;
        public ComponentPlayer player;
        public SubsystemTerrain m_subsystemTerrain;
        public Project project => m_subsystemTerrain.Project;
       
       public ExtrasOperator(WEUser user)
       {
           User = user;
           player = user.m_componentPlayer;
           m_subsystemTerrain = user.m_subsystemTerrain;
       }
       
       public void UpdatePlayerCollision()
       {
           player.ComponentBody.TerrainCollidable = PlayerCollisionEnabled;
           player.ComponentLocomotion.IsCreativeFlyEnabled = true;
       }
       
       public void UpdateBlockLighting()
       {
           if (!BlockLightingEnabled)
           {
               for (int i = 0; i < 15; i++)
			    {
				    LightingManager.LightIntensityByLightValue[i] = LightingManager.LightIntensityByLightValue[15];
			    }
			    for (int j = 0; j < 6; j++)
	            {
				    for (int k = 0; k < 15; k++)
				    {
					    LightingManager.LightIntensityByLightValueAndFace[k + j * 16] = LightingManager.LightIntensityByLightValueAndFace[15 + j * 16];
				    }
			    }
	       }
           else LightingManager.CalculateLightingTables();
           
           m_subsystemTerrain.TerrainUpdater.DowngradeAllChunksState(TerrainChunkState.InvalidLight, forceGeometryRegeneration: true);
       }
       
       public void UpdateBlockBehaviorEnabled()
       {
           SubsystemBlockBehaviors subsystemBlockBehaviors = project.FindSubsystem<SubsystemBlockBehaviors>();
           if (!BlockBehaviorEnabled)
           {
               // Clear all block behaviors
               subsystemBlockBehaviors.m_blockBehaviors.Clear();
               for (int i=0; i<subsystemBlockBehaviors.m_blockBehaviorsByContents.Length; i++)
               {
                   subsystemBlockBehaviors.m_blockBehaviorsByContents[i] = new SubsystemBlockBehavior[0];
               }
           }
           else
           {
               // Full reload
               subsystemBlockBehaviors.Load(subsystemBlockBehaviors.ValuesDictionary);
           }
       }
       
       public bool lastTimeStopped = false;
       
       public Dictionary<IUpdateable, SubsystemUpdate.UpdateableInfo> oldUpdateables = new Dictionary<IUpdateable, SubsystemUpdate.UpdateableInfo>();
       public List<IUpdateable> oldSortedUpdateables = new List<IUpdateable>();
       
       public void UpdateTimeStop()
       {
           SubsystemUpdate updates = project.FindSubsystem<SubsystemUpdate>();
           if (TimeStopped)
           {
               foreach (IUpdateable updateable in oldUpdateables.Keys)
               {
                   try
                   {
                       if (updateable is SubsystemGameWidgets ||
                            updateable is SubsystemSignBlockBehavior ||
                           (updateable is Component component && component.Entity.FindComponent<ComponentPlayer>(false) != null))
                       {
                           updateable.Update(Time.FrameDuration);
                       }
                   }
                   catch
                   {
                   }
               }
           }
           
           
           if (TimeStopped == lastTimeStopped)
           {
               return;
           }
           if (TimeStopped)
           {
               oldUpdateables = updates.m_updateables;
               updates.m_updateables = new Dictionary<IUpdateable, SubsystemUpdate.UpdateableInfo>();
               oldSortedUpdateables = updates.m_sortedUpdateables;
               updates.m_sortedUpdateables = new List<IUpdateable>();
               
           }
           else
           {
               updates.m_updateables = oldUpdateables;
               updates.m_sortedUpdateables = oldSortedUpdateables;
           }
           lastTimeStopped = TimeStopped;
       }
       
       public void ClearAnimals()
       {
           int entityCount = 0;
           int offset = 0;
           List<Entity> entities = project.Entities.ToList();
           foreach (Entity entity in entities)
           {
               if (entity.FindComponent<ComponentBlockEntity>(false) != null)
                   continue;
               if (entity.FindComponent<ComponentPlayer>(false) != null)
                   continue;
               project.RemoveEntity(entity, true);
               entityCount++;
           }
           try
           {
               SubsystemSpawn spawn = project.FindSubsystem<SubsystemSpawn>(true);
               entityCount += spawn.m_spawns.Count;
               spawn.m_spawns.Clear();
               spawn.m_chunks.Clear();
           }
           catch
           {
           }
           User.ShowMessage(entityCount + " animals removed.");
       }
       
       public void ClearDrops()
       {
           SubsystemPickables pickables = m_subsystemTerrain.Project.FindSubsystem<SubsystemPickables>();
           int dropsCount = pickables.m_pickables.Count;
           pickables.m_pickables.Clear();
           User.ShowMessage(dropsCount + " drops removed.");
       }
       
       public void Draw(Camera camera)
       {
           if (DrawChunkBorders)
           {
               PrimitivesRenderer3D chunkBorderRender = new PrimitivesRenderer3D();
               FlatBatch3D chunkBorderBatch = chunkBorderRender.FlatBatch(0, DepthStencilState.Default, RasterizerState.CullNone, BlendState.Opaque);
               foreach (TerrainChunk chunk in m_subsystemTerrain.Terrain.AllocatedChunks)
               {
                   Color color = Color.Purple;
                   if (chunk.BoundingBox.Contains(player.ComponentBody.Position))
                       color = Color.Blue;
                   if (chunk.State < TerrainChunkState.Valid)
                       color = Color.Red;
                   chunkBorderBatch.QueueBoundingBox(chunk.BoundingBox, color);
               }
               chunkBorderRender.Flush(camera.ViewProjectionMatrix);
           }
       }
       
       public void ToggleCircuitStepDebugger()
	   {
		    if (!SlowCircuitStepMode)
		    {
		        List<float> speeds = new List<float>() { 0.08f, 0.4f, 0.7f, 1.2f }; // Less Slow, Slow, Very Slow
		        DialogsManager.ShowDialog(User.m_componentPlayer.GuiWidget, new ListSelectionDialog("Select Circuit Speed", speeds, 56f, (object speed) => new LabelWidget
		        {
			        Text = GetSpeedName((float) speed),
			        HorizontalAlignment = WidgetAlignment.Center,
			        VerticalAlignment = WidgetAlignment.Center
		        }, delegate(object speed)
		        {
			        CircuitStepDebugger.StepDuration = (float) speed;
			        CircuitStepDebugger.ControlledUpdating();
			        User.ShowMessage("Circuit Step Debugger : ON");
			        SlowCircuitStepMode = true;
		        }));
		    }
		    else
		    {
		        CircuitStepDebugger.StepDuration = 0f;
		        CircuitStepDebugger.NormalUpdating();
		        User.ShowMessage("Circuit Step Debugger : Off");
		        SlowCircuitStepMode = false;
		    }
	    }
	    
	    public string GetSpeedName(float speed)
	    {
	        if (speed == 0.08f) return "Fast - " + speed+"s";
	        if (speed == 0.4f) return "Less Slow - " + speed+"s";
	        if (speed == 0.7f) return "Slow - " + speed+"s";
	        if (speed == 1.2f) return "Very Slow - " + speed+"s";
	        return speed.ToString();
	    }
       
       public void SaveWorldMap()
       {
           /*
           List<Point2> modifiedChunks = new List<Point2>();
           Point2 leftmostTop = null;
           Point2 rightmostBottom = null;
           string regionsPath = Storage.CombinePaths(m_subsystemTerrain.SubsystemGameInfo.DirectoryName, "Regions");
           bool noModifiedChunks = false;
           if (Storage.DirectoryExists(regionsPath))
           {
               foreach (string regionName in Storage.ListFileNames(RegionsDirectoryName))
			   {
			        if (!regionName.StartsWith("Region ") || !regionName.EndsWith(".dat"))
			            continue;
			        using (Stream stream= Storage.OpenFile())
			        {
		                using (BinaryReader reader = new BinaryReader(regionStream, Encoding.UTF8, leaveOpen: true))
					    {
						    int index = 0;
					    	reader.BaseStream.Position = 4L;
					    }
				    }
    		   }
           }*/
       }
       
       //public static void StopTime(
    }
}