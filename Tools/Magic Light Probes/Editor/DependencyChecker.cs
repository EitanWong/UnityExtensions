using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    public class DependencyChecker
    {
        private const string TerrainPackageDefine = "TERRAINPACKAGE_EXIST";
        private const string BakeryPackageDefine = "BAKERYPACKAGE_EXIST";
        private const string HDRPPackageDefine = "HDRPPACKAGE_EXIST";
        private static bool hasTerrainPackage;
        private static bool hasBakeryPackage;
        private static bool hasHDRPPackage;
        private static bool manual;

        [MenuItem("Tools/Magic Tools/Magic Light Probes/Check Dependencies", priority = 1)]
        private static void ManualCheck()
        {
            manual = true;
            Check();
        }

        [DidReloadScripts]
        private static void Check()
        {
            hasTerrainPackage = DoesTypeExist("Terrain");
            hasBakeryPackage = DoesTypeExist("ftRenderLightmap");
            hasHDRPPackage = DoesTypeExist("HDRenderPipelineAsset");

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            bool hasTerrainDefine = scriptingDefines.Contains(TerrainPackageDefine);
            bool hasBakeryDefine = scriptingDefines.Contains(BakeryPackageDefine);
            bool hasHDRPDefine = scriptingDefines.Contains(HDRPPackageDefine);

            // Terian Package Check

            if (hasTerrainPackage && !hasTerrainDefine)
            {
                if (EditorUtility.DisplayDialog("MLP Dependency Checker", "The \"Terrain\" package is installed in the project, \"Scripting Define Symbols\" will be automatically configured.", "OK"))
                {
                    AddDefine(TerrainPackageDefine, false);
                }
            }
            else if (!hasTerrainPackage && hasTerrainDefine)
            {
                if (EditorUtility.DisplayDialog("MLP Dependency Checker", "The \"Terrain\" package is not installed in the project, \"Scripting Define Symbols\" will be automatically configured.", "OK"))
                {
                    RemoveDefine(TerrainPackageDefine, false);
                }
            }

            // Bakery Package Check

            if (hasBakeryPackage && !hasBakeryDefine)
            {
                if (EditorUtility.DisplayDialog("MLP Dependency Checker", "The \"Bakery\" package is installed in the project, \"Scripting Define Symbols\" will be automatically configured.", "OK"))
                {
                    AddDefine(BakeryPackageDefine, false);
                }
            }
            else if (!hasBakeryPackage && hasBakeryDefine)
            {
                if (EditorUtility.DisplayDialog("MLP Dependency Checker", "The \"Bakery\" package is not installed in the project, \"Scripting Define Symbols\" will be automatically configured.", "OK"))
                {
                    RemoveDefine(BakeryPackageDefine, false);
                }
            }

            // HDRP Package Check

            if (hasHDRPPackage && !hasHDRPDefine)
            {
                if (EditorUtility.DisplayDialog("MLP Dependency Checker", "The \"HDRP\" package is installed in the project, \"Scripting Define Symbols\" will be automatically configured.", "OK"))
                {
                    AddDefine(HDRPPackageDefine, false);
                }
            }
            else if (!hasHDRPPackage && hasHDRPDefine)
            {
                if (EditorUtility.DisplayDialog("MLP Dependency Checker", "The \"HDRP\" package is not installed in the project, \"Scripting Define Symbols\" will be automatically configured.", "OK"))
                {
                    RemoveDefine(HDRPPackageDefine, false);
                }
            }

            if (manual)
            {
                manual = false;
                EditorUtility.DisplayDialog("MLP Dependency Checker", "Scripting Define Symbols configured.", "OK");
            }
        }

        public static bool DoesTypeExist(string className)
        {
            var foundType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from type in GetTypesSafe(assembly)
                             where type.Name == className
                             select type).FirstOrDefault();

            return foundType != null;
        }
        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            return types.Where(x => x != null);
        }

        static private void RemoveDefine(string define, bool processLockFile = true)
        {
            if (processLockFile)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Light Probes", SearchOption.AllDirectories);

                if (File.Exists(directories[0] + "/MLP_lock"))
                {
                    File.Delete(directories[0] + "/MLP_lock");
                }
            }

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            List<string> listDefines = scriptingDefines.ToList();
            listDefines.Remove(define);

            string newDefines = string.Join(";", listDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
        }

        static private void AddDefine(string define, bool processLockFile = true)
        {
            if (processLockFile)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Light Probes", SearchOption.AllDirectories);

                if (!File.Exists(directories[0] + "/MLP_lock"))
                {
                    File.Create(directories[0] + "/MLP_lock");
                }
            }

            string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            string[] scriptingDefines = scriptingDefine.Split(';');
            List<string> listDefines = scriptingDefines.ToList();
            listDefines.Add(define);

            string newDefines = string.Join(";", listDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
        }
    }
}
