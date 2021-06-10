#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using System;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using Object = UnityEngine.Object;

	internal class TraverseStats
	{
		public long gameObjectsTraversed;
		public long componentsTraversed;
		public long propertiesTraversed;

		public void Clear()
		{
			gameObjectsTraversed = componentsTraversed = propertiesTraversed = 0;
		}
	}

	internal class ObjectTraverseInfo
	{
		public bool ForcePrefabsAsRegularObjects { get; private set; }
		public bool SkipCleanPrefabInstances { get; private set; }
		public int TotalRoots { get; private set; }

		public GameObject current;
		public bool inPrefabInstance;
		public bool inMissingPrefabInstance;
		public int[] dirtyComponents;
		public int rootIndex;

		public bool skipCurrentTree;

		public ObjectTraverseInfo(bool skipCleanPrefabInstances, bool forcePrefabsAsRegularObjects, int totalRoots)
		{
			SkipCleanPrefabInstances = skipCleanPrefabInstances;
			ForcePrefabsAsRegularObjects = forcePrefabsAsRegularObjects;
			TotalRoots = totalRoots;
		}
	}

	internal class SerializedObjectTraverseInfo
	{
		public Object TraverseTarget { get; private set; }
		public bool OnlyVisibleProperties { get; private set; }

		public bool skipCurrentTree;

		public SerializedObjectTraverseInfo(Object traverseTarget, bool onlyVisibleProperties = true)
		{
			TraverseTarget = traverseTarget;
			OnlyVisibleProperties = onlyVisibleProperties;
		}
	}

	internal static class CSTraverseTools
	{
		internal delegate bool GameObjectTraverseCallback(ObjectTraverseInfo traverseInfo);
		internal delegate void SceneRootTraverseCallback(int index, int total, out bool canceled);
		internal delegate void ComponentTraverseCallback(ObjectTraverseInfo traverseInfo, Component component, int orderIndex);
		internal delegate void SerializableObjectTraverseCallback(SerializedObjectTraverseInfo traverseInfo, SerializedProperty currentProperty);

		private static readonly TraverseStats Stats = new TraverseStats();

		public static void ClearStats()
		{
			Stats.Clear();
		}

		public static TraverseStats GetStats()
		{
			return Stats;
		}
		
		public static bool TraverseSceneGameObjects(Scene scene, bool skipCleanPrefabInstances, bool forceAllPrefabsTraverse, GameObjectTraverseCallback callback, SceneRootTraverseCallback rootTraverseCallback = null)
		{
			var rootObjects = scene.GetRootGameObjects();
			return TraverseRootGameObjects(rootObjects, skipCleanPrefabInstances, forceAllPrefabsTraverse, callback, rootTraverseCallback);
		}
		
		public static bool TraversePrefabGameObjects(GameObject prefabAsset, bool skipCleanPrefabInstances, bool forceAllPrefabsTraverse, GameObjectTraverseCallback callback)
		{
			return TraverseRootGameObjects(new [] {prefabAsset}, skipCleanPrefabInstances, forceAllPrefabsTraverse, callback);
		}

		public static bool TraverseRootGameObjects(GameObject[] rootObjects, bool skipCleanPrefabInstances, bool forceAllPrefabsTraverse, GameObjectTraverseCallback callback, SceneRootTraverseCallback rootTraverseCallback = null)
		{
			var rootObjectsCount = rootObjects.Length;
			var objectTraverseInfo = new ObjectTraverseInfo(skipCleanPrefabInstances, forceAllPrefabsTraverse, rootObjectsCount);

			for (var i = 0; i < rootObjectsCount; i++)
			{
				var rootObject = rootObjects[i];

				if (rootTraverseCallback != null)
				{
					bool canceled;
					rootTraverseCallback.Invoke(i, rootObjectsCount, out canceled);
					if (canceled)
					{
						return false;
					}
				}

				objectTraverseInfo.current = rootObject;
				objectTraverseInfo.rootIndex = i;

				if (!TraverseGameObjectTree(objectTraverseInfo, callback))
				{
					return false;
				}
			}

			return true;
		}

		private static bool TraverseGameObjectTree(ObjectTraverseInfo traverseInfo, GameObjectTraverseCallback callback)
		{
			Stats.gameObjectsTraversed++;

			var prefabInstanceRoot = false;
			if (!traverseInfo.inPrefabInstance && !traverseInfo.ForcePrefabsAsRegularObjects)
			{
				//Debug.Log("IsPartOfPrefabInstance " + PrefabUtility.IsPartOfPrefabInstance(componentOrGameObject));
				//Debug.Log(traverseInfo.current.name);
				traverseInfo.inPrefabInstance = CSPrefabTools.IsInstance(traverseInfo.current);
				if (traverseInfo.inPrefabInstance)
				{
					if (!CSPrefabTools.IsMissingPrefabInstance(traverseInfo.current))
					{
						if (traverseInfo.SkipCleanPrefabInstances)
						{
							if (CSPrefabTools.IsWholeInstanceHasAnyOverrides(traverseInfo.current))
							{
								CSPrefabTools.GetOverridenObjectsFromWholePrefabInstance(traverseInfo.current, out traverseInfo.dirtyComponents);
							}
						}
					}
					else
					{
						traverseInfo.inMissingPrefabInstance = true;
					}

					prefabInstanceRoot = true;
				}
			}

			if (!callback.Invoke(traverseInfo))
			{
				return false;
			}

			if (traverseInfo.skipCurrentTree)
			{
				if (prefabInstanceRoot)
				{
					traverseInfo.dirtyComponents = null;
					traverseInfo.inPrefabInstance = false;
					traverseInfo.inMissingPrefabInstance = false;
					traverseInfo.skipCurrentTree = false;
				}

				return true;
			}

			var transform = traverseInfo.current.transform;
			var childrenCount = transform.childCount;

			for (var i = 0; i < childrenCount; i++)
			{
				var child = transform.GetChild(i);
				traverseInfo.current = child.gameObject;
				if (!TraverseGameObjectTree(traverseInfo, callback))
				{
					return false;
				}
			}

			if (prefabInstanceRoot)
			{
				traverseInfo.dirtyComponents = null;
				traverseInfo.inPrefabInstance = false;
				traverseInfo.inMissingPrefabInstance = false;
				traverseInfo.skipCurrentTree = false;
			}

			return true;
		}

		public static void TraverseGameObjectComponents(ObjectTraverseInfo traverseInfo, ComponentTraverseCallback callback)
		{
			var components = traverseInfo.current.GetComponents<Component>();
			var checkingPrefabInstanceObject = false;
			var checkingAddedToInstanceObject = false;

			if (traverseInfo.SkipCleanPrefabInstances)
			{
				checkingPrefabInstanceObject = traverseInfo.inPrefabInstance && !traverseInfo.inMissingPrefabInstance;
				if (checkingPrefabInstanceObject)
				{
					checkingAddedToInstanceObject = PrefabUtility.IsAddedGameObjectOverride(traverseInfo.current);
				}
			}

			var hasDirtyComponents = traverseInfo.dirtyComponents != null;

			var visibleIndex = -1;
			for (var i = 0; i < components.Length; i++)
			{
				var component = components[i];
				if (CSComponentTools.IsComponentVisibleInInspector(component))
				{
					visibleIndex++;
				}

				if (component == null)
				{
					// to register missing script
					callback(traverseInfo, null, visibleIndex);
					continue;
				}

				// transforms are checked at the GameObject level
				if (component is Transform) continue;

				if (!checkingPrefabInstanceObject)
				{
					Stats.componentsTraversed++;
					callback(traverseInfo, component, visibleIndex);
				}
				else
				{
					var hasOverridenProperties = checkingAddedToInstanceObject;
					if (!hasOverridenProperties && hasDirtyComponents)
					{
						if (Array.IndexOf(traverseInfo.dirtyComponents, component.GetInstanceID()) != -1)
						{
							hasOverridenProperties = true;
						}
					}

					if (hasOverridenProperties)
					{
						Stats.componentsTraversed++;
						callback(traverseInfo, component, visibleIndex);
					}
				}
			}
		}

		public static void TraverseObjectProperties(SerializedObjectTraverseInfo traverseInfo, SerializableObjectTraverseCallback callback)
		{
			var so = new SerializedObject(traverseInfo.TraverseTarget);
			var iterator = so.GetIterator();

			if (traverseInfo.OnlyVisibleProperties)
			{
				while (iterator.NextVisible(!traverseInfo.skipCurrentTree))
				{
					if (traverseInfo.skipCurrentTree) traverseInfo.skipCurrentTree = false;
					Stats.propertiesTraversed++;
					callback(traverseInfo, iterator);
				}
			}
			else
			{
				while (iterator.Next(!traverseInfo.skipCurrentTree))
				{
					if (!traverseInfo.skipCurrentTree) traverseInfo.skipCurrentTree = false;

					Stats.propertiesTraversed++;
					callback(traverseInfo, iterator);
				}
			}
		}
	}
}