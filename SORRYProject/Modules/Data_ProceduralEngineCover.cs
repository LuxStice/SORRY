using KSP.OAB;
using KSP.Rendering.Planets;
using KSP.Sim;
using KSP.Sim.Definitions;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking.Types;
using static SORRY.Modules.Module_ProceduralEngineCover;

namespace SORRY.Modules
{
    [Serializable]
    public sealed class Data_ProceduralEngineCover : ModuleData
    {
        public override Type ModuleType => typeof(Module_ProceduralEngineCover);

        [LocalizedField("SORRY/PEC/DEBUG/Diameter")]
        [Tooltip("Part Diameter")]
        [KSPState]
        public float Diameter = 5;///TODO: Create PAM label with this value

        [LocalizedField("SORRY/PEC/DEBUG/BottomHeight")]
        [Tooltip("Height of the bottom node, the default node will be deleted!")]
        [KSPState]
        [HideInInspector]
        public float BottomNodeOffset = 0f;///TODO: Maybe change this to a ModuleProperty and allow it to be changed in PAM

        [LocalizedField("SORRY/PEC/DEBUG/Rings")]
        [HideInInspector]
        [KSPState]
        [Tooltip("Rings that make the cluster")]
        public List<ClusterRing> Rings = new();

        [HideInInspector]
        [KSPState]
        public Vector3 originalBottomNodePos;

        internal List<AttachNodeDefinition> GetRingDefinitions(int index)
        {
            return GetRingsDefinitions()[index];
        }
        internal List<List<AttachNodeDefinition>> GetRingsDefinitions()
        {
            List<ClusterRing> rings = Rings;
            List<List<AttachNodeDefinition>> ringDefs = new(rings.Count);
            float diameter = Diameter;
            float currentOffsetFromCenter = 0;

            for (int i = 0; i < rings.Count; i++)
            {
                //Saving some usefull data
                ClusterRing currentRing = rings[i];
                EngineSlot slot = currentRing.EngineSlot;
                List<AttachNodeDefinition> currentDefs = new(currentRing.EngineCount);
                var Sizes = SizesUI.GetSize(currentRing.EngineSlot.Diameter);
                if (i > 0 && currentRing.EngineCount == 1)
                    SORRYLog.Warning("Engine cluster has only 1 engine but its not the middle cluster!");
                //Calculating the offset from the center (aka the radius)
                if (i == 0)
                {
                    if (currentRing.EngineCount > 1)
                        currentOffsetFromCenter = slot.NozzleRadius;
                }
                else
                {
                    currentOffsetFromCenter = diameter / rings.Count * i;
                    currentOffsetFromCenter /= 2f;
                }

                currentOffsetFromCenter += currentRing.PositionOffset;
                ///TODO: Maybe add limit relative to previous and next ring
                currentOffsetFromCenter = Mathf.Clamp(currentOffsetFromCenter, 0, diameter/2f);

                float rotateBy = 360f / currentRing.EngineCount;//Getting the rotation angle
                Vector3 pos = new Vector3(currentOffsetFromCenter, currentRing.HeightOffset, 0);//Apply current offset and height offset

                if (currentRing.RotationOffset > -180 && currentRing.RotationOffset <= 180 && currentRing.RotationOffset != 0)//Apply rotation Offset
                    pos = RotatePointAroundPivot(pos, Vector3.zero, Vector3.up * currentRing.RotationOffset);

                for (int e = 0; e < currentRing.EngineCount; e++)
                {
                    pos = RotatePointAroundPivot(pos, Vector3.zero, Vector3.up * rotateBy);

                    string nodeID = $"Cluster#{i}_Engine#{e}";
                    string NodeSymmetryGroupID = string.Empty;
                    if (currentRing.EngineCount > 1)
                        NodeSymmetryGroupID = $"Cluster#{i}";

                    AttachNodeDefinition def = new()
                    {
                        position = pos,
                        nodeID = nodeID,
                        NodeSymmetryGroupID = NodeSymmetryGroupID,
                        size = Sizes.size,
                        visualSize = Sizes.visualSize,
                        isRigid = false,
                        nodeType = KSP.Sim.AttachNodeType.Stack,
                        attachMethod = AttachNodeMethod.FIXED_JOINT,
                        IsMultiJoint = true,
                        MultiJointMaxJoint = 3,
                        MultiJointRadiusOffset = 0.63f,
                        angularStrengthMultiplier = 1,
                        orientation = new Vector3d(0, -1, 0),
                        isResourceCrossfeed = true,
                    };
                    currentDefs.Add(def);
                }
                ringDefs.Add(currentDefs);
            }
            return ringDefs;
        }

        internal void AddCluster(ClusterRing value)
        {
            value.SetSizeLimit(Diameter);
            Rings.Add(value);
        }
        internal void RemoveCluster(ClusterRing value)
        {
            Rings.Remove(value);
        }


