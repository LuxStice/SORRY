using System;
using KSP.Sim;
using KSP.Sim.Definitions;
using UnityEngine;
using UnityEngine.Serialization;

namespace SORRY.Modules
{
	[Serializable]
	public class Data_DeployableControlSurface : ModuleData
	{
		public override Type ModuleType
		{
			get
			{
				return typeof(Module_DeployableControlSurface);
			}
		}

		[LocalizedField("Deploy Gridfins")]
		[KSPState(CopyToSymmetrySet = true)]
		[Tooltip("Current Control Surface State")]
		public ModuleProperty<bool> IsDeployed = new ModuleProperty<bool>(false, false);
	}
}
