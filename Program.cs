using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        private const string SectionIdentifier = "SimpleCruiseControl";

        private const string CustomDataPrint = "[" + SectionIdentifier + "]\n" +
                                               "";

        private List<IMyLightingBlock> warningLights = new List<IMyLightingBlock>();

        private List<IMyShipDrill> drills = new List<IMyShipDrill>();

        private List<Wheel> wheels = new List<Wheel>();

        private MyIni configDataParser = new MyIni();


        public Program()
        {
            if (Me.CustomData == String.Empty)
            {
                Me.CustomData = CustomDataPrint;
            }

            MyIniParseResult result;
            if (!configDataParser.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            // Collect necessary blocks
            GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(warningLights,
                light => MyIni.HasSection(light.CustomData, SectionIdentifier));
            Echo("collected " + warningLights.Count + " lights.");
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drills,
                drill => MyIni.HasSection(drill.CustomData, SectionIdentifier));
            Echo("collected " + drills.Count + " drills.");
            List<IMyMotorSuspension> tempWheels = new List<IMyMotorSuspension>();
            GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(tempWheels,
                wheel => MyIni.HasSection(wheel.CustomData, SectionIdentifier));
            Echo("collected " + tempWheels.Count + " wheels.");
            foreach (IMyMotorSuspension wheel in tempWheels)
            {
                MyIni wheelDataParser = new MyIni();
                MyIniParseResult wheelDataResult;
                if (!wheelDataParser.TryParse(wheel.CustomData, out wheelDataResult))
                {
                    throw new Exception(wheelDataResult.ToString());
                }
                bool reversePropulsion = wheelDataParser.Get(SectionIdentifier, "reverse propulsion").ToBoolean();
                this.wheels.Add(new Wheel(wheel, reversePropulsion));
            }
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
            switch (argument.ToLower())
            {
                case "start":
                    StartDrills();
                    break;
                case "stop":
                    StopDrills();
                    break;
            }
        }

        private void StartDrills()
        {
            foreach (IMyShipDrill drill in drills)
            {
                drill.Enabled = true;
            }

            foreach (IMyLightingBlock light in warningLights)
            {
                light.Enabled = true;
            }

            foreach (Wheel wheel in wheels)
            {
                wheel.StartCruise();
            }
        }

        private void StopDrills()
        {
            foreach (IMyShipDrill drill in drills)
            {
                drill.Enabled = false;
            }

            foreach (IMyLightingBlock light in warningLights)
            {
                light.Enabled = false;
            }

            foreach (Wheel wheel in wheels)
            {
                wheel.StopCruise();
            }
        }
    }
}