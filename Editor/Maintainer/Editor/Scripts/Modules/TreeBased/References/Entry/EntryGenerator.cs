#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	using Core;
	using System.Collections.Generic;
	using Tools;
	using UnityEngine;

	internal static class EntryGenerator
	{
		private class CachedObjectData
		{
			public long objectId;
			public int objectInstanceId;
			public string transformPath;
		}

		private static readonly Dictionary<int, CachedObjectData> CachedObjects = new Dictionary<int, CachedObjectData>();

		public static void ResetCachedObjects()
		{
			CachedObjects.Clear();
		}

		public static ReferencingEntryData CreateNewReferenceEntry(Location currentLocation, Object lookAt, GameObject lookAtGameObject, EntryAddSettings settings)
		{
			var lookAtInstanceId = lookAt.GetInstanceID();
			CachedObjectData cachedObject;
			
			if (CachedObjects.ContainsKey(lookAtInstanceId))
			{
				cachedObject = CachedObjects[lookAtInstanceId];
			}
			else
			{
				cachedObject = new CachedObjectData
				{
					objectId = CSObjectTools.GetUniqueObjectId(lookAt),
					objectInstanceId = lookAt.GetInstanceID(),
				};

				if (currentLocation == Location.SceneGameObject || currentLocation == Location.PrefabAssetGameObject)
				{
					if (lookAtGameObject != null)
					{
						var transform = lookAtGameObject.transform;
						cachedObject.transformPath = CSEditorTools.GetFullTransformPath(transform);
					}
					else
					{
						cachedObject.transformPath = lookAt.name;
					}
				}
				else if (currentLocation == Location.PrefabAssetObject)
				{
					cachedObject.transformPath = lookAt.name;
				}
				else
				{
					cachedObject.transformPath = string.Empty;
				}

				CachedObjects.Add(lookAtInstanceId, cachedObject);
			}

			var newEntry = new ReferencingEntryData
			{
				location = currentLocation,
				objectId = cachedObject.objectId,
				objectInstanceId = cachedObject.objectInstanceId,
				transformPath = cachedObject.transformPath
			};

			if (settings != null)
			{
				newEntry.componentName = settings.componentName;
				newEntry.componentId = settings.componentIndex;
				newEntry.componentInstanceId = settings.componentInstanceId;
				newEntry.prefixLabel = settings.prefix;
				newEntry.suffixLabel = settings.suffix;
				newEntry.propertyPath = settings.propertyPath;
			}

			return newEntry;
		}
	}
}