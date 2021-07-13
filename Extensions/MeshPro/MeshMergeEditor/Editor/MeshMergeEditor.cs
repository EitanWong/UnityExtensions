using System;
using UnityEditor;
using UnityEngine;

namespace MeshMergeEditor
{
    public class MeshMergeEditor : Editor
    {
        [MenuItem("GameObject/合并网格 %^M", true, int.MaxValue / 2)]
        private static bool CheckIsNotPlaying() => Selection.gameObjects.Length > 1;


        /// <summary>
        /// 合并网格
        /// </summary>
        [MenuItem("GameObject/合并网格 %^M", false, int.MaxValue / 2)]
        public static void CombineMesh()
        {
            SceneView sceneView = null;
            if (SceneView.sceneViews != null && SceneView.sceneViews.Count > 0)
            {
                sceneView = (SceneView) SceneView.sceneViews[0];

                if (Selection.gameObjects.Length <= 1)
                {
                    if (sceneView)
                        sceneView.ShowNotification(new GUIContent("请选择\n两个以上的\n网格对象"), 1f);
                    return;
                }
            }
            else
            {
                EditorWindow.mouseOverWindow.ShowNotification(new GUIContent("请选择\n两个以上的\n网格对象"), 1f);
            }

            try
            {
                var selectTransforms = Selection.transforms;
                CombineInstance[] combineInstance = new CombineInstance[selectTransforms.Length];
                MeshRenderer first_Renderer = null;
                bool hasMeshFilter = false;
                for (int i = 0; i < selectTransforms.Length; i++)
                {
                    var meshFilter = selectTransforms[i].GetComponent<MeshFilter>();
                    if (meshFilter)
                    {
                        if (!hasMeshFilter)
                            hasMeshFilter = true;
                        combineInstance[i].mesh = meshFilter.sharedMesh;
                        combineInstance[i].transform = meshFilter.transform.localToWorldMatrix;
                        meshFilter.transform.gameObject.SetActive(false);
                    }

                    if (first_Renderer == null)
                    {
                        var meshRenderer = selectTransforms[i].GetComponent<MeshRenderer>();
                        if (meshRenderer)
                            first_Renderer = meshRenderer;
                    }
                }


                if (!hasMeshFilter) //如果没有网格选择器
                {
                    if (sceneView)
                        sceneView.ShowNotification(new GUIContent("没有网格选择器"), 1f);
                    else
                    {
                        EditorWindow.mouseOverWindow.ShowNotification(new GUIContent("没有网格选择器"), 1f);
                    }

                    return;
                }

                Mesh mesh_Combined = new Mesh();
                mesh_Combined.CombineMeshes(combineInstance);
                mesh_Combined.name = selectTransforms[0].name;
                var newMeshGameObject = new GameObject(string.Format("{0}-Merged", selectTransforms[0].name));
                var newMeshFilter = newMeshGameObject.AddComponent<MeshFilter>();
                newMeshFilter.sharedMesh = mesh_Combined;
                var newMeshRenderer = newMeshGameObject.AddComponent<MeshRenderer>();
                if (first_Renderer)
                {
                    newMeshRenderer.sharedMaterial = first_Renderer.sharedMaterial;
                    newMeshRenderer.shadowCastingMode = first_Renderer.shadowCastingMode;
                    newMeshRenderer.allowOcclusionWhenDynamic = first_Renderer.allowOcclusionWhenDynamic;
                    newMeshRenderer.receiveShadows = first_Renderer.receiveShadows;
                    newMeshRenderer.rendererPriority = first_Renderer.rendererPriority;
                }
                else
                {
                    newMeshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
                }

                //属性复制
                if (sceneView)
                    sceneView.ShowNotification(new GUIContent("网格已合并"), 1f);
                else
                {
                    EditorWindow.mouseOverWindow.ShowNotification(new GUIContent("网格已合并"), 1f);
                }
            }
            catch (Exception e)
            {
                if (sceneView)
                    sceneView.ShowNotification(new GUIContent(String.Format("网格合并失败\n{0}", e.Message)), 1f);
                else
                {
                    EditorWindow.mouseOverWindow.ShowNotification(
                        new GUIContent(String.Format("网格合并失败\n{0}", e.Message)), 1f);
                }

                Debug.LogError(e);
                throw;
            }
        }
    }
}