using System;
using I2.Loc;
using KSP.IO;
using KSP.Modules;
using UnityEngine;

namespace SORRY.Modules
{
	[DisallowMultipleComponent]
	public class Module_DeployableControlSurface : Module_ControlSurface
	{
		public override Type PartComponentModuleType
		{
			get
			{
				return typeof(PartComponentModule_DeployableControlSurface);
			}
		}


		public override void OnInitialize()
		{
			base.OnInitialize();
			base.AddActionGroupAction(new Action<bool>(this.SetControlSurfaceActiveState), KSP.Sim.KSPActionGroup.Brakes, "Toggle Control Surface", this.dataDeployableControlSurface.IsDeployed);
			base.AddActionGroupAction(new Action(this.SetControlSurfaceActiveStateOn), KSP.Sim.KSPActionGroup.None, "Activate Control Surface");
			base.AddActionGroupAction(new Action(this.SetControlSurfaceActiveStateOff), KSP.Sim.KSPActionGroup.None, "Deactivate Control Surface");
			this.UpdatePAMControlVisibility();
        }
        public override void AddDataModules()
        {
            base.AddDataModules();
            if (this.DataModules.TryGetByType<Data_DeployableControlSurface>(out this.dataDeployableControlSurface))
            {
                return;
            }
            this.dataDeployableControlSurface = new Data_DeployableControlSurface();
            this.DataModules.TryAddUnique<Data_DeployableControlSurface>(this.dataDeployableControlSurface, out this.dataDeployableControlSurface);
        }

        public override void CtrlSurfaceUpdate(Vector3 vel, float deltaTime)
		{
			if (this.dataDeployableControlSurface.IsDeployed.GetValue())
			{
				base.CtrlSurfaceUpdate(vel, deltaTime);
			}
		}

		private void SetDragCubes(bool deployed)
		{
			this._dataDrag.SetCubeWeight("Deployed", deployed ? 1f : 0f);
			this._dataDrag.SetCubeWeight("Retracted", deployed ? 0f : 1f);
		}

		public override string GetModuleDisplayName()
		{
			return LocalizationManager.GetTranslation("PartModules/ControlSurface/Name", true, 0, true, false, null, null, true);
		}

		private void SetControlSurfaceActiveState(bool newState)
		{
			this.animator.SetBool("Deployed", newState);
			this.dataDeployableControlSurface.IsDeployed.SetValue(newState);
			this.SetDragCubes(newState);
		}
		private void SetControlSurfaceActiveStateOn()
		{
			this.SetControlSurfaceActiveState(true);
		}
		private void SetControlSurfaceActiveStateOff()
		{
			this.SetControlSurfaceActiveState(false);
		}

		public override void UpdatePAMControlVisibility()
		{
			base.UpdatePAMControlVisibility();
			//this.dataDeployableControlSurface.SetVisible(this.dataDeployableControlSurface.IsDeployed, this.dataCtrlSurface.IsCtrlSurfaceActive);
			this.dataDeployableControlSurface.SetLabel(dataDeployableControlSurface.IsDeployed, "Debug/IsDeployed");
		}

		public Animator animator;
		[SerializeField]
		protected Data_DeployableControlSurface dataDeployableControlSurface;
	}
}
