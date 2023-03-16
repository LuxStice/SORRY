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

		[LocalizedField("SORRY/DCS/DEBUG/IsDeployed")]
		[KSPState(CopyToSymmetrySet = true)]
		[FormerlySerializedAs("isDeployed")]
		[Tooltip("Current Control Surface State")]
		public ModuleProperty<bool> IsDeployed = new ModuleProperty<bool>(false, true);
	}
}
