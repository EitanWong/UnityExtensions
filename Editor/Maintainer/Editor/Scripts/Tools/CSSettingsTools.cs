#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using UnityEditor.AI;
	using UnityEngine;

	internal static class CSSettingsTools
	{
		public static Object GetInSceneLightmapSettings()
		{
			var mi = CSReflectionTools.GetGetLightmapSettingsMethodInfo();
			if (mi != null)
			{
				return (Object)mi.Invoke(null, null);
			}

			Debug.LogError(Maintainer.ConstructError("Can't retrieve LightmapSettings object via reflection!"));
			return null;
		}

		public static Object GetInSceneRenderSettings()
		{
			var mi = CSReflectionTools.GetGetRenderSettingsMethodInfo();
			if (mi != null)
			{
				return (Object)mi.Invoke(null, null);
			}

			Debug.LogError(Maintainer.ConstructError("Can't retrieve RenderSettings object via reflection!"));
			return null;
		}

		public static Object GetInSceneNavigationSettings()
		{
			return NavMeshBuilder.navMeshSettingsObject;
		}
	}
}