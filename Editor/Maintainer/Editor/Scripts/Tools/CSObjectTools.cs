#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UnityEngine.SceneManagement;
	using Object = UnityEngine.Object;
	using UnityEditor.Experimental.SceneManagement;

	internal static class CSObjectTools
	{
		public static long GetUniqueObjectId(Object unityObject, bool recursiveCall = false)
		{
			var id = -1L;
			var siblingId = 0;

			if (unityObject == null) return id;

			var go = unityObject as GameObject;
			if (go != null)
			{
				siblingId = go.transform.GetSiblingIndex();
				//unityObject = go.transform;
			}

			if (CSPrefabTools.IsInstance(unityObject))
			{
				var prefabAssetSource = CSPrefabTools.GetAssetSource(unityObject);
				if (prefabAssetSource != null)
				{
					if (!recursiveCall)
					{
						id = GetUniqueObjectId(prefabAssetSource, true);
						return id + siblingId;
					}

					Debug.LogError(Maintainer.ConstructError("Couldn't reach asset source: " + unityObject.name), unityObject);
				}
			}

			if (AssetDatabase.Contains(unityObject))
			{
				id = GetUniqueObjectIdFromAssetObject(unityObject);
			}
			else
			{
				id = GetLocalIdentifierInFile(unityObject);
				if (id <= 0)
				{
					id = unityObject.GetInstanceID();
				}
			}

			if (id <= 0)
			{
				id = siblingId;
			}

			if (id <= 0)
			{
				id = unityObject.name.GetHashCode();
			}

			return id;
		}

		private static long GetUniqueObjectIdFromAssetObject(Object unityObject)
		{
			/*var go = unityObject as GameObject;
			if (go != null)
			{
				unityObject = go.transform;
			}*/

			long result = -1;
			var path = AssetDatabase.GetAssetPath(unityObject);
			if (!string.IsNullOrEmpty(path))
			{
				result = GetAssetLocalIdentifierInFile(unityObject);
			}
			else
			{
				Debug.LogError(Maintainer.ConstructError("Can't get path to the asset " + unityObject.name));
			}

			return result;
		}

		public static string GetNicePropertyPath(string fullPropertyPath)
		{
			return CSEditorTools.NicifyName(RemoveArrayStuffFromProperty(fullPropertyPath));
		}

		public static string GetNativeObjectType(Object unityObject)
		{
			string result;

			try
			{
				var fullName = unityObject.ToString();
				var openingIndex = fullName.IndexOf('(') + 1;
				if (openingIndex != 0)
				{
					var closingIndex = fullName.LastIndexOf(')');
					result = fullName.Substring(openingIndex, closingIndex - openingIndex);
				}
				else
				{
					result = null;
				}
			}
			catch
			{
				result = null;
			}

			return result;
		}

		public static void SelectGameObject(GameObject go, bool inScene)
		{
			if (inScene)
			{
				Selection.activeTransform = go == null ? null : go.transform;
			}
			else
			{
				Selection.activeGameObject = go;
			}
		}

		public static GameObject FindGameObjectInScene(Scene scene, long objectId, string transformPath = null)
		{
			GameObject result = null;
			var rootObjects = scene.GetRootGameObjects();

			foreach (var rootObject in rootObjects)
			{
				result = FindChildGameObjectRecursive(rootObject.transform, objectId, rootObject.transform.name, transformPath);
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		public static GameObject FindChildGameObjectRecursive(Transform parent, long objectId, string currentTransformPath, string transformPath = null)
		{
			var skipObjectIdCheck = false;

			if (!string.IsNullOrEmpty(transformPath))
			{
				if (!currentTransformPath.StartsWith(transformPath, StringComparison.Ordinal))
				{
					skipObjectIdCheck = true;
				}
			}

			if (!skipObjectIdCheck)
			{
				if (objectId == 0)
				{
					return parent.gameObject;
				}

				var currentObjectId = GetUniqueObjectId(parent.gameObject);
				if (currentObjectId == objectId)
				{
					return parent.gameObject;
				}
			}

			GameObject result = null;
			for (var i = 0; i < parent.childCount; i++)
			{
				var childTransform = parent.GetChild(i);
				result = FindChildGameObjectRecursive(childTransform, objectId, currentTransformPath + "/" + childTransform.name, transformPath);
				if (result != null) break;
			}

			return result;
		}

		public static string GetScriptPathFromObject(Object unityObject)
		{
			if (unityObject == null) return null;

			MonoScript monoScript = null;

			var monoBehaviour = unityObject as MonoBehaviour;
			if (monoBehaviour != null)
			{
				monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
			}

			var scriptableObject = unityObject as ScriptableObject;
			if (scriptableObject != null)
			{
				monoScript = MonoScript.FromScriptableObject(scriptableObject);
			}

			return monoScript == null ? null : AssetDatabase.GetAssetPath(monoScript);
		}

		public static int[] GetObjectsInstanceIDs(Object[] objects)
		{
			var instanceIds = new int[objects.Length];
			for (var i = 0; i < objects.Length; i++)
			{
				instanceIds[i] = objects[i].GetInstanceID();
			}

			return instanceIds;
		}

		public static Object[] GetObjectsFromInstanceIds(int[] objectInstanceIDs)
		{
			var objects = new List<Object>(objectInstanceIDs.Length);
			foreach (var id in objectInstanceIDs)
			{
				var o = EditorUtility.InstanceIDToObject(id);
				if (o != null)
				{
					objects.Add(o);
				}
			}

			return objects.ToArray();
		}

		public static bool IsHiddenInInspector(Object target)
		{
			return (target.hideFlags & HideFlags.HideInInspector) != 0;
		}
		
		public static string TryGetObjectAssetPath(Object target)
		{
			var assetPath = AssetDatabase.GetAssetOrScenePath(target);
                        
			if (string.IsNullOrEmpty(assetPath))
            {
				var component = target as Component;
				var gameObject = target as GameObject;

				if (component != null)
				{
					gameObject = component.gameObject;
				}

				if (gameObject == null)
				{
					return null;
				}

	            var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
				if (prefabStage != null)
				{
#if UNITY_2020_1_OR_NEWER
					assetPath = prefabStage.assetPath;
#else
					assetPath = prefabStage.prefabAssetPath;
#endif
				}
            }
			
			return assetPath;
		}

		private static string RemoveArrayStuffFromProperty(string fullPropertyPath)
		{
			var index = fullPropertyPath.IndexOf(".array.data[", StringComparison.OrdinalIgnoreCase);
			if (index == -1) return fullPropertyPath;

			var propertyPath = fullPropertyPath.Remove(index, 11);
			return propertyPath;
		}

		private static long GetLocalIdentifierInFile(Object unityObject)
		{
			var go = unityObject as GameObject;
			if (go != null)
			{
				unityObject = go;
			}

			var serializedObject = new SerializedObject(unityObject);
			try
			{
				CSReflectionTools.SetInspectorToDebug(serializedObject);
				var serializedProperty = serializedObject.FindProperty("m_LocalIdentfierInFile");
				return serializedProperty.longValue;
			}
			catch (Exception e)
			{
				Debug.LogWarning(Maintainer.ConstructWarning("Couldn't get data from debug inspector for object " + unityObject.name + " due to this error:\n" + e));
				return -1;
			}
		}

		private static long GetAssetLocalIdentifierInFile(Object unityObject)
		{
			string guid;
			long id;

			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(unityObject, out guid, out id))
			{
				return -1;
			}

			return id;
		}
	}
}