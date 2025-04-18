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
        private const string SectionIdentifier = "CruiseControlConfig";

        private const string DefaultCustomData = "[" + SectionIdentifier + "]\n" +
                                                 "[" + LogDisplaySectionIdentifier + "]\n" +
                                                 "screen number = 0";

        private const string LogDisplaySectionIdentifier = "CCLogConfig";
        private const string ToggleSectionIdentifier = "CCToggle";

        private readonly List<ToggleBlock> blocksToToggle = new List<ToggleBlock>();

        private readonly MyIni configDataParser = new MyIni();

        private readonly List<IMyTextSurface> outputSurfaces = new List<IMyTextSurface>();

        private readonly List<Wheel> wheels = new List<Wheel>();

        public Program()
        {
            if (Me.CustomData == String.Empty)
            {
                Me.CustomData = DefaultCustomData;
            }

            MyIniParseResult result;
            if (!configDataParser.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            InitializeLogScreens();
            InitializeToggleBlocks();
            InitializeWheels();
        }

        private void InitializeWheels()
        {
            List<IMyMotorSuspension> tempWheels = new List<IMyMotorSuspension>();
            GridTerminalSystem.GetBlocksOfType(tempWheels,
                wheel => MyIni.HasSection(wheel.CustomData, SectionIdentifier));
            MyIni wheelDataParser = new MyIni();
            foreach (IMyMotorSuspension wheel in tempWheels)
            {
                MyIniParseResult wheelDataResult;
                if (!wheelDataParser.TryParse(wheel.CustomData, out wheelDataResult))
                {
                    throw new Exception(wheelDataResult.ToString());
                }

                bool reversePropulsion = wheelDataParser.Get(SectionIdentifier, "reverse propulsion").ToBoolean();
                wheels.Add(new Wheel(wheel, reversePropulsion));
            }

            Echo("collected " + wheels.Count + " wheels.");
        }

        private void InitializeToggleBlocks()
        {
            List<IMyFunctionalBlock> functionalToggleBlocks = new List<IMyFunctionalBlock>();
            GridTerminalSystem.GetBlocksOfType(functionalToggleBlocks,
                block => MyIni.HasSection(block.CustomData, ToggleSectionIdentifier));
            MyIni blockConfigParser = new MyIni();
            foreach (IMyFunctionalBlock block in functionalToggleBlocks)
            {
                MyIniParseResult blockConfig;
                if (!blockConfigParser.TryParse(block.CustomData, out blockConfig))
                {
                    throw new Exception(blockConfig.ToString());
                }

                bool invertToggle = blockConfigParser.Get(ToggleSectionIdentifier, "invert toggle").ToBoolean();
                blocksToToggle.Add(new ToggleBlock(block, invertToggle));
            }

            Echo($"collected {functionalToggleBlocks.Count} blocks to toggle.");
        }

        private void InitializeLogScreens()
        {
            List<IMyTerminalBlock> outputBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(outputBlocks,
                block => MyIni.HasSection(block.CustomData, LogDisplaySectionIdentifier));
            int skippedscreens = 0;
            MyIni blockConfigParser = new MyIni();
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
                    MyIniParseResult configParseResult;
                    if (!blockConfigParser.TryParse(block.CustomData, out configParseResult))
                    {
                        throw new Exception(configParseResult.ToString());
                    }

                    int surfaceNumber = blockConfigParser.Get(LogDisplaySectionIdentifier, "screen number")
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
                Echo = WriteToScreens;
            }

            Echo("Screen setup complete. Skipped " + skippedscreens + " screens");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument.ToLower())
            {
                case "start":
                    Start();
                    break;
                case "stop":
                    Stop();
                    break;
            }
        }

        private void Start()
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.StartCruise();
            }

            foreach (ToggleBlock block in blocksToToggle)
            {
                block.Enable();
            }
        }

        private void Stop()
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.StopCruise();
            }

            foreach (ToggleBlock block in blocksToToggle)
            {
                block.Disable();
            }
        }

        private void WriteToScreens(string text)
        {
            foreach (IMyTextSurface surface in outputSurfaces)
            {
                surface.WriteText(text + "\n", true);
            }
        }
    }
}