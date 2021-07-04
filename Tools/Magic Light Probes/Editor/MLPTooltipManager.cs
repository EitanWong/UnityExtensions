using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [InitializeOnLoad]
    public static class MLPTooltipManager
    {
        static MLPTooltipManager()
        {
            MainComponent.GenerateTooltips();
            MLPLightComponent.GenerateTooltips();
        }

        public static class MainComponent
        {
            public enum Tabs
            {
                BasicParameters,
                CullingOptions,
                Debug
            }

            private static Dictionary<string, string> mainComponentBasicParameters;
            private static Dictionary<string, string> mainComponentCullingOptions;
            private static Dictionary<string, string> mainComponentDebug;

            public static void GenerateTooltips()
            {
                mainComponentBasicParameters = new Dictionary<string, string>();
                mainComponentCullingOptions = new Dictionary<string, string>();
                mainComponentDebug = new Dictionary<string, string>();

                #region Basic Parameters

                mainComponentBasicParameters.Add("Workflow", "This option allows you to select a workflow. \r\n\r\n" +
                    "Simple - In this workflow, the position of the light sources and their properties are not taken " +
                    "into account. You only need to add volume to the scene, make the minimum settings and click the " +
                    "calculation button. This allows you to calculate faster and does not require additional configuration. \r\n\r\n" +
                    "Advanced - This workflow requires the addition of the \"MLP Light\" component to all light sources that must be " +
                    "taken into account during the calculation.This allows you to achieve more accurate results, " +
                    "but also requires fine tuning of each light source.");
                mainComponentBasicParameters.Add("Use Dynamic Density", "Use dynamic volume density. (Experemental) \r\n\r\n" +
                    "If checked, then the density of the probes will automatically increase in areas where clusters of objects are located.\r\n\r\n" +
                    "This mode is suitable for large open scenes, on indoor scenes it is better to use manual placement.");
                mainComponentBasicParameters.Add("Max Value", "The maximum value of the upper limit of the parameter \"Probes Spacing\".");
                mainComponentBasicParameters.Add("Probes Spacing", "The lower spacing, the longer the calculation will take. Typically, " +
                    "a value of 0.4 is sufficient.");
                mainComponentBasicParameters.Add("Corners Detection Threshold", "The accuracy of determining intersections of geometry.");
                mainComponentBasicParameters.Add("Corner Probe Spacing", "Distance between probes installed at intersections of geometry.");
                mainComponentBasicParameters.Add("Distance From Geometry", "The distance by which the probes will be moved away from the geometry.");
                mainComponentBasicParameters.Add("Probes Count Limit", "If your volume is too large, it will automatically be divided into parts for " +
                    "faster calculation and memory saving. If the number of probes in the volume exceeds this value, then it will be divided.");
                mainComponentBasicParameters.Add("Ground And Floor Objects", "Use these options to select objects that border the scene below. " +
                    "No probes will be installed below such objects. It is assumed that in such areas the player cannot be and lighting " +
                    "is not needed there.");
                mainComponentBasicParameters.Add("Use Volume Bottom", "If checked, then instead of objects, the down side of the volume will be used.");

                #endregion

                #region Culling Options

                mainComponentCullingOptions.Add("Exclude Lights From Process", "The indicated light sources will be excluded from the calculation " +
                    "in this volume.");
                mainComponentCullingOptions.Add("Max Height Above Geometry", "All probes above the geometry whose height is higher than the " +
                    "specified value will be culled.");
                mainComponentCullingOptions.Add("Max Height Above Terrain", "All probes above the terrain whose height is higher than the specified " +
                    "value will be culled.");
                mainComponentCullingOptions.Add("Collision Detection Layers", "Selected layers will be taken into account in the calculation.");
                mainComponentCullingOptions.Add("Collision Detection Radius", "Probes in which geometry is detected in the specified radius will be " +
                    "excluded from the calculation.");
                mainComponentCullingOptions.Add("Consider Distance To Lights", "If checked, then the calculation will take into account the distance " +
                    "to the light sources.");
                mainComponentCullingOptions.Add("Cull By Color", "Activate the probe removal algorithm with equivalent color.");
                mainComponentCullingOptions.Add("Save Probes On Shading Borders", "Automatically keep probes at shading borders \r\n" +
                    "regardless of the set threshold.");
                mainComponentCullingOptions.Add("Filling Mode", "In Advanced workflow mode MLP can work in three different fill modes. " +
                    "Separate Filling, Vertical Duplicate, and Full Filling. \r\n\r\n" +
                    "In the \"Separate Filling\" mode, the volume will be divided into sub - volumes.This is a fully automatic mode that takes into account " +
                    "contrast areas(shadows, color changes, changes in light intensity, unlit areas, range and angles of light sources). \r\n" +
                    "In the \"Vertical Duplication\" mode, the probes will be located along the geometry and then duplicated to the specified height with the " +
                    "specified step.\r\n\r\n" +
                    "In the \"Full Filling\" mode, the probes will be located evenly throughout the volume.All probes inside the geometry will be automatically " +
                    "deleted.");
                mainComponentCullingOptions.Add("Light Intensity Threshold", "The accuracy of the culling of probes by light intensity.The lower accuracy, " +
                    "the greater the range of probes will be considered equivalent and will be culled.");
                mainComponentCullingOptions.Add("Color Threshold", "The threshold for determining the color equivalence of neighboring probes.");
                mainComponentCullingOptions.Add("Fill Equivalent Volume", "If checked, then the volume of culled probes according to the light intensity " +
                    "will be filled. \r\n" +
                    "Filling Rate - Density of filling the volume. 1 - full filling.");
                mainComponentCullingOptions.Add("Fill Unlit Volume", "If checked, the volume of unlit probes will be filled.\r\n" +
                    "Filling Rate - Density of filling the volume. 1 - full filling.");
                mainComponentCullingOptions.Add("Optimize For Mixed Lighting", "Check this option if your scene uses only mixed-mode lights. All parts of " +
                    "the volume that do not contain any geometry will be removed and this will increase the calculation speed.");
                mainComponentCullingOptions.Add("Try Prevent Light Leakage", "Check this option to enable an algorithm that attempts to " +
                    "automatically prevent lighting leaks from neighboring rooms. From probes that are on the other side of the wall.");
                mainComponentCullingOptions.Add("Free Volume Filling Rate", "Density of filling the free volume.\r\n" +
                    "1 - full filling.");
                    mainComponentCullingOptions.Add("Save Probes On Geometry Edges", "Forces probes to be kept at the edges of the geometry."); 

                #endregion

                #region Debug Options

                mainComponentDebug.Add("Bounds Dispaly Mode", "The display mode of the volume boundaries. Use this option to hide or show volume boundaries.");
                mainComponentDebug.Add("Enable Debug Mode", "Activate debug mode.\r\n\r\n" +
                    "In debug mode, you can configure each of the volume calculation passes in detail.In this mode, instead of adding probes to the " +
                    "target Light Probe Group, debugging objects will be added to the scene.");
                mainComponentDebug.Add("Debug Object Scale", "The scale of debugging objects.");

                #endregion
            }

            public static GUIContent GetParameter (string name, Tabs componentTab)
            {
                string tooltip = "";

                switch (componentTab)
                {
                    case Tabs.BasicParameters:
                        if (!mainComponentBasicParameters.TryGetValue(name, out tooltip))
                        {
                            tooltip = "No description for this parameter.";
                        }
                        break;
                    case Tabs.CullingOptions:
                        if (!mainComponentCullingOptions.TryGetValue(name, out tooltip))
                        {
                            tooltip = "No description for this parameter.";
                        }
                        break;
                    case Tabs.Debug:
                        if (!mainComponentDebug.TryGetValue(name, out tooltip))
                        {
                            tooltip = "No description for this parameter.";
                        }
                        break;
                }

                return new GUIContent(name, tooltip);
            }
        }

        public static class MLPLightComponent
        {
            public enum Tabs
            {
                BasicParameters,
                CullingOptions,
                Debug
            }

            private static Dictionary<string, string> mlpLightParameters;

            public static void GenerateTooltips()
            {
                mlpLightParameters = new Dictionary<string, string>();

                mlpLightParameters.Add("Calculation Type", "Types of volume calculation for a light source.\r\n\r\n" +
                    "Accurate Shadows - lighting probes will be installed at the borders of the shadows formed by the light source, " +
                    "these probes will not be taken into account at the culling passes.\r\n\r\n" +
                    "Light Intensity - only probes with an intensity difference greater than the range defined by the " +
                    "\"Light Intensity Threshold\" parameter will be left.");
                mlpLightParameters.Add("Range", "Light range.");
                mlpLightParameters.Add("Angle", "Light angle.");
                mlpLightParameters.Add("Use Source Parameters", "If checked, the settings will be copied from the source to which the component is attached.");
                mlpLightParameters.Add("Save Nearby Probes", "If checked, then the probes will be saved in the indicated radius. These probes will not be " +
                    "taken into account at the rejection stages.");
                mlpLightParameters.Add("Save Radius", "The radius in which the probes should be stored.");
                mlpLightParameters.Add("Save On Out Of Range", "If checked, the probes will be stored on the border of the light source range.");
                mlpLightParameters.Add("Accurate Trace", "If checked, then the illuminance of the probes in the range of the source will be calculated " +
                    "by several points that you can set yourself (the \"Add Point...\" button), otherwise, by one point located in the center of the source.");
            }

            public static GUIContent GetParameter(string name, Tabs componentTab)
            {
                string tooltip = "";

                if (!mlpLightParameters.TryGetValue(name, out tooltip))
                {
                    tooltip = "No description for this parameter.";
                }

                return new GUIContent(name, tooltip);
            }
        }
    }
}
