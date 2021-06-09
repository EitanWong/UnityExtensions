#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	using Core;
	using Tools;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;
	using UnityEngine.Tilemaps;

	internal delegate void ProcessObjectReferenceHandler(Object lookAt, int lookForInstanceId, EntryAddSettings settings);

	internal static class EntryFinder
	{
		public static Location currentLocation;
		public static ProcessObjectReferenceHandler currentProcessReferenceCallback;

		public static bool OnGameObjectTraverse(ObjectTraverseInfo traverseInfo)
		{
			var target = traverseInfo.current;

			//Debug.Log("OnGameObjectTraverse " + target);

			if (traverseInfo.inPrefabInstance)
			{
				var prefabAssetSource = CSPrefabTools.GetAssetSource(target);
				if (prefabAssetSource != null)
				{
					var instanceId = prefabAssetSource.GetInstanceID();
					currentProcessReferenceCallback(target, instanceId, null);

					if (traverseInfo.dirtyComponents == null)
					{
						traverseInfo.skipCurrentTree = true;
						return true;
					}
				}
			}

			var thumbnail = AssetPreview.GetMiniThumbnail(target);
			if (thumbnail != null && (thumbnail.hideFlags & HideFlags.HideAndDontSave) == 0)
			{
				var addSettings = new EntryAddSettings
				{
					prefix = "[Object Icon]",
				};
				currentProcessReferenceCallback(target, thumbnail.GetInstanceID(), addSettings);
			}

			CSTraverseTools.TraverseGameObjectComponents(traverseInfo, OnGameObjectComponentTraverse);

			return true;
		}

		public static void TraverseObjectProperties(Object inspectedUnityObject, Object target, EntryAddSettings addSettings)
		{
			if (target is Tilemap)
			{
				ManualComponentProcessor.ProcessTilemap(inspectedUnityObject, (Tilemap)target, addSettings, currentProcessReferenceCallback);
				return;
			}
			
			GenericObjectProcessor.ProcessObject(currentLocation, inspectedUnityObject, target, addSettings, currentProcessReferenceCallback);
		}

		private static void OnGameObjectComponentTraverse(ObjectTraverseInfo traverseInfo, Component component, int orderIndex)
		{
			if (component == null) return;

			var target = traverseInfo.current;
			var componentName = CSComponentTools.GetComponentName(component);
			if (CSObjectTools.IsHiddenInInspector(component))
			{
				orderIndex = -1;
			}

			var addSettings = new EntryAddSettings
			{
				componentName = componentName,
				componentIndex = orderIndex,
				componentInstanceId = component.GetInstanceID(),
			};

			TraverseObjectProperties(target, component, addSettings);
		}
	}
}