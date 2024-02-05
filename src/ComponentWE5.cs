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
    public class WEUser
    {
        public class SpecialData
        {
            public object Data;
            public Type Type => Data?.GetType();
            
            public static SpecialData FromPosition(Point3 position)
            {
                var membhv = GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>();
                var trbhv = GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>();
                var signbhv = GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>();
                var blockentities = GameManager.Project.FindSubsystem<SubsystemBlockEntities>();
                
                SpecialData data = new SpecialData();
                if (membhv.m_blocksData.ContainsKey(position))
                    data.Data = membhv.m_blocksData[position];
                else if (trbhv.m_blocksData.ContainsKey(position))
                    data.Data = trbhv.m_blocksData[position];
                else if (signbhv.m_textsByPoint.ContainsKey(position))
                    data.Data = signbhv.m_textsByPoint[position];
                else if (blockentities.m_blockEntities.ContainsKey(position))
                    data.Data = blockentities.m_blockEntities[position];
                
                return data;
            }
            public void ToPosition(Point3 position)
            {
                var membhv = GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>();
                var trbhv = GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>();
                var signbhv = GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>();
                var blockentities = GameManager.Project.FindSubsystem<SubsystemBlockEntities>();
                
                switch (this.Type.Name)
                {
                    case "MemoryBankData":
                        MemoryBankData memData = Data as MemoryBankData;
                        membhv.SetBlockData(position, memData);
                        break;
                    case "TruthTableData":
                        TruthTableData trData = Data as TruthTableData;
                        trbhv.SetBlockData(position, trData);
                        break;
                    case "SignData":
                        SignData signData = Data as SignData;
                        signbhv.SetSignData(position, signData.Lines, signData.Colors, signData.Url);
                        break;
                    case "ComponentInventoryBase":
                        ComponentInventoryBase inventoryData= Data as ComponentInventoryBase;
                        if (blockentities.m_blockEntities.TryGetValue(position, out ComponentBlockEntity target))
                        {
                            ComponentInventoryBase inv = target.Entity.FindComponent<ComponentInventoryBase>(false);
                            if (inv != null && inv.SlotsCount == inventoryData.SlotsCount)
                               inv.m_slots = inventoryData.m_slots;
                        }
                        break;
                }
            }
        }
        
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemSky m_subsystemSky;
        public ComponentPlayer m_componentPlayer = null;
        public GameWidget m_gameWidget = null;
        
        public List<string> names = new List<string>();
        public List<string> extras = new List<string>();

        public List<Category> m_categories = new List<Category>();
        
        public BitmapButtonWidget F1;
        public BitmapButtonWidget F2;
        public BitmapButtonWidget F3;
        public BitmapButtonWidget F5;
        public BitmapButtonWidget F6;
        public BitmapButtonWidget F7;
        public BitmapButtonWidget F8;
        public BitmapButtonWidget F9;
        public BitmapButtonWidget F10;
        public BitmapButtonWidget F11;
        public BitmapButtonWidget F12;

        public BitmapButtonWidget WorldEditMenu;
        public StackPanelWidget WorldEditMenuContainerTop;
        public StackPanelWidget WorldEditMenuContainerBottom;

        
        public int speed = 45;

        public PrimitivesRenderer2D PrimitivesRenderer2D = new PrimitivesRenderer2D();
        public PrimitivesRenderer3D PrimitivesRenderer3D = new PrimitivesRenderer3D();
        
        public LookControlMode OldLookControlMode = LookControlMode.EntireScreen;

        public TerrainRaycastResult Point1 = default;
        public TerrainRaycastResult Point2 = default;
        public TerrainRaycastResult Point3 = default;

        public int SelectedBlock;
        public int ReplaceableBlock;
        public SpecialData SelectedSpecialData;

        internal Dictionary<Point3, SignData> SignDataList = new Dictionary<Point3, SignData>();
        internal Dictionary<Point3, IEditableItemData> EditableDataList = new Dictionary<Point3, IEditableItemData>();
        internal Dictionary<Point3, ComponentInventoryBase> InventoryDataList = new Dictionary<Point3, ComponentInventoryBase>();
        internal BlockGrid BlockMemory;

        public bool Point1Set => Point1.CellFace != default(CellFace);
        public bool Point2Set => Point2.CellFace != default(CellFace);
        public bool Point3Set => Point3.CellFace != default(CellFace);
        
        public Camera cam;
        
        public ExtrasOperator extrasOperator;
        
        public Point3 PositionFromPoints(Point3 position, BoundingBox rect)
        {
            return position - Point1.CellFace.Point;
        }
        
        public void CopySpecialData()
        {
            InventoryDataList.Clear();
            SignDataList.Clear();
            EditableDataList.Clear();
            
            BoundingBox rect = new BoundingBox(new List<Vector3>() { new Vector3(Point1.CellFace.Point), new Vector3(Point2.CellFace.Point) });
            foreach (KeyValuePair<Point3, MemoryBankData> editableData in GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>().m_blocksData)
            {
                if (!rect.Contains(new Vector3(editableData.Key)))
                {
                  continue;
               }
               
               EditableDataList.Add(PositionFromPoints(editableData.Key, rect), editableData.Value.Copy());
            }
            foreach (KeyValuePair<Point3, TruthTableData> editableData in GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>().m_blocksData)
            {
                if (!rect.Contains(new Vector3(editableData.Key)))
                {
                  continue;
                }
                
                EditableDataList.Add(PositionFromPoints(editableData.Key, rect), editableData.Value.Copy());
            }
            foreach (KeyValuePair<Point3, SubsystemSignBlockBehavior.TextData> signData in GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>().m_textsByPoint)
            {
                if (!rect.Contains(new Vector3(signData.Key)))
                {
                  continue;
                }
                
                SignDataList.Add(PositionFromPoints(signData.Key, rect), new SignData
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
                
                
                InventoryDataList.Add(PositionFromPoints(blockEntityData.Key, rect), inv);
            }
        }
        
        public void PasteSpecialData()
        {
            var membhv = GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>();
            var trbhv = GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>();
            var signbhv = GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>();
            var blockentities = GameManager.Project.FindSubsystem<SubsystemBlockEntities>();
            foreach (KeyValuePair<Point3, IEditableItemData> editable in EditableDataList)
            {
                if (editable.Value is MemoryBankData memdata)
                {
                    membhv.SetBlockData(editable.Key + Point3.CellFace.Point, memdata);
                }
                if (editable.Value is TruthTableData trdata)
                {
                    trbhv.SetBlockData(editable.Key + Point3.CellFace.Point, trdata);
                }
            }
            foreach (KeyValuePair<Point3, SignData> signData in SignDataList)
            {
                signbhv.SetSignData(signData.Key + Point3.CellFace.Point, signData.Value.Lines, signData.Value.Colors, signData.Value.Url);
            }
            
            foreach (KeyValuePair<Point3, ComponentInventoryBase> inventoryData in InventoryDataList)
            {
                if (blockentities.m_blockEntities.TryGetValue(inventoryData.Key + Point3.CellFace.Point, out ComponentBlockEntity target))
                {
                    ComponentInventoryBase inv = target.Entity.FindComponent<ComponentInventoryBase>(false);
                    if (inv == null || inv.SlotsCount != inventoryData.Value.SlotsCount) continue;
                    
                    inv.m_slots = inventoryData.Value.m_slots;
                }
            }
        }
        
        public void TryTransferBlock(Point3 origin, Point3 target)
        {
            var membhv = GameManager.Project.FindSubsystem<SubsystemMemoryBankBlockBehavior>();
            var trbhv = GameManager.Project.FindSubsystem<SubsystemTruthTableCircuitBlockBehavior>();
            var signbhv = GameManager.Project.FindSubsystem<SubsystemSignBlockBehavior>();
            var blockentities = GameManager.Project.FindSubsystem<SubsystemBlockEntities>();
            
            
            if (membhv.GetBlockData(origin) != null)
            {
                membhv.SetBlockData(target, membhv.GetBlockData(origin));
                return;
            }
            
            if (trbhv.GetBlockData(origin) != null)
            {
                trbhv.SetBlockData(target, trbhv.GetBlockData(origin));
                return;
            }
            
            if (signbhv.GetSignData(origin) != null)
            {
                var signdata = signbhv.GetSignData(origin);
                signbhv.SetSignData(target, signdata.Lines, signdata.Colors, signdata.Url);
            }
            
            blockentities.m_blockEntities.TryGetValue(origin, out ComponentBlockEntity originB);
            if (blockentities.m_blockEntities.TryGetValue(target, out ComponentBlockEntity targetB))
            {
                ComponentInventoryBase inv = targetB.Entity.FindComponent<ComponentInventoryBase>(false);
                ComponentInventoryBase origin_inv = originB.Entity.FindComponent<ComponentInventoryBase>(false);
                if (inv != null && inv.SlotsCount == origin_inv.SlotsCount)
                    inv.m_slots = origin_inv.m_slots;
            }
        }
        
        public void Initialize(ComponentPlayer player)
        {
            m_componentPlayer = player;
            m_subsystemTerrain = player.Project.FindSubsystem<SubsystemTerrain>();
            m_subsystemSky = player.Project.FindSubsystem<SubsystemSky>();
            WEOperationManager.m_subsystemTerrain = m_subsystemTerrain;
            
            Point1 = default;
            Point2 = default;
            Point3 = default;
            
            names.Add("Image");
            names.Add("Round");
            names.Add("Sphere");
            names.Add("Prism");
            names.Add("Square");
            names.Add("Frame or box");
            names.Add("Maze");
            names.Add("Mountain");
            names.Add("Line");
            //names.Add("Copy/Paste zone in file");
            
            
            
            foreach (string name in names)
            {
                m_categories.Add(new Category
                {
                    Name = name
                });
            }
            LoadBTN();
            extrasOperator = new ExtrasOperator(this);
        }
        
        
        
        public void UpdateExtras()
        {
            extras.Clear();
            extras.Add("Transfer" + (Point1Set && Point2Set && Point3Set ? "" : " (need points 1,2,3)"));
            extras.Add("Rotate" + (Point1Set && Point2Set || Point3Set ? "" : " (need points 1,2 or 3)"));
            extras.Add("Fill specific block" + (Point1Set && Point2Set ? "" : " (need points 1,2)"));
            extras.Add("Replace specific blocks" + (Point1Set && Point2Set ? "" : " (need points 1,2)"));
            extras.Add("Get specific block");
            extras.Add("Fill Memory Banks Data" + (Point1Set && Point2Set ? "" : " (need points 1,2)"));
            extras.Add("Copy/Paste zone in file" + ((Point1Set && Point2Set) || Point3Set ? "" : " (need points 1,2 or 3)"));
            extras.Add(extrasOperator.BlockBehaviorEnabled ? "Disable Block Behaviors" : "Enable Block Behaviors");
            extras.Add("Draw Chunk Boundaries");
            extras.Add("Restore Terrain" + (Point1Set && Point2Set ? "" : " (need points 1,2)"));
            extras.Add(extrasOperator.SlowCircuitStepMode ? "Normal Circuit Step Mode" : "Slow Circuit Step Mode");
            extras.Add("Technical Information");
            extras.Add(extrasOperator.BlockLightingEnabled ? "Disable Block Lighting" : "Enable Block Lighting");
            extras.Add("Edit World Settings");
            extras.Add("Save World Map");
            extras.Add(extrasOperator.TimeStopped ? "Resume Time" : "Stop Time");
            extras.Add(extrasOperator.PlayerCollisionEnabled ? "Disable Player Collision" : "Enable Player Collision");
            extras.Add("Teleport");
            extras.Add("Clear Animals");
            extras.Add("Clear Drops");
            if (Point1Set || Point2Set || Point3Set)
                extras.Add("Unselect Points");
            extras.Add("Fast Run Settings");
        }
        
        public void ShowExtras()
        {
            UpdateExtras();
            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new ListSelectionDialog(string.Empty, extras, 56f, delegate(object c)
            {
                LabelWidget labelWidget = new LabelWidget();
                labelWidget.Text = c.ToString();
                labelWidget.Color = c.ToString().Contains("(") ? Color.Gray*0.9f : Color.White;
                int horizontalAlignment = 1;
                labelWidget.HorizontalAlignment = (WidgetAlignment) horizontalAlignment;
                int verticalAlignment = 1;
                labelWidget.VerticalAlignment = (WidgetAlignment) verticalAlignment;
                return labelWidget;
            }, delegate(object c)
            {
                if (c == null)
                {
                    return;
                }
                string extraName = c.ToString();
                if (extraName.Contains("("))
                    return;
                switch (extraName)
                {
                    case "Disable Block Lighting":
                    case "Enable Block Lighting":
                        extrasOperator.BlockLightingEnabled = !extrasOperator.BlockLightingEnabled;
                        extrasOperator.UpdateBlockLighting();
                        break;
                    case "Clear Animals":
                        extrasOperator.ClearAnimals();
                        break;
                    case "Clear Drops":
                        extrasOperator.ClearDrops();
                        break;
                    case "Disable Player Collision":
                    case "Enable Player Collision":
                        extrasOperator.PlayerCollisionEnabled = !extrasOperator.PlayerCollisionEnabled;
                        extrasOperator.UpdatePlayerCollision();
                        break;
                    case "Draw Chunk Boundaries":
                        extrasOperator.DrawChunkBorders = !extrasOperator.DrawChunkBorders;
                        break;
                    case "Unselect Points":
                        Point1 = default;
                        Point2 = default;
                        Point3 = default;
                        break;
                    case "Teleport":
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new TeleportDialog(m_componentPlayer));
                        break;
                    case "Edit World Settings":
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new ModifyWorldDialog(m_subsystemTerrain.Project.FindSubsystem<SubsystemGameInfo>().WorldSettings));
                        break;
                    case "Fast Run Settings":
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new FastRun(m_componentPlayer));
                        break;
                    case "Copy/Paste zone in file":
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new ZoneDialog(m_componentPlayer, Point1, Point2, Point3, m_subsystemTerrain));
                        break;
                    case "Fill Memory Banks Data":
                        WEOperationManager.StartOperation("FillMemoryBankData", this);
                       break;
                    case "Disable Block Behaviors":
                    case "Enable Block Behaviors":
                        extrasOperator.BlockBehaviorEnabled = !extrasOperator.BlockBehaviorEnabled;
                        extrasOperator.UpdateBlockBehaviorEnabled();
                        break;
                    case "Slow Circuit Step Mode":
                    case "Normal Circuit Step Mode":
                        extrasOperator.ToggleCircuitStepDebugger();
                        break;
                    case "Restore Terrain":
                        WEOperationManager.StartOperation("RestoreTerrain", this);
                        break;
                    case "Stop Time":
                    case "Resume Time":
                        extrasOperator.TimeStopped = !extrasOperator.TimeStopped;
                        extrasOperator.UpdateTimeStop();
                        break;
                }
            }));
        }
        
        public void OnExit()
        {
            if (WorldEditMenu.IsChecked)
            {
                SettingsManager.LookControlMode = OldLookControlMode;
            }
        }
        
        public void Draw(Camera camera)
        {
            cam = camera;
            
            if (Point1Set && Point2Set && WorldEditMenu.IsChecked) // Выделение зоны между точками 1 и 2
            {
                int startX = MathUtils.Min(Point1.CellFace.X, Point2.CellFace.X);
                int endX = MathUtils.Max(Point1.CellFace.X, Point2.CellFace.X);
                int startY = MathUtils.Min(Point1.CellFace.Y, Point2.CellFace.Y);
                int endY = MathUtils.Max(Point1.CellFace.Y, Point2.CellFace.Y);
                int startZ = MathUtils.Min(Point1.CellFace.Z, Point2.CellFace.Z);
                int endZ = MathUtils.Max(Point1.CellFace.Z, Point2.CellFace.Z);

                Vector3 pointStart = new Vector3(startX, startY, startZ);
                Vector3 pointEnd = new Vector3(endX + 1, endY + 1, endZ + 1);
                BoundingBox boundingBox = new BoundingBox(pointStart, pointEnd);
                PrimitivesRenderer3D.FlatBatch(0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque).QueueBoundingBox(boundingBox, Color.Green);
            }

            if (Point3Set && WorldEditMenu.IsChecked) // Выделение зоны вставки / Selection of the insertion zone
            {
                int startX = MathUtils.Min(Point1.CellFace.X, Point2.CellFace.X);
                int endX = MathUtils.Max(Point1.CellFace.X, Point2.CellFace.X);
                int startY = MathUtils.Min(Point1.CellFace.Y, Point2.CellFace.Y);
                int endY = MathUtils.Max(Point1.CellFace.Y, Point2.CellFace.Y);
                int startZ = MathUtils.Min(Point1.CellFace.Z, Point2.CellFace.Z);
                int endZ = MathUtils.Max(Point1.CellFace.Z, Point2.CellFace.Z);

                startX += Point3.CellFace.X - Point1.CellFace.X;
                startY += Point3.CellFace.Y - Point1.CellFace.Y;
                startZ += Point3.CellFace.Z - Point1.CellFace.Z;
                endX += Point3.CellFace.X - Point1.CellFace.X;
                endY += Point3.CellFace.Y - Point1.CellFace.Y;
                endZ += Point3.CellFace.Z - Point1.CellFace.Z;
                
                Vector3 pointStart = new Vector3(startX, startY, startZ);
                Vector3 pointEnd = new Vector3(endX + 1, endY + 1, endZ + 1);
                BoundingBox boundingBox = new BoundingBox(pointStart, pointEnd);
                PrimitivesRenderer3D.FlatBatch(0, DepthStencilState.None, (RasterizerState)null, (BlendState)null).QueueBoundingBox(boundingBox, Color.Red);
            }
            
            PrimitivesRenderer3D.Flush(camera.ViewProjectionMatrix, true, int.MaxValue);
            //PrimitivesRenderer2D.Flush(true);
            extrasOperator.Draw(camera);
        }
        
        public void Update()
        {
            try
            {
                if (Keyboard.IsKeyDownOnce(Key.F1) || F1.IsClicked)
                {
                    
                    Ray3 ray1 = new Ray3(cam.ViewPosition, cam.ViewDirection);
                    object result = m_componentPlayer.ComponentMiner.Raycast(ray1, RaycastMode.Digging, true, false, false);
                    if (result is TerrainRaycastResult)
                    {
                        Point1 = (TerrainRaycastResult) result;
                        SelectedBlock = m_subsystemTerrain.Terrain.GetCellValue(Point1.CellFace.X, Point1.CellFace.Y, Point1.CellFace.Z);
                        SelectedSpecialData = SpecialData.FromPosition(Point1.CellFace.Point);
                        ShowMessage($"Set position 1 on: {Point1.CellFace.X}, {Point1.CellFace.Y}, {Point1.CellFace.Z}, Block ID {SelectedBlock}");
                    }
                    else
                    {
                        Point1 = default;
                        Point1.CellFace.Point = new Point3(cam.ViewPosition);
                        Point1.CellFace.Face = 5;
                        SelectedBlock = m_subsystemTerrain.Terrain.GetCellValue(Point1.CellFace.X, Point1.CellFace.Y, Point1.CellFace.Z);
                        SelectedSpecialData = SpecialData.FromPosition(Point1.CellFace.Point);
                        ShowMessage($"Set position 1 on: {Point1.CellFace.X}, {Point1.CellFace.Y}, {Point1.CellFace.Z}, Block ID {SelectedBlock}");
                    }
                }
                if (Keyboard.IsKeyDown(Key.F2) || F2.IsClicked)
                {
                    Ray3 ray1 = new Ray3(cam.ViewPosition, cam.ViewDirection);
                    object result = m_componentPlayer.ComponentMiner.Raycast(ray1, RaycastMode.Digging, true, false, false);
                    if (result is TerrainRaycastResult)
                    {
                        Point2 = (TerrainRaycastResult) result;
                        ReplaceableBlock = m_subsystemTerrain.Terrain.GetCellValue(Point2.CellFace.X, Point2.CellFace.Y, Point2.CellFace.Z);
                        ShowMessage($"Set position 2 on: {Point2.CellFace.X}, {Point2.CellFace.Y}, {Point2.CellFace.Z}, Block ID {ReplaceableBlock}");
                    }
                    else
                    {
                        Point2 = default;
                        Point2.CellFace.Point = new Point3(cam.ViewPosition);
                        Point2.CellFace.Face = 5;
                        ReplaceableBlock = m_subsystemTerrain.Terrain.GetCellValue(Point2.CellFace.X, Point2.CellFace.Y, Point2.CellFace.Z);
                        ShowMessage($"Set position 2 on: {Point2.CellFace.X}, {Point2.CellFace.Y}, {Point2.CellFace.Z}, Block ID {ReplaceableBlock}");
                    }
                }
                if (Keyboard.IsKeyDown(Key.F3) || F3.IsClicked)
                {
                    Ray3 ray1 = new Ray3(cam.ViewPosition, cam.ViewDirection);
                    object result = m_componentPlayer.ComponentMiner.Raycast(ray1, RaycastMode.Digging, true, false, false);
                    if (result is TerrainRaycastResult)
                    {
                        Point3 = (TerrainRaycastResult) result;
                        ShowMessage($"Set position 3 on: {Point3.CellFace.X}, {Point3.CellFace.Y}, {Point3.CellFace.Z}, Block ID {ReplaceableBlock}");
                        return;
                    }
                }
                
                if (F5.IsChecked)
                {
                    if (m_componentPlayer.ComponentLocomotion.WalkSpeed != speed)
                    {
                        m_componentPlayer.ComponentLocomotion.WalkSpeed = speed;
                        m_componentPlayer.ComponentLocomotion.CreativeFlySpeed = speed;
                    }
               }
               else
               {
                      m_componentPlayer.ComponentLocomotion.WalkSpeed = m_componentPlayer.ComponentLocomotion.ValuesDictionary.GetValue<float>("WalkSpeed");
                      m_componentPlayer.ComponentLocomotion.CreativeFlySpeed = m_componentPlayer.ComponentLocomotion.ValuesDictionary.GetValue<float>("CreativeFlySpeed");
                }
                
                WorldEditMenuContainerBottom.IsVisible = WorldEditMenu.IsChecked;
                WorldEditMenuContainerTop.IsVisible = WorldEditMenu.IsChecked;
                if (WorldEditMenu.IsClicked)
                { 
                    if (WorldEditMenu.IsChecked)
                    {
                        OldLookControlMode = (LookControlMode) ((int) SettingsManager.LookControlMode);
                        SettingsManager.LookControlMode = LookControlMode.SplitTouch;
                    }
                    else
                    {
                        SettingsManager.LookControlMode = OldLookControlMode;
                    }
                }
                if (Keyboard.IsKeyDownOnce(Key.F12) || F12.IsClicked)
                {
                    ShowExtras();
                }
                else if (Keyboard.IsKeyDownOnce(Key.F6) || F6.IsClicked)
                {
                    WEOperationManager.StartOperation("Fill", this);
                }
                else if (Keyboard.IsKeyDownOnce(Key.F7) || F7.IsClicked)
                {
                    WEOperationManager.StartOperation("Replace", this);
                }
                else if (Keyboard.IsKeyDownOnce(Key.F8) || F8.IsClicked)
                {
                    WEOperationManager.StartOperation("Clear", this);
                }
                else if (Keyboard.IsKeyDownOnce(Key.F9) || F9.IsClicked)
                {
                    WEOperationManager.StartOperation("MemCopy", this);
                }
                else if (Keyboard.IsKeyDownOnce(Key.F10) || F10.IsClicked)
                {
                    WEOperationManager.StartOperation("MemPaste", this);
                }
                else if (F11.IsClicked || Keyboard.IsKeyDownOnce(Key.Enter))
                {
                    Select_mode(m_categories, names);
                }
                extrasOperator.UpdateTimeStop();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        public void ShowMessage(string error)
        {
            m_componentPlayer.ComponentGui.DisplaySmallMessage(error, Color.White, false, false);
        }

        public float ProcessInputValue(float value, float deadZone, float saturationZone)
        {
            return MathUtils.Sign(value) * MathUtils.Clamp((float)(((double)MathUtils.Abs(value) - (double)deadZone) / ((double)saturationZone - (double)deadZone)), 0f, 1f);
        }
        
        public void Select_mode(List<Category> m_categories, List<string> a)
        {
            if (m_componentPlayer != null)
            {
                DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new ListSelectionDialog(string.Empty, m_categories, 56f, delegate(object c)
                {
                    LabelWidget labelWidget = new LabelWidget();
                    labelWidget.Text = ((Category)c).Name;
                    labelWidget.Color = Color.White;
                    int horizontalAlignment = 1;
                    labelWidget.HorizontalAlignment = (WidgetAlignment) horizontalAlignment;
                    int verticalAlignment = 1;
                    labelWidget.VerticalAlignment = (WidgetAlignment) verticalAlignment;
                    return labelWidget;
                }, delegate(object c)
                {
                    if (c == null)
                    {
                        return;
                    }
                    int index = m_categories.IndexOf((Category)c);
                    string a2 = a[index];
                    if (a2 == "Image")
                    {
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new ImageDialog(m_componentPlayer, Point1, Point2, Point3, m_subsystemTerrain));
                    }
                    else if (a2 == "Round")
                    {
                        if (!Point1Set)
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected point 1", Color.White, false, false);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new Round(m_componentPlayer, SelectedBlock, Point1, m_subsystemTerrain));
                        }
                    }
                    else if (a2 == "Sphere")
                    {
                        if (!Point1Set)
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected point 1", Color.White, false, false);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new Sphere(m_componentPlayer, SelectedBlock, Point1, m_subsystemTerrain));
                        }
                    }
                    else if (a2 == "Prism")
                    {
                        if (!Point1Set)
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected point 1", Color.White, false, false);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new Prism(m_componentPlayer, SelectedBlock, Point1, m_subsystemTerrain));
                        }
                    }
                    else if (a2 == "Square")
                    {
                        if (!Point1Set)
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected point 1", Color.White, false, false);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new Square(m_componentPlayer, SelectedBlock, Point1, m_subsystemTerrain));
                        }
                    }
                    else if (a2 == "Frame or box")
                    {
                        if (!Point1Set && !Point2Set)
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected points 1,2", Color.White ,false, false);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new Rectangle(m_componentPlayer, SelectedBlock, Point1, Point2, m_subsystemTerrain));
                        }
                    }
                    else if (a2 == "Maze")
                    {
                        if (!(Point1Set && Point2Set))
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected points 1,2", Color.White, false, false);
                        }
                        else
                        {
                            Point3 start;
                            start.X = Point1.CellFace.X;
                            start.Y = Point1.CellFace.Y;
                            start.Z = Point1.CellFace.Z;
                            Point3 end;
                            end.X = Point2.CellFace.X;
                            end.Y = Point2.CellFace.Y;
                            end.Z = Point2.CellFace.Z;
                            API_WE.CreativeMaze(start, end, SelectedBlock, m_subsystemTerrain, m_componentPlayer);
                        }
                    }
                    else if (a2 == "Mountain")
                    {
                        if (!Point1Set)
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected point 1", Color.White, false, false);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new Mountain(m_componentPlayer, Point1, m_subsystemTerrain));
                        }
                    }
                    if (a2 == "Copy/Paste zone in file")
                    {
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new ZoneDialog(m_componentPlayer, Point1, Point2, Point3, m_subsystemTerrain));
                    }
                    else if (a2 == "Line")
                    {
                        Point3 v = default(Point3);
                        Point3 v2 = default(Point3);
                        if (!(Point1Set && Point2Set))
                        {
                            m_componentPlayer.ComponentGui.DisplaySmallMessage("You have not selected points 1,2", Color.White, false, false);
                        }
                        else
                        {
                            v.X = Point1.CellFace.X;
                            v.Y = Point1.CellFace.Y;
                            v.Z = Point1.CellFace.Z;
                            v2.X = Point2.CellFace.X;
                            v2.Y = Point2.CellFace.Y;
                            v2.Z = Point2.CellFace.Z;
                            API_WE.LinePoint(v, v2, SelectedBlock, m_subsystemTerrain);
                        }
                    }
                    else if (a2 == "Fast Run Settings")
                    {
                        DialogsManager.ShowDialog(m_componentPlayer.GameWidget, new FastRun(m_componentPlayer));
                    }
                }));
            }
        }

        

        public void lingTi(bool s, int f, int blockID, int r, TerrainRaycastResult Point1)
        {
            int num = 0;
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    for (int k = -r; k <= r; k++)
                    {
                        if (f == 1)
                        {
                            if (MathUtils.Abs(i) + MathUtils.Abs(j) <= r && MathUtils.Abs(k) + MathUtils.Abs(j) <= r && (!s || MathUtils.Abs(i) + MathUtils.Abs(j) >= r || MathUtils.Abs(k) + MathUtils.Abs(j) >= r))
                            {
                                SubsystemTerrain subsystemTerrain = m_subsystemTerrain;
                                TerrainRaycastResult value = Point1;
                                int num2 = value.CellFace.X + i;
                                value = Point1;
                                int num3 = value.CellFace.Y + j;
                                value = Point1;
                                subsystemTerrain.ChangeCell(num2, num3, value.CellFace.Z + k, blockID, true);
                                num++;
                            }
                        }
                        else if (f == 2)
                        {
                            if (MathUtils.Abs(j) + MathUtils.Abs(i) <= r && MathUtils.Abs(k) + MathUtils.Abs(i) <= r && (!s || MathUtils.Abs(j) + MathUtils.Abs(i) >= r || MathUtils.Abs(k) + MathUtils.Abs(i) >= r))
                            {
                                SubsystemTerrain subsystemTerrain2 = m_subsystemTerrain;
                                TerrainRaycastResult value = Point1;
                                int num4 = value.CellFace.X + i;
                                value = Point1;
                                int num5 = value.CellFace.Y + j;
                                value = Point1;
                                subsystemTerrain2.ChangeCell(num4, num5, value.CellFace.Z + k, blockID, true);
                                num++;
                            }
                        }
                        else if (MathUtils.Abs(j) + MathUtils.Abs(k) <= r && MathUtils.Abs(i) + MathUtils.Abs(k) <= r && (!s || MathUtils.Abs(j) + MathUtils.Abs(k) >= r || MathUtils.Abs(i) + MathUtils.Abs(k) >= r))
                        {
                            SubsystemTerrain subsystemTerrain3 = m_subsystemTerrain;
                            TerrainRaycastResult value = Point1;
                            int num6 = value.CellFace.X + i;
                            value = Point1;
                            int num7 = value.CellFace.Y + j;
                            value = Point1;
                            subsystemTerrain3.ChangeCell(num6, num7, value.CellFace.Z + k, blockID, true);
                            num++;
                        }
                    }
                }
            }
            m_componentPlayer.ComponentGui.DisplaySmallMessage(string.Format("7896868", num), Color.White, true, true);
        }

        public void LoadBTN()
        {
            GameWidget gameWidget = m_componentPlayer.GameWidget;
            m_gameWidget = gameWidget;
            StackPanelWidget WEPlace = gameWidget.Children.Find<StackPanelWidget>("RightControlsContainer");
            WEPlace.LoadChildren(WEPlace, ContentManager.Get<XElement>("WE/WEMenu"));
            Subtexture f1_normal = new Subtexture(ContentManager.Get<Texture2D>("WE/Button1"), Vector2.Zero, Vector2.One);
            Subtexture f2_normal = new Subtexture(ContentManager.Get<Texture2D>("WE/Button2"), Vector2.Zero, Vector2.One);
            Subtexture f3_normal = new Subtexture(ContentManager.Get<Texture2D>("WE/Button3"), Vector2.Zero, Vector2.One);
            Subtexture extra_normal = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonExtra"), Vector2.Zero, Vector2.One);
            Subtexture fill_normal = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonFill"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture6 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonReplace"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture7 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonClear"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture8 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonMemCopy"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture9 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonMemPaste"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture = new Subtexture(ContentManager.Get<Texture2D>("WE/Button1_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture2 = new Subtexture(ContentManager.Get<Texture2D>("WE/Button2_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture3 = new Subtexture(ContentManager.Get<Texture2D>("WE/Button3_pressed"), Vector2.Zero, Vector2.One);
            Subtexture extra_clicked = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonExtra_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture5 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonFill_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture6 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonReplace_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture7 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonClear_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture8 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonMemCopy_pressed"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture9 = new Subtexture(ContentManager.Get<Texture2D>("WE/ButtonMemPaste_pressed"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture10 = new Subtexture(ContentManager.Get<Texture2D>("WE/Button_geo"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture10 = new Subtexture(ContentManager.Get<Texture2D>("WE/Button_geo_press"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture11 = new Subtexture(ContentManager.Get<Texture2D>("WE/Button_run"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture11 = new Subtexture(ContentManager.Get<Texture2D>("WE/Button_run_press"), Vector2.Zero, Vector2.One);
            Subtexture normalSubtexture12 = new Subtexture(ContentManager.Get<Texture2D>("WE/WEBTN"), Vector2.Zero, Vector2.One);
            Subtexture clickedSubtexture12 = new Subtexture(ContentManager.Get<Texture2D>("WE/WEBTNP"), Vector2.Zero, Vector2.One);
            F1 = WEPlace.Children.Find<BitmapButtonWidget>("F1", true);
            F2 = WEPlace.Children.Find<BitmapButtonWidget>("F2", true);
            F3 = WEPlace.Children.Find<BitmapButtonWidget>("F3", true);
            F5 = WEPlace.Children.Find<BitmapButtonWidget>("F5", true);
            F6 = WEPlace.Children.Find<BitmapButtonWidget>("F6", true);
            F7 = WEPlace.Children.Find<BitmapButtonWidget>("F7", true);
            F8 = WEPlace.Children.Find<BitmapButtonWidget>("F8", true);
            F9 = WEPlace.Children.Find<BitmapButtonWidget>("F9", true);
            F10 = WEPlace.Children.Find<BitmapButtonWidget>("F10", true);
            F11 = WEPlace.Children.Find<BitmapButtonWidget>("F11", true);
            F12 = WEPlace.Children.Find<BitmapButtonWidget>("F12", true);
            WorldEditMenu = gameWidget.Children.Find<BitmapButtonWidget>("WorldEditMenu", true);
            WorldEditMenuContainerBottom = gameWidget.Children.Find<StackPanelWidget>("WorldEditMenuContainerBottom", true);
            WorldEditMenuContainerTop = gameWidget.Children.Find<StackPanelWidget>("WorldEditMenuContainerTop", true);
            WorldEditMenu.NormalSubtexture = normalSubtexture12;
            WorldEditMenu.ClickedSubtexture = clickedSubtexture12;
            F1.NormalSubtexture = f1_normal;
            F2.NormalSubtexture = f2_normal;
            F3.NormalSubtexture = f3_normal;
            F5.NormalSubtexture = normalSubtexture11;
            F6.NormalSubtexture = fill_normal;
            F7.NormalSubtexture = normalSubtexture6;
            F8.NormalSubtexture = normalSubtexture7;
            F9.NormalSubtexture = normalSubtexture8;
            F10.NormalSubtexture = normalSubtexture9;
            F1.ClickedSubtexture = clickedSubtexture;
            F2.ClickedSubtexture = clickedSubtexture2;
            F3.ClickedSubtexture = clickedSubtexture3;
            F5.ClickedSubtexture = clickedSubtexture11;
            F6.ClickedSubtexture = clickedSubtexture5;
            F7.ClickedSubtexture = clickedSubtexture6;
            F8.ClickedSubtexture = clickedSubtexture7;
            F9.ClickedSubtexture = clickedSubtexture8;
            F10.ClickedSubtexture = clickedSubtexture9;
            F11.NormalSubtexture = normalSubtexture10;
            F11.ClickedSubtexture = clickedSubtexture10;
            F12.NormalSubtexture = extra_normal;
            F12.ClickedSubtexture = extra_clicked;
        }
        
        public void SetCell(int x, int y, int z, int id)
        {
            m_subsystemTerrain.ChangeCell(x, y, z, id, true);
        }
    }
}
