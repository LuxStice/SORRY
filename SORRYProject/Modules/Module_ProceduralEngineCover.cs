using KSP.Game;
using KSP.Messages;
using KSP.OAB;
using KSP.Sim.Definitions;
using KSP.Sim.DeltaV;
using System.Net.NetworkInformation;
using UnityEngine;

namespace SORRY.Modules
{
    public sealed class Module_ProceduralEngineCover : PartBehaviourModule
    {
        [SerializeField]
        public Data_ProceduralEngineCover _dataProceduralEngineCover = new();
        public override Type PartComponentModuleType => typeof(PartComponentModule_ProceduralEngineCover);
        private static ObjectAssemblyBuilder OAB => GameManager.Instance.Game.OAB.Current;
        private static StagingDataProvider stagingDataProvider => ((ObjectAssemblyBuilderHUD)OAB.Stats.OABActiveHUD).stagingDriver._stagingDataProvider;

        public override void AddDataModules()
        {
            base.AddDataModules();
            _dataProceduralEngineCover ??= new Data_ProceduralEngineCover();
            this.DataModules.TryAddUnique<Data_ProceduralEngineCover>(this._dataProceduralEngineCover, out this._dataProceduralEngineCover);
        }

        public override void InitForNewOABPart()
        {
            base.InitForNewOABPart();
            isDirty = true;
            SORRYPlugin.Instance.OpenWindow(true);
        }
        public void ShowWindow(bool change = true)
        {
            _WindowOpen = change;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            SORRYPlugin.Register(this);
            if (OABPart is not null)
            {
                var bottomNode = OABPart.GetNode("bottom");
                _dataProceduralEngineCover.originalBottomNodePos = bottomNode.PartRelativePosition;
                UpdateNodes();
            }
            //CreateNodes();
        }

        public override void OnModuleOABFixedUpdate(float fixedDeltaTime)
        {
            base.OnModuleOABFixedUpdate(fixedDeltaTime);

            if (isDirty)
            {
                isDirty = false;
                UpdateNodes();
            }
        }

        public ObjectAssemblyPartNode ToNode(AttachNodeDefinition nodeDefinition)
        {
            var oabPArt1 = (ObjectAssemblyPart)OABPart;
            ObjectAssemblyAssets builderAssets = OAB.BuilderAssets;

            var node = new ObjectAssemblyAvailablePartNode(nodeDefinition.size, nodeDefinition.position,
                Quaternion.LookRotation(nodeDefinition.orientation, Vector3.up), nodeDefinition.nodeID, nodeDefinition.NodeSymmetryGroupID,
                nodeDefinition.visualSize, nodeDefinition.nodeType, true);
            return oabPArt1.CreateStackNode(OABPart, node, builderAssets);
        }
        public void UpdateNodes()
        {
            bool stagingChanged = false;
            if (OABPart is null)
            {
                isDirty = false;
                return;
            }

            var nodeDefs = _dataProceduralEngineCover.GetRingsDefinitions();

            var bottomNode = OABPart.GetNode("bottom");
            OABPart.SetNodeLocalPosition(bottomNode, 
                _dataProceduralEngineCover.originalBottomNodePos 
                + new Vector3(0, _dataProceduralEngineCover.BottomNodeOffset, 0));

            List<string> updatedNodeList = new() { "top", "bottom" };

            for (int i = 0; i < nodeDefs.Count; i++)
            {
                Data_ProceduralEngineCover.ClusterRing currentRing = _dataProceduralEngineCover.Rings[i];
                Data_ProceduralEngineCover.ClusterRing previousRing, nextRing;
                ///TODO: Add offset limit according to this
                if (i > 0)
                    previousRing = _dataProceduralEngineCover.Rings[i - 1];
                if (i < nodeDefs.Count - 1)
                    nextRing = _dataProceduralEngineCover.Rings[i + 1];

                IObjectAssemblyPart PartTemplate = null;
                foreach(var node in OABPart.Nodes)
                {
                    if(node.NodeSymmetryGroupID == nodeDefs[i][0].NodeSymmetryGroupID)
                    {
                        if (node.IsConnected)
                            PartTemplate = node.ConnectedPart;
                    }
                }

                ///TODO: 1) look at existing nodes and compare with new ones
                ///2) if already exists, update positions
                ///3) if new add to list and instantiate engine
                ///4) if removed, delete engine and delete node
                ///4) 1. look into an updated list of nodes to remove old ones

                foreach (var nodeDef in nodeDefs[i])
                {
                    ObjectAssemblyPartNode node = null;
                    foreach (var OABPartNode in OABPart.Nodes)
                    {
                        if (OABPartNode.NodeTag == nodeDef.nodeID)
                        {
                            node = (ObjectAssemblyPartNode)OABPartNode;
                            break;
                        }
                    }//1)

                    if (node is null)
                    {
                        node = ToNode(nodeDef);
                        OABPart.Nodes.Add(node);
                        //get part template
                        if (PartTemplate is not null)
                        {
                            var newPart = OAB.ActivePartTracker.ManuallyCreatePart(OAB.ActivePartTracker._allKnownParts.Find(a => a.PartData.partName == PartTemplate.PartName), null, false, true, null);
                            var newPartTopNode = newPart.Nodes.Find(a => a.NodeTag.ToLower() == "top");
                            ObjectAssemblyPlacementTool.SnapNodes(node, newPartTopNode);
                            OAB.ActivePartTracker.StackParts(node, newPartTopNode);
                            newPart.Assembly = PartTemplate.Assembly;
                            newPart.SetOriginalPart(PartTemplate);
                            var partSymmetry = stagingDataProvider.FindPartSymmetrySet(((IDeltaVPart)PartTemplate).GlobalId);
                            partSymmetry.Data.AllParts.Add(((IDeltaVPart)newPart).GlobalId);
                            stagingChanged = true;

                        }
                    }//3)
                    OABPart.SetNodeLocalPosition(node, nodeDef.position);//2)
                    OABPart.SetNodeLocalScale(node, nodeDef.size);
                    if (node.IsConnected)
                    {
                        var otherNode = node.ConnectedPart.GetNode("top");
                        if (otherNode is null)
                        {
                            SORRYLog.Error($"Part {node.ConnectedPart.PartName} has incorrectly named the top node! It should be \"top\"! please report to the modder");
                        }
                        else if (Vector3.Distance(node.WorldPosition, otherNode.WorldPosition) > 0.001f)
                        {
                            Vector3 difference = node.WorldPosition - otherNode.WorldPosition;
                            otherNode.Owner.PartTransform.position += difference;
                        }
                    }

                    updatedNodeList.Add(node.NodeTag);//NodeTag should be unique so it serves as an ID
                }
            }

            for (int n = 0; n < OABPart.Nodes.Count; n++)
            {
                var node = OABPart.Nodes[n];
                if (updatedNodeList.Contains(node.NodeTag))
                    continue;
                stagingChanged = true;
                if (node.IsConnected)
                    OAB.ActivePartTracker.DeletePart(node.ConnectedPart);
                Destroy(node.NodeTransform.gameObject);
                OABPart.Nodes.Remove(node);
                n--;
            }//4)

            OABPart.ShowNodes(true);
            if (stagingChanged)
            {
                Game.Messages.Publish<PartStageChangedMessage>();
                stagingDataProvider.RebuildPartSymmetrySets();
            }
        }

