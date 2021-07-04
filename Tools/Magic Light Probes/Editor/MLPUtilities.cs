using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    public static class MLPUtilites
    {
        public static bool CheckIfStatic (GameObject gameObject, out string errorMessage)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            bool isStatic = false;
            errorMessage = "";

#if UNITY_2019_2_OR_NEWER
            if ((flags & StaticEditorFlags.ContributeGI) != 0)
            {
                isStatic = true;
            }
            else
            {
                errorMessage = "The object you selected is not static. " +
                "Only static (marked with the \"Contribute GI\" flag) objects can be taken into account by the system.";
            }
#else
            if ((flags & StaticEditorFlags.LightmapStatic) != 0)
            {
                isStatic = true;
            }
            else
            {
                errorMessage = "The object you selected is not static. " +
                "Only static (marked with the \"Lightmap static\" flag) objects can be taken into account by the system.";
            }
#endif

            return isStatic;
        }

        public static bool CheckIfStatic(GameObject gameObject)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(Selection.activeGameObject);
            bool isStatic = false;

#if UNITY_2019_2_OR_NEWER
            if ((flags & StaticEditorFlags.ContributeGI) != 0)
            {
                isStatic = true;
            }
#else
            if ((flags & StaticEditorFlags.LightmapStatic) != 0)
            {
                isStatic = true;
            }
#endif

            return isStatic;
        }
    }
}
