using KSP.Modules;
using KSP.Sim;
using KSP.Sim.impl;
using System;

namespace SORRY.Modules
{
    internal class PartComponentModule_ProceduralEngineCover : PartComponentModule
    {
        public override Type PartBehaviourModuleType => typeof(Module_ProceduralEngineCover);
        private Data_ProceduralEngineCover _dataProceduraEngineCover;

        public override void OnStart(double universalTime)
        {
            base.OnStart(universalTime);

            if (!this.DataModules.TryGetByType<Data_ProceduralEngineCover>(out this._dataProceduraEngineCover))
            {
                SORRYLog.Error("Unable to find a Data_Fairing in the PartComponentModule for " + base.Part.PartName);
                return;
            }

            foreach(var ringDefs in _dataProceduraEngineCover.GetRingsDefinitions())
            {
                foreach(var ringDef in ringDefs)
                {
                    if (base.Part.TryGetAttachment(ringDef.nodeID, out var node))
                    {
                        node.IsDynamic = true;//This is needed so the node is saved in the inflight save... IsDynamic is not set on AttachNodeData.LoadFromDefinition()
                    }
                    else
                    {
                        SORRYLog.Warning($"Somehow part {this.Part.Name} did not have node {ringDef.nodeID}. Creating one");
                        AttachNodeData attachNodeData = new(ringDef.nodeID, this.Part);
                        attachNodeData.LoadFromDefinition(ringDef);
                        attachNodeData.IsDynamic = true;
                        this.Part.AddNodeAttachment(attachNodeData);
                    }
                }
            }
        }
    }
}