        private int FirstIndexOfCluster(Data_ProceduralEngineCover.ClusterRing ring)
        {
            int index = 1;//top and bottom
            var Rings = _dataProceduralEngineCover.Rings;

            for (int i = 0; i < Rings.Count; i++)
            {
                var currentRing = Rings[i];
                if (currentRing.Equals(ring))
                    break;

                i += currentRing.EngineCount;
            }

            return index;
        }
        private int LastIndexOfCluster(Data_ProceduralEngineCover.ClusterRing ring)
        {
            int index = 1;//top and bottom
            var Rings = _dataProceduralEngineCover.Rings;

            for (int i = 0; i < Rings.Count; i++)
            {
                var currentRing = Rings[i];
                i += currentRing.EngineCount;
                if (currentRing.Equals(ring))
                {
                    break;
                }
            }

            return index;
        }

        new void OnDestroy()
        {
            SORRYPlugin.Unregister(this);
            base.OnDestroy();
        }

        public void AddCluster(Data_ProceduralEngineCover.EngineSlot slot)
        {
            _dataProceduralEngineCover.AddCluster(new() { EngineSlot = slot });
        }
        public void AddCluster(Data_ProceduralEngineCover.ClusterRing ring)
        {
            _dataProceduralEngineCover.AddCluster(ring);
        }
        public void RemoveCluster(Data_ProceduralEngineCover.ClusterRing cluster)
        {
            _dataProceduralEngineCover.RemoveCluster(cluster);
        }

        [NonSerialized]
        public bool _WindowOpen;
        List<Data_ProceduralEngineCover.ClusterRing> rings => _dataProceduralEngineCover.Rings;
        float bottomHeight => _dataProceduralEngineCover.BottomNodeOffset;
        int ringAmount => _dataProceduralEngineCover.Rings.Count;
        bool isDirty;

        public void DrawGUI()
        {
            if (_WindowOpen)
            {
                bool changed = false;
                GUILayout.Label(OABPart is not null ? OABPart.PartName : "ERROR");

                float bottomOffset = this.bottomHeight;
                GUILayout.Label($"Bottom node Height: {bottomHeight}");
                bottomOffset = GUILayout.HorizontalSlider(bottomOffset, -1.5f, 1.5f);
                if (bottomOffset != bottomHeight)
                {
                    bottomOffset = (float)Math.Round(bottomOffset, 2);
                    _dataProceduralEngineCover.BottomNodeOffset = bottomOffset;
                    changed = true;
                }

                if (GUILayout.Button("Add Ring"))
                {
                    int min = 1, max = 24;

                    if (rings.Count > 0)
                        min = 2;

                    AddCluster(new Data_ProceduralEngineCover.ClusterRing(1.25f, rings.Count * 3, min, max));
                }


                for (int i = 0; i < rings.Count; i++)
                {
                    var ring = rings[i];
                    if (ring._showGUI)
                    {
                        if (GUILayout.Button($"Ring#{i} [-]"))
                        {
                            ring._showGUI = false;
                        }
                        if (ring.DrawGUI())
                        {
                            changed = true;
                        }
                        if (GUILayout.Button("Delete Ring", new GUIStyle(GUI.skin.button) {  fontStyle = FontStyle.Bold,richText = true}))
                        {
                            i--;
                            RemoveCluster(ring);
                            changed = true;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button($"Ring#{i} [+]"))
                        {
                            ring._showGUI = true;
                        }
                    }
                }

                if (changed)
                {
                    isDirty = true;
                }
            }
        }


        internal static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
    }
}