using Game;
using GameEntitySystem;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

public class CircuitStepDebugger : ModLoader, IDrawable
{
    public PrimitivesRenderer3D pr3d;
    public FlatBatch3D batch;
    
    public SubsystemElectricity m_subsystemElectricity;
    public SubsystemTerrain m_subsystemTerrain;
    
    public int[] DrawOrders => new int[1] { 105 };
    
    public override void __ModInitialize()
    {
        ModsManager.RegisterHook("OnProjectLoaded", this);
        pr3d = new PrimitivesRenderer3D();
        batch = pr3d.FlatBatch();
    }
    
    public static float StepDuration { get; set; } // 0.4, 0.7, 1.2
    
    public static void ControlledUpdating()
    {
        SubsystemUpdate update = GameManager.Project.FindSubsystem<SubsystemUpdate>();
        SubsystemElectricity subsystemElectricity = GameManager.Project.FindSubsystem<SubsystemElectricity>();
        update.RemoveUpdateable(subsystemElectricity);
    }
    
    public static void NormalUpdating()
    {
        SubsystemUpdate update = GameManager.Project.FindSubsystem<SubsystemUpdate>();
        SubsystemElectricity subsystemElectricity = GameManager.Project.FindSubsystem<SubsystemElectricity>();
        update.AddUpdateable(subsystemElectricity);
    }
    
    public override void OnProjectLoaded(Project project)
    {
        m_subsystemElectricity = project.FindSubsystem<SubsystemElectricity>();
        m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>();
        SubsystemDrawing drawing = project.FindSubsystem<SubsystemDrawing>();
        drawing.AddDrawable(this);
        batch.Clear();
        StepDuration = 0f;
    }
    
