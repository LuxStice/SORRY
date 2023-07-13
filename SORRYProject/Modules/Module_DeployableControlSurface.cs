using System;
using I2.Loc;
using KSP.IO;
using KSP.Modules;
using KSP.Sim.Definitions;
using UnityEngine;
using static SORRY.SORRYLog;

namespace SORRY.Modules
{
	[DisallowMultipleComponent]
    public class Module_DeployableControlSurface : Module_ControlSurface
	{
		private Module_Deployable deployable;
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

            UpdatePAMControlVisibility();

            AddActionGroupAction(SetControlSurfaceActiveState, KSP.Sim.KSPActionGroup.Brakes, "Toggle Gridfin", _dataDeployableControlSurface.IsDeployed);
            AddActionGroupAction(SetControlSurfaceActiveStateOn, KSP.Sim.KSPActionGroup.None, "Deploy Gridfin");
            AddActionGroupAction(SetControlSurfaceActiveStateOff, KSP.Sim.KSPActionGroup.None, "Retract Gridfin");
			SetControlSurfaceInvertControlState(true);

            _dataDeployableControlSurface.IsDeployed.OnChangedValue += OnDeployedStateChanged;
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
			base.CtrlSurfaceUpdate(vel, deltaTime);
		}

		private void SetDragCubes(bool deployed)
		{
			this._dataDrag.SetCubeWeight("Deployed", deployed ? 1f : 0f);
			this._dataDrag.SetCubeWeight("Retracted", !deployed ? 1f : 0f);
		}

		public override string GetModuleDisplayName()
		{
			return LocalizationManager.GetTranslation("PartModules/ControlSurface/Name", true, 0, true, false, null, null, true);
		}

		private void OnDeployedStateChanged(bool deployed)
        {
			isDirty = true;
        }
		private bool isDirty;
		void Update()
		{
			if (isDirty)
            {
                this.SetDragCubes(IsDeployed);
                if (!IsDeployed)
                {
                    _ctrlSurface.localRotation = _neutral * Quaternion.AngleAxis(0, _rotationVector);
                }
                if (this.animator is not null)
                    this.animator.SetBool("Deployed", IsDeployed);

                if (SDebug.ShowDebug)
                {
                    if (_dataDeployableControlSurface.TryGetProperty<string>(DEBUG_CONTEXT_KEY, out var moduleProperty))
                    {
                        moduleProperty.SetValue(IsDeployed ? "Deployed" : "Retracted");
                    }
                }
				isDirty = false;
            }
		}

		public bool IsDeployed
		{
			get => _dataDeployableControlSurface.IsDeployed.GetValue();
			set
			{
				if(value != IsDeployed)
				{
					_dataDeployableControlSurface.IsDeployed.SetValue(value);
				}
			}
		}
		private void SetControlSurfaceActiveState(bool newState)
        {
            IsDeployed = newState;
			dataCtrlSurface.AllowControl = newState;
			UpdatePAMControlVisibility();
		}
		private void ToggleDeployedState()
		{
			SetControlSurfaceActiveState(!IsDeployed);
		}
		private void SetControlSurfaceActiveStateOn()
        {
            this.SetControlSurfaceActiveState(true);
		}
		private void SetControlSurfaceActiveStateOff()
        {
            this.SetControlSurfaceActiveState(false);
		}

		private const string DEBUG_CONTEXT_KEY = "debug-DCSState";


        public override void UpdatePAMControlVisibility()
        {
            _dataDeployableControlSurface.SetVisible(_dataDeployableControlSurface.IsDeployed, dataCtrlSurface.IsCtrlSurfaceActive);
            if (SDebug.ShowDebug)
            {
                var moduleProperty = new ModuleProperty<string>("unitialized", false);
                moduleProperty.ContextKey = DEBUG_CONTEXT_KEY;
                moduleProperty.SetValue(_dataDeployableControlSurface.IsDeployed.GetValue() ? "Deployed" : "Retracted");
                _dataDeployableControlSurface.AddProperty("Current deploy state", moduleProperty);
                _dataDeployableControlSurface.SetToStringDelegate(moduleProperty, ToStringDelegate);
                _dataDeployableControlSurface.SetVisible(moduleProperty, SDebug.ShowDebug);
            }
            base.UpdatePAMControlVisibility();


        }

		string ToStringDelegate(object obj)
		{
			return IsDeployed ? "Deployed" : "Retracted";
        }

		public Animator animator;
		[SerializeField]
		protected Data_DeployableControlSurface _dataDeployableControlSurface = new Data_DeployableControlSurface();
	}
}
