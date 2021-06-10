#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	using System;
	using Core;
	using Tools;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	internal static class GenericObjectProcessor
	{
		private const string MainTexturePropertyName = "_MainTex";
		private static readonly int MainTextureShaderProperty = Shader.PropertyToID(MainTexturePropertyName);

		public static void ProcessObject(Location currentLocation, Object inspectedUnityObject, Object target, EntryAddSettings addSettings, ProcessObjectReferenceHandler processReferenceCallback)
		{
			var onlyVisibleProperties = currentLocation != Location.ScriptAsset;
			var componentTraverseInfo = new SerializedObjectTraverseInfo(target, onlyVisibleProperties);

			string lastScriptPropertyName = null;

			CSTraverseTools.TraverseObjectProperties(componentTraverseInfo, (info, sp) =>
			{
				if (currentLocation == Location.ScriptAsset)
				{
					if (sp.isArray)
					{
						if (sp.type == "string")
						{
							if (sp.propertyPath.IndexOf("m_DefaultReferences.Array.data[", StringComparison.OrdinalIgnoreCase) == 0)
							{
								if (sp.stringValue != null)
								{
									lastScriptPropertyName = sp.stringValue;

									// skipping first pair item of the m_DefaultReferences array item
									sp.Next(false);
								}
							}
						}
					}
				}

				if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue != null)
				{
					string propertyName;

					if (lastScriptPropertyName != null)
					{
						propertyName = lastScriptPropertyName;
						lastScriptPropertyName = string.Empty;
					}
					else
					{
						propertyName = sp.propertyPath;
					}

					/*if (string.Equals(propertyName, "m_Script", StringComparison.OrdinalIgnoreCase))
					{
						propertyName = "Script source";
					}*/

					addSettings.propertyPath = propertyName;

					processReferenceCallback(inspectedUnityObject, sp.objectReferenceInstanceIDValue, addSettings);

					/* material instance handling */

					var material = sp.objectReferenceValue as Material;
					if (material == null) return;

					if (currentLocation == Location.PrefabAssetGameObject)
					{
						if (AssetDatabase.GetAssetPath(material) != AssetDatabase.GetAssetPath(target)) return;
						if (AssetDatabase.IsSubAsset(material)) return;
					}
					else
					{
						if (AssetDatabase.Contains(material)) return;
					}

					addSettings.prefix = "[Material Instance]";
					addSettings.suffix = "(Main Texture)";

					var mainTextureInstanceId = 0;
					if (material.HasProperty(MainTextureShaderProperty))
					{
						var mainTexture = material.mainTexture;
						mainTextureInstanceId = mainTexture != null ? mainTexture.GetInstanceID() : 0;
					}

					processReferenceCallback(inspectedUnityObject, mainTextureInstanceId, addSettings);

					addSettings.suffix = "(Shader)";

					var shaderInstanceId = material.shader != null ? material.shader.GetInstanceID() : 0;
					processReferenceCallback(inspectedUnityObject, shaderInstanceId, addSettings);

					var materialSo = new SerializedObject(material);

					var texEnvs = materialSo.FindProperty("m_SavedProperties.m_TexEnvs.Array");
					if (texEnvs != null)
					{
						for (var k = 0; k < texEnvs.arraySize; k++)
						{
							var arrayItem = texEnvs.GetArrayElementAtIndex(k);
							var fieldName = arrayItem.displayName;
							if (fieldName == MainTexturePropertyName) continue;

							var textureProperty = arrayItem.FindPropertyRelative("second.m_Texture");
							if (textureProperty != null)
							{
								if (textureProperty.propertyType == SerializedPropertyType.ObjectReference)
								{
									addSettings.suffix = " (" + fieldName + ")";
									processReferenceCallback(inspectedUnityObject, textureProperty.objectReferenceInstanceIDValue, addSettings);
								}
							}
							else
							{
								Debug.LogError(Maintainer.ConstructError("Can't get second.m_Texture from texEnvs at " + inspectedUnityObject.name));
							}
						}
					}
					else
					{
						Debug.LogError(Maintainer.ConstructError("Can't get m_SavedProperties.m_TexEnvs.Array from material instance at " + inspectedUnityObject.name));
					}
				}

				lastScriptPropertyName = null;
			});
		}
	}
}