        [Serializable]
        public class ClusterRing
        {
            private Guid _id;
            [NonSerialized]
            public int Min, Max;
            public int EngineCount;
            public float HeightOffset;
            public float PositionOffset;
            public float RotationOffset;
            public EngineSlot EngineSlot;

            public ClusterRing()
            {
                _id = Guid.NewGuid();
                Min = 1;
                Max = 20;
                EngineCount = 1;
                HeightOffset = 0;
                PositionOffset = 0;
                RotationOffset = 0;
                EngineSlot = new EngineSlot(1.25f);
                _size = new SizesUI(1.25f);
            }
            public ClusterRing(float engineDiameter) : this()
            {
                EngineSlot = new EngineSlot(engineDiameter);
                _size = new SizesUI(engineDiameter);
            }
            public ClusterRing(float engineDiameter, int EngineCount) : this(engineDiameter)
            {
                EngineSlot = new EngineSlot(engineDiameter);
                this.EngineCount = EngineCount;
            }
            public ClusterRing(float engineDiameter, int EngineCount, int Min, int Max) : this(engineDiameter)
            {
                EngineSlot = new EngineSlot(engineDiameter);
                this.Min = Min;
                this.Max = Max;
                EngineCount = Mathf.Clamp(EngineCount, Min, Max);
                this.EngineCount = EngineCount;
            }

            public void SetSizeLimit(float diameter)
            {
                int i = 0;
                foreach(var size in SizesUI.Sizes)
                {
                    if(diameter == size)
                    {
                        _size.SetLimit(i);
                        break;
                    }
                    i++;
                }
            }

            [NonSerialized]
            public bool _showGUI;
            SizesUI _size;
            public bool DrawGUI()
            {
                bool changed = false;
                int EngineCount = this.EngineCount;
                GUILayout.Label($"{EngineCount} Engines");
                EngineCount = (int)GUILayout.HorizontalSlider(EngineCount, Min, Max);
                if (EngineCount != this.EngineCount)
                {
                    changed = true;
                    this.EngineCount = EngineCount;
                }
                if (_size.DrawGUI())
                {
                    changed = true;
                }

                float HeightOffset = this.HeightOffset,
                    RotationOffset = this.RotationOffset,
                    PositionOffset = this.PositionOffset;

                GUILayout.Label($"Height: {HeightOffset}");
                HeightOffset = GUILayout.HorizontalSlider(HeightOffset, -1f, 1f);
                if (HeightOffset != this.HeightOffset)
                {
                    changed = true;
                    HeightOffset = (float)Math.Round(HeightOffset, 2);
                    this.HeightOffset = HeightOffset;
                }

                float rotMinMax = 360f / EngineCount;

                GUILayout.Label($"Rotation: {RotationOffset}");
                RotationOffset = GUILayout.HorizontalSlider(RotationOffset, -rotMinMax, rotMinMax);
                if (RotationOffset != this.RotationOffset)
                {
                    changed = true;
                    RotationOffset = (float)Math.Round(RotationOffset, 2);
                    this.RotationOffset = RotationOffset;
                }

                GUILayout.Label($"Position: {PositionOffset}");
                PositionOffset = GUILayout.HorizontalSlider(PositionOffset, -1f, 1f);
                if (PositionOffset != this.PositionOffset)
                {
                    changed = true;
                    PositionOffset = (float)Math.Round(PositionOffset, 2);
                    this.PositionOffset = PositionOffset;
                }

                return changed;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;
                if (obj.GetType() != GetType())
                    return false;
                ClusterRing otherRing = (ClusterRing)obj;
                return Equals(otherRing);
            }
            public bool Equals(ClusterRing other)
            {
                return _id == other._id;
            }

            public override int GetHashCode()
            {
                return _id.GetHashCode();
            }
        }
        [Serializable]
        public struct EngineSlot
        {
            public float Diameter;
            [KSPDefinition]
            public float Radius => Diameter / 2f;
            [KSPDefinition]
            public float NozzleDiameter => Diameter * NozzleToEngineRatio;
            [KSPDefinition]
            public float NozzleRadius => NozzleDiameter / 2f;
            public float NozzleToEngineRatio;

            public EngineSlot()
            {
                NozzleToEngineRatio = .75f;
            }
            public EngineSlot(float Diameter)
            {
                if (Diameter <= 0)
                {
                    throw new ArgumentOutOfRangeException("Diameter should be greater than 0!");
                }

                if (!SizesUI.Sizes.Contains(Diameter))
                    SORRYLog.Warning($"Size {Diameter} is not a standard size!");
                this.Diameter = Diameter;
                NozzleToEngineRatio = .75f;
            }
        }
    }
}