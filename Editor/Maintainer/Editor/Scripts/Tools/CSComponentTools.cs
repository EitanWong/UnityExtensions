#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using UnityEngine;

	internal static class CSComponentTools
	{
		public static bool IsComponentVisibleInInspector(Component component)
		{
			return component == null || !CSObjectTools.IsHiddenInInspector(component);
		}

		public static int GetComponentIndex(Component target)
		{
			if (target == null) return -1;
			if (!IsComponentVisibleInInspector(target)) return -1;

			var allComponents = target.GetComponents<Component>();
			var visibleIndex = -1;
			foreach (var component in allComponents)
			{
				if (IsComponentVisibleInInspector(component))
				{
					visibleIndex++;
				}

				if (target == component)
				{
					return visibleIndex;
				}
			}

			return -1;
		}

		public static Component GetComponentWithIndex(GameObject parent, long componentIndex)
		{
			if (parent == null) return null;
			if (componentIndex == -1) return null;

			var allComponents = parent.GetComponents<Component>();
			var visibleIndex = -1;
			foreach (var component in allComponents)
			{
				if (IsComponentVisibleInInspector(component))
				{
					visibleIndex++;
				}

				if (componentIndex == visibleIndex)
				{
					return component;
				}
			}

			return null;
		}

		public static string GetComponentName(Component target)
		{
			var result = target.GetType().Name;

			if ((target.hideFlags & HideFlags.HideInInspector) != 0)
			{
				result += " (HideInInspector)";
			}

			return result;
		}
	}
}