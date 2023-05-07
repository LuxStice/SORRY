using System;
using I2.Loc;
using KSP.IO;
using KSP.Modules;
using KSP.Sim.Definitions;
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
            animator = GetComponentInChildren<Animator>(true);
            base.AddActionGroupAction(new Action<bool>(this.SetControlSurfaceActiveState), KSP.Sim.KSPActionGroup.Brakes, "Toggle Gridfin", this._dataDeployableControlSurface.IsDeployed);
			base.AddActionGroupAction(new Action(this.SetControlSurfaceActiveStateOn), KSP.Sim.KSPActionGroup.None, "Deploy Gridfin");
			base.AddActionGroupAction(new Action(this.SetControlSurfaceActiveStateOff), KSP.Sim.KSPActionGroup.None, "Retract Gridfin");
			_dataDeployableControlSurface.IsDeployed.OnChanged += OnDeployedStateChanged;

            this.UpdatePAMControlVisibility();
			SetControlSurfaceActiveState(_dataDeployableControlSurface.IsDeployed.GetValue());
        }
        public override void AddDataModules()
        {
			base.AddDataModules(); 
			this._dataDeployableControlSurface ??= new Data_DeployableControlSurface();
            this.DataModules.TryAddUnique<Data_DeployableControlSurface>(this._dataDeployableControlSurface, out this._dataDeployableControlSurface);
        }

        public override void CtrlSurfaceUpdate(Vector3 vel, float deltaTime)
		{
			if(animator.GetBool("Deployed") != _dataDeployableControlSurface.IsDeployed.storedValue)
            {
                this.animator.SetBool("Deployed", _dataDeployableControlSurface.IsDeployed.storedValue);
            }
			if (this._dataDeployableControlSurface.IsDeployed.GetValue())
			{
				base.CtrlSurfaceUpdate(vel, deltaTime);
			}
		}

		private void SetDragCubes(bool deployed)
		{
			this._dataDrag.SetCubeWeight("Deployed", deployed ? 1f : 0f);
			//this._dataDrag.SetCubeWeight("Retracted", 0f);
		}

		public override string GetModuleDisplayName()
		{
			return LocalizationManager.GetTranslation("PartModules/ControlSurface/Name", true, 0, true, false, null, null, true);
		}

		private void OnDeployedStateChanged()
        {
			bool newState = _dataDeployableControlSurface.IsDeployed.GetValue();
            this.SetDragCubes(newState);
			if (!newState)
                _ctrlSurface.localRotation = _neutral * Quaternion.AngleAxis(0, _rotationVector);
            //this.animator.SetBool("Deployed", newState);
        }

		private void SetControlSurfaceActiveState(bool newState)
		{
			_dataDeployableControlSurface.IsDeployed.SetValue(newState);
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
            bool isAdvancedShown = dataCtrlSurface.IsAdvancedSettingsShown.GetValue();
			bool deployed = _dataDeployableControlSurface.IsDeployed.GetValue();

            _dataDeployableControlSurface.SetVisible(_dataDeployableControlSurface.IsDeployed, dataCtrlSurface.IsCtrlSurfaceActive);
            dataCtrlSurface.SetVisible(dataCtrlSurface.InvertControl, dataCtrlSurface.IsCtrlSurfaceActive);
            dataCtrlSurface.SetVisible(dataCtrlSurface.IsAdvancedSettingsShown, dataCtrlSurface.IsCtrlSurfaceActive);
            dataCtrlSurface.SetVisible(dataCtrlSurface.EnablePitch, dataCtrlSurface.IsCtrlSurfaceActive && isAdvancedShown);
            dataCtrlSurface.SetVisible(dataCtrlSurface.EnableYaw, dataCtrlSurface.IsCtrlSurfaceActive && isAdvancedShown);
            dataCtrlSurface.SetVisible(dataCtrlSurface.EnableRoll, dataCtrlSurface.IsCtrlSurfaceActive && isAdvancedShown);
            dataCtrlSurface.SetVisible(dataCtrlSurface.AuthorityLimiter, dataCtrlSurface.IsCtrlSurfaceActive && isAdvancedShown);
            dataCtrlSurface.SetVisible(dataCtrlSurface.LiftDragRatioParent, PartBackingMode == PartBehaviourModule.PartBackingModes.Flight);
            dataCtrlSurface.SetVisible(dataCtrlSurface.AoA, true);

			dataCtrlSurface.SetVisible(dataCtrlSurface.Deploy, false);
			dataCtrlSurface.SetVisible(dataCtrlSurface.DeployAngle, false);
        }

		public Animator animator;
		[SerializeField]
		protected Data_DeployableControlSurface _dataDeployableControlSurface = new Data_DeployableControlSurface();
	}
}
