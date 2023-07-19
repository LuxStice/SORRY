using System;
using KSP.Sim.impl;

namespace SORRY.Modules
{
	public class PartComponentModule_DeployableControlSurface : PartComponentModule_ControlSurface
	{
        private Data_DeployableControlSurface _dataProceduraEngineCover;

        public override Type PartBehaviourModuleType
		{
			get
			{
				return typeof(Module_DeployableControlSurface);
			}
		}

		public override void OnStart(double universalTime)
        {
            if (!this.DataModules.TryGetByType<Data_DeployableControlSurface>(out this._dataProceduraEngineCover))
            {
                SORRYLog.Error("Unable to find a Data_Fairing in the PartComponentModule for " + base.Part.PartName);
                return;
            }
        }

		public override void OnUpdate(double universalTime, double deltaUniversalTime)
		{
		}
	}
}
