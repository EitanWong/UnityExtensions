#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Use it to guess current directory of the Maintainer.
	/// </summary>
	public class MaintainerMarker : ScriptableObject
	{
		/// <summary>
		/// Returns raw path of the MaintainerMarker script for further reference.
		/// </summary>
		/// <returns>Path of the MaintainerMarker ScriptableObject asset.</returns>
		public static string GetAssetPath()
		{
			string result;

			var tempInstance = CreateInstance<MaintainerMarker>();
			var script = MonoScript.FromScriptableObject(tempInstance);
			if (script != null)
			{
				result = AssetDatabase.GetAssetPath(script);
			}
			else
			{
				result = AssetDatabase.FindAssets("MaintainerMarker")[0];
				result = AssetDatabase.GUIDToAssetPath(result);
			}
			
			DestroyImmediate(tempInstance);
			return result;
		}
	}
}