using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MagicLightProbes
{
    [CustomEditor(typeof(MLPVolume))]
    public class MLPVolumeEditor : Editor
    {
        BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();

        public override void OnInspectorGUI()
        {
            MLPVolume mlpVolume = (MLPVolume)target;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();

            GUILayout.Label("Sub-Volumes Count", GUILayout.MinWidth(200));
            GUILayout.Label(mlpVolume.parentRootComponent.subVolumesDivided.Count.ToString());

            GUILayout.EndHorizontal();

            if (mlpVolume.parentRootComponent.tooManySubVolumes)
            {
                GUILayout.BeginHorizontal();

                EditorGUILayout.HelpBox("The main volume contains too many sub-volumes. \r\n\r\n" +
                        "What can you do: \r\n" +
                        " - Increase the \"Probes Count Limit\"\r\n" +
                        " - Change the spacing settings (\"Probe Spacing\" or \"Corners Detection Threshold\") \r\n" +
                        " - Reduce volume sizes)", MessageType.Error);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void OnSceneGUI()
        {
            MLPVolume mlpVolume = (MLPVolume)target;

            EventType currentEvent = Event.current.type;

            if (currentEvent == EventType.MouseUp)
            {
                mlpVolume.parentRootComponent.changed = true;
            }

            boxBoundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y | PrimitiveBoundsHandle.Axes.Z;
            boxBoundsHandle.wireframeColor = Color.yellow;
            boxBoundsHandle.center = mlpVolume.transform.position;
            boxBoundsHandle.size = mlpVolume.transform.localScale;

            //Handles.matrix = mlpVolume.transform.localToWorldMatrix;

            EditorGUI.BeginChangeCheck();            

            boxBoundsHandle.DrawHandle();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mlpVolume, "MLP Change Bounds");

                mlpVolume.transform.position = boxBoundsHandle.center;
                mlpVolume.transform.localScale = boxBoundsHandle.size;
            }            
        }
    }
}