    public void Draw(Camera camera, int drawOrder)
    {
        if (StepDuration == 0f)
            return;
        if (Time.PeriodicEvent((double) StepDuration, 0.0))
        {
		    // Update the drawing of the next step
		    batch.Clear();
            foreach (KeyValuePair<int, Dictionary<ElectricElement, bool>> elementsInStep in m_subsystemElectricity.m_futureSimulateLists)
            {
                foreach (KeyValuePair<ElectricElement, bool> element in elementsInStep.Value)
                {
                    if (!element.Value)
                        return;
                    Color col = Color.Blue;
                    if (elementsInStep.Key == m_subsystemElectricity.CircuitStep + 1)
                      col = Color.Green;
                    foreach (CellFace cell in element.Key.CellFaces)
                    {
                        if (BlocksManager.Blocks[m_subsystemTerrain.Terrain.GetCellContentsFast(cell.X, cell.Y, cell.Z)] is WireThroughBlock)
                        {
                            batch.QueueQuad(new Vector3(cell.X, cell.Y, cell.Z + 1.01f), new Vector3(cell.X + 1, cell.Y, cell.Z + 1.01f), new Vector3(cell.X + 1, cell.Y + 1, cell.Z + 1.01f), new Vector3(cell.X, cell.Y + 1, cell.Z + 1.01f), col * 0.35f);
                            batch.QueueQuad(new Vector3(cell.X + 1.01f, cell.Y, cell.Z), new Vector3(cell.X + 1.01f, cell.Y + 1, cell.Z), new Vector3(cell.X + 1.01f, cell.Y + 1, cell.Z + 1), new Vector3(cell.X + 1.01f, cell.Y, cell.Z + 1), col * 0.35f);
                            batch.QueueQuad(new Vector3(cell.X, cell.Y, cell.Z -0.01f), new Vector3(cell.X + 1, cell.Y, cell.Z -0.01f), new Vector3(cell.X + 1, cell.Y + 1, cell.Z -0.01f), new Vector3(cell.X, cell.Y + 1, cell.Z -0.01f), col * 0.35f);
                            batch.QueueQuad(new Vector3(cell.X -0.01f, cell.Y, cell.Z), new Vector3(cell.X -0.01f, cell.Y + 1, cell.Z), new Vector3(cell.X -0.01f, cell.Y + 1, cell.Z + 1), new Vector3(cell.X-0.01f, cell.Y, cell.Z + 1), col * 0.35f);
                            batch.QueueQuad(new Vector3(cell.X, cell.Y + 1.01f, cell.Z), new Vector3(cell.X + 1, cell.Y + 1.01f, cell.Z), new Vector3(cell.X + 1, cell.Y + 1.01f, cell.Z + 1), new Vector3(cell.X, cell.Y + 1.01f, cell.Z + 1), col * 0.35f);
                            batch.QueueQuad(new Vector3(cell.X, cell.Y -0.01f, cell.Z), new Vector3(cell.X + 1, cell.Y -0.01f, cell.Z), new Vector3(cell.X + 1, cell.Y -0.01f, cell.Z + 1), new Vector3(cell.X, cell.Y -0.01f, cell.Z + 1), col * 0.35f);
                            continue;
                        }
                        switch (cell.Face)
                        {
                            case 0:
                                batch.QueueQuad(new Vector3(cell.X, cell.Y, cell.Z + 0.1f), new Vector3(cell.X + 1, cell.Y, cell.Z + 0.1f), new Vector3(cell.X + 1, cell.Y + 1, cell.Z + 0.1f), new Vector3(cell.X, cell.Y + 1, cell.Z + 0.1f), col * 0.35f);
                                break;
                        
                            case 1:
                                batch.QueueQuad(new Vector3(cell.X + 0.1f, cell.Y, cell.Z), new Vector3(cell.X + 0.1f, cell.Y + 1, cell.Z), new Vector3(cell.X + 0.1f, cell.Y + 1, cell.Z + 1), new Vector3(cell.X + 0.1f, cell.Y, cell.Z + 1), col * 0.35f);
                                break;
                        
                            case 2:
                                batch.QueueQuad(new Vector3(cell.X, cell.Y, cell.Z + 0.9f), new Vector3(cell.X + 1, cell.Y, cell.Z + 0.9f), new Vector3(cell.X + 1, cell.Y + 1, cell.Z + 0.9f), new Vector3(cell.X, cell.Y + 1, cell.Z + 0.9f), col * 0.35f);
                                break;
                        
                            case 3:
                                batch.QueueQuad(new Vector3(cell.X + 0.9f, cell.Y, cell.Z), new Vector3(cell.X + 0.9f, cell.Y + 1, cell.Z), new Vector3(cell.X + 0.9f, cell.Y + 1, cell.Z + 1), new Vector3(cell.X+0.9f, cell.Y, cell.Z + 1), col * 0.35f);
                                break;
                        
                            case 4:
                                batch.QueueQuad(new Vector3(cell.X, cell.Y + 0.1f, cell.Z), new Vector3(cell.X + 1, cell.Y + 0.1f, cell.Z), new Vector3(cell.X + 1, cell.Y + 0.1f, cell.Z + 1), new Vector3(cell.X, cell.Y + 0.1f, cell.Z + 1), col * 0.35f);
                                break;
                        
                            case 5:
                                batch.QueueQuad(new Vector3(cell.X, cell.Y + 0.9f, cell.Z), new Vector3(cell.X + 1, cell.Y + 0.9f, cell.Z), new Vector3(cell.X + 1, cell.Y + 0.9f, cell.Z + 1), new Vector3(cell.X, cell.Y + 0.9f, cell.Z + 1), col * 0.35f);
                                break;
                            
                            default:
                                batch.QueueQuad(new Vector3(cell.X, cell.Y + 1.1f, cell.Z), new Vector3(cell.X + 1, cell.Y + 1.1f, cell.Z), new Vector3(cell.X + 1, cell.Y + 1.1f, cell.Z + 1), new Vector3(cell.X, cell.Y + 1.1f, cell.Z + 1), col * 0.35f);
                                break;
                        }
                    }
                }
            }
            
            // Update the circuit to the next step
            m_subsystemElectricity.UpdateElectricElements();
		    m_subsystemElectricity.CircuitStep++;
		    m_subsystemElectricity.m_nextStepSimulateList = null;
		    if (m_subsystemElectricity.m_futureSimulateLists.TryGetValue(m_subsystemElectricity.CircuitStep, out var value))
		    {
		        m_subsystemElectricity.m_futureSimulateLists.Remove(m_subsystemElectricity.CircuitStep);
		        SubsystemElectricity.SimulatedElectricElements += value.Count;
		        foreach (ElectricElement key in value.Keys)
		        {
			        if (m_subsystemElectricity.m_electricElements.ContainsKey(key))
			        {
				        m_subsystemElectricity.SimulateElectricElement(key);
			        }
		        }
		        m_subsystemElectricity.ReturnListToCache(value);
		    }
        }
        pr3d.Flush(camera.ViewProjectionMatrix, clearAfterFlush: false);
    }
}