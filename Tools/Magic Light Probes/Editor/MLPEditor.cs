using System.IO;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [CustomEditor(typeof(MagicLightProbes)), CanEditMultipleObjects]
    public class MLPEditor : Editor
    {
        private Texture2D logoAsset;
        private byte[] textureData;
        private bool initialized;

        //private void Init ()
        //{
        //    string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Light Probes/Editor/Icons", SearchOption.AllDirectories);
        //    string[] str = Directory.GetFiles(directories[0]);

        //    textureData = File.ReadAllBytes(str[0]);

        //    logoAsset = new Texture2D(512, 512);
        //    logoAsset.LoadImage(textureData);

        //    initialized = true;
        //}
        
        public override void OnInspectorGUI()
        {            
            MagicLightProbes magicLightProbes = (MagicLightProbes)target;

            if (magicLightProbes.unloaded)
            {
                magicLightProbes.unloaded = false;

                EditorApplication.update -= magicLightProbes.CheckState;
                EditorApplication.update += magicLightProbes.CheckState;
            }

            if (!GeneralMethods.stylesInitialized)
            {
                GeneralMethods.InitStyles();
            }
            
            serializedObject.Update();
            GeneralMethods.MLPMainComponentEditor(magicLightProbes);            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(magicLightProbes);
        }

        //void OnSceneGUI()
        //{
        //    MagicLightProbes magicLightProbes = (MagicLightProbes)target;

        //    Handles.DrawSolidRectangleWithOutline(Tools.handleRect, Color.yellow, Color.green);            

        //    //magicLightProbes.transform.localScale = Handles.ScaleHandle(magicLightProbes.transform.localScale, magicLightProbes.transform.position, Quaternion.identity, 0.5f);
        //}
    }
}
