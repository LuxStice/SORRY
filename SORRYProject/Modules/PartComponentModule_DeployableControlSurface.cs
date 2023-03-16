using System;
using KSP.Sim.impl;

namespace SORRY.Modules
{
	public class PartComponentModule_DeployableControlSurface : PartComponentModule_ControlSurface
	{
		public override Type PartBehaviourModuleType
		{
			get
			{
				return typeof(Module_DeployableControlSurface);
			}
		}

		public override void OnStart(double universalTime)
		{
		}

		public override void OnUpdate(double universalTime, double deltaUniversalTime)
		{
		}
	}
}
