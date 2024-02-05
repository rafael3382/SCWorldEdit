using System;
using System.Collections.Generic;
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
    public class WEUserManager : ModLoader
    {
        private static Dictionary<ComponentPlayer,WEUser> m_usersByPlayer = new Dictionary<ComponentPlayer,WEUser>();
        
        public static WEUser GetUserByComponent(ComponentPlayer player)
        {
            return m_usersByPlayer[player];
        }
        
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("OnEntityAdd", this);
            ModsManager.RegisterHook("SubsystemUpdate", this);
            ModsManager.RegisterHook("GuiDraw", this);
            ModsManager.RegisterHook("ToFreeChunks", this);
            Log.Information("WorldEdit loaded.");
            Storage.CreateDirectory(ZoneDialog.Path_mod);
        }
        
        public override void OnEntityAdd(Entity entity)
		{
		    ComponentPlayer componentPlayer = entity.FindComponent<ComponentPlayer>(false);
		    if (componentPlayer == null)
		       return;
		    try
		    {
			    m_usersByPlayer[componentPlayer] = new WEUser();
			    m_usersByPlayer[componentPlayer].Initialize(componentPlayer);
			    Log.Information("Added WE user.");
			}
			catch (Exception ex)
			{
			    Log.Error(ex);
			}
			//return false;
		}
		
		public override void SubsystemUpdate(float dt)
		{
		    foreach (WEUser user in m_usersByPlayer.Values)
                user.Update();
		}

		public override void GuiDraw(ComponentGui componentGui, Camera camera, int drawOrder)
		{
		    if (!m_usersByPlayer.Keys.Contains(componentGui.m_componentPlayer))
		        return;
		    m_usersByPlayer[componentGui.m_componentPlayer].Draw(camera);
		}
		
		public override void ToFreeChunks(TerrainUpdater terrainUpdater, TerrainChunk chunk, out bool KeepWorking)
		{
			KeepWorking = false;
			try
		    {
			if (!WEOperationManager.PersistentChunks.ContainsKey(chunk.Coords))
			    return;
            if (true) //WEOperationManager.PersistentChunks[chunk.Coords] > Time.RealTime - 1.1)
			{
			    KeepWorking = true;
			}
			else
			{
			    Log.Information(chunk.Coords + " persistent chunk timeouted and therefore disposed. RIP");
			    WEOperationManager.PersistentChunks.Remove(chunk.Coords);
			    terrainUpdater.m_subsystemTerrain.TerrainSerializer.SaveChunk(chunk);
			}
			} catch (Exception ex) { Log.Error(ex); }
		}
    }
}