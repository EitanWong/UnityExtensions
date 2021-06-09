using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FlyingWormConsole3;
using Hub.Editor.Scripts;
using Hub.Editor.Scripts.Base;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Hub.Editor.Scripts.Pages
{
    public class HubScenePage : Hub_PageBase
    {
        private string[] allScenesId;
        //private GUIStyle AM_MixerHeader2_Style = new GUIStyle("AM MixerHeader2");

        private void OnEnable()
        {
            pageName = "场景管理";
            layer = -3;
        }

        protected override void OnGUI()
        {
            allScenesId = AssetDatabase.FindAssets("t:scene");
            foreach (var guid in allScenesId)
            {
                DrawSceneIcon(guid);
            }
            
        }

        private void DrawSceneIcon(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var fileName = Path.GetFileName(path);

            var iconContsent = Hub_Styles.Scene;
            iconContsent.text = fileName;
            if (GUILayout.Button(iconContsent, Hub_Styles.OL_Ping))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(path);
            }
        }
    }
}