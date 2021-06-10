#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Text;
	using Tools;
	using UnityEngine;

	/// <summary>
	/// Exact reference location kind.
	/// </summary>
	public enum Location
	{
		NotFound = 0,
		Invisible = 10,
		SceneGameObject = 20,
		PrefabAssetGameObject = 30,
		PrefabAssetObject = 35,
		ScriptAsset = 40,
		ScriptableObjectAsset = 50,
		SceneLightingSettings = 60,
		SceneNavigationSettings = 70,
		TileMap = 80,
	}

	/// <summary>
	/// Describes exact reference location as close to the end point as possible.
	/// </summary>
	[Serializable]
	public class ReferencingEntryData
	{
		/// <summary>
		/// Location kind.
		/// </summary>
		public Location Location
		{
			get { return location; }
		}

		/// <summary>
		/// Full transform path.
		/// </summary>
		public string TransformPath
		{
			get { return transformPath; }
		}

		/// <summary>
		/// Component type name.
		/// </summary>
		public string ComponentName
		{
			get { return componentName; }
		}

		/// <summary>
		/// Full property path.
		/// </summary>
		public string PropertyPath
		{
			get { return propertyPath; }
		}

		[SerializeField] internal Location location;
		[SerializeField] internal string prefixLabel;
		[SerializeField] internal string transformPath;
		[SerializeField] internal string componentName;
		[SerializeField] internal string propertyPath;
		[SerializeField] internal string suffixLabel;
		[SerializeField] internal int objectInstanceId = -1;
		[SerializeField] internal long objectId = -1L;
		[SerializeField] internal int componentInstanceId = -1;
		[SerializeField] internal long componentId = -1L;

		[NonSerialized]
		private StringBuilder labelStringBuilder;

		[NonSerialized]
		private string label;

		/// <summary>
		/// Get structured label of the exact reference for the output.
		/// </summary>
		/// <returns>String in format '[optional prefix] [transform path] | [component name]: [property path] [optional suffix]'.</returns>
		public string GetLabel()
		{
			if (label != null) return label;

			labelStringBuilder = new StringBuilder();

			var needsSpace = false;

			if (!string.IsNullOrEmpty(prefixLabel))
			{
				labelStringBuilder.Append(prefixLabel);
				needsSpace = true;
			}

			if (!string.IsNullOrEmpty(transformPath))
			{
				if (needsSpace) labelStringBuilder.Append(' ');

				string labelTransformPath;

				if (location == Location.PrefabAssetGameObject)
				{
					var transformPathSplitterIndex = transformPath.IndexOf('/');
					if (transformPathSplitterIndex != -1)
					{
						labelTransformPath = transformPath.Remove(0, transformPathSplitterIndex + 1);
					}
					else
					{
						labelTransformPath = "[Prefab Root]";
					}
				}
				else
				{
					labelTransformPath = transformPath;
				}

				if (labelTransformPath != null)
				{
					labelStringBuilder.Append(labelTransformPath);
					needsSpace = true;
				}
			}

			if (!string.IsNullOrEmpty(componentName))
			{
				if (needsSpace) labelStringBuilder.Append(" | ");
				labelStringBuilder.Append(componentName);
				needsSpace = true;
			}

			if (!string.IsNullOrEmpty(propertyPath))
			{
				if (needsSpace) labelStringBuilder.Append(": ");
				var nicePropertyPath = CSObjectTools.GetNicePropertyPath(propertyPath);
				labelStringBuilder.Append(nicePropertyPath);
				needsSpace = true;
			}

			if (!string.IsNullOrEmpty(suffixLabel))
			{
				if (needsSpace) labelStringBuilder.Append(' ');
				labelStringBuilder.Append(suffixLabel);
			}

			label = labelStringBuilder.ToString();

			labelStringBuilder.Length = 0;

			return label;
		}
	}
}