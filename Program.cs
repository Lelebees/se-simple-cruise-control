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
                                               "[" + OutputTextPanelSectionIdentifier + "]\n" +
                                               "screen number = 0";

        private const string OutputTextPanelSectionIdentifier = "SimpleCruiseControlTextPanel";


        private List<IMyLightingBlock> warningLights = new List<IMyLightingBlock>();

        private List<IMyShipDrill> drills = new List<IMyShipDrill>();

        private List<Wheel> wheels = new List<Wheel>();

        private MyIni configDataParser = new MyIni();

        private List<IMyTextSurface> outputSurfaces = new List<IMyTextSurface>();

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

            #region initialize output screens

            List<IMyTerminalBlock> outputBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(outputBlocks,
                block => MyIni.HasSection(block.CustomData, OutputTextPanelSectionIdentifier));
            int skippedscreens = 0;
            foreach (IMyTerminalBlock block in outputBlocks)
            {
                IMyTextSurface outputSurface;
                IMyTextSurface surface = block as IMyTextSurface;
                if (surface != null)
                {
                    outputSurface = surface;
                }
                else if (block is IMyTextSurfaceProvider)
                {
                    MyIni blockConfigParser = new MyIni();
                    MyIniParseResult configParseResult;
                    if (!blockConfigParser.TryParse(block.CustomData, out configParseResult))
                    {
                        throw new Exception(configParseResult.ToString());
                    }

                    int surfaceNumber = blockConfigParser.Get(OutputTextPanelSectionIdentifier, "screen number")
                        .ToInt32();
                    outputSurface = ((IMyTextSurfaceProvider)block).GetSurface(surfaceNumber);
                }
                else
                {
                    skippedscreens++;
                    continue;
                }

                outputSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                outputSurfaces.Add(outputSurface);
            }

            if (outputSurfaces.Count > 0)
            {
                Echo = writeToScreens;
            }

            Echo("Screen setup complete. Skipped " + skippedscreens + " screens");

            #endregion

            GridTerminalSystem.GetBlocksOfType(warningLights,
                light => MyIni.HasSection(light.CustomData, SectionIdentifier));
            Echo("collected " + warningLights.Count + " lights.");
            GridTerminalSystem.GetBlocksOfType(drills,
                drill => MyIni.HasSection(drill.CustomData, SectionIdentifier));
            Echo("collected " + drills.Count + " drills.");
            List<IMyMotorSuspension> tempWheels = new List<IMyMotorSuspension>();
            GridTerminalSystem.GetBlocksOfType(tempWheels,
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
                wheels.Add(new Wheel(wheel, reversePropulsion));
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
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

        private void writeToScreens(string text)
        {
            foreach (IMyTextSurface surface in outputSurfaces)
            {
                surface.WriteText(text + "\n", true);
            }
        }
    }
}