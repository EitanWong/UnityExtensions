namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static class TransformProEditorLoader
    {
        static TransformProEditorLoader()
        {
#pragma warning disable 618
            EditorApplication.playmodeStateChanged += TransformProEditorLoader.Load;
#pragma warning restore 618
            TransformProEditorLoader.Load();
        }

        private static void Load()
        {
            /*
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                TransformProEditorGadgets.Unload();
                return;
            }
            */

            TransformProStyles.Initialise();
            TransformProPreferences.Load();

            if (TransformProEditorGadgets.Instance == null)
            {
                TransformProEditorGadgets.Create();
            }

            if (TransformProEditorGadgets.Instance == null)
            {
                Debug.LogWarning("[<color=red>TransformPro</color>] Could not create gadget manager.");
                return;
            }

            //EditorApplication.delayCall += TransformProEditorGadgets.Instance.Setup;
            TransformProEditorGadgets.Instance.Setup();
        }
    }
}
