#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using UnityEditor;
	using UnityEditor.SceneManagement;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	internal static class CSSceneTools
	{
		public class OpenSceneResult
		{
			public bool success;
			public bool sceneWasLoaded;
			public bool sceneWasAdded;
			public Scene scene;
			public string scenePath;
		}

		public static Scene GetSceneByPath(string path)
		{
			return SceneManager.GetSceneByPath(path);
		}

		public static OpenSceneResult OpenSceneWithSavePrompt(string path, bool activate = true)
		{
			StageUtility.GoToMainStage();

			var result = new OpenSceneResult();

			var targetScene = SceneManager.GetSceneByPath(path);
			if (targetScene == SceneManager.GetActiveScene())
			{
				result.scene = targetScene;
				result.success = true;
				result.scenePath = path;

				return result;
			}

			if (!SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return result;
			}

			return OpenScene(path, activate);
		}

		public static OpenSceneResult OpenScene(string path, bool activate = false)
		{
			StageUtility.GoToMainStage();

			var result = new OpenSceneResult();
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError(Maintainer.ConstructError("Can't open scene since path is absent!"));
				return result;
			}

			var targetScene = SceneManager.GetSceneByPath(path);
			result.scene = targetScene;
			result.scenePath = path;

			if (targetScene == SceneManager.GetActiveScene())
			{
				result.success = true;
				return result;
			}

			if (!targetScene.isLoaded)
			{
				result.sceneWasAdded = EditorSceneManager.GetSceneManagerSetup().All(s => s.path != targetScene.path);

				try
				{
					targetScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
				}
				catch (Exception e)
				{
					Debug.LogError(Maintainer.ConstructError("Error while opening scene: " + path + "\n" + e));
					return result;
				}

				result.scene = targetScene;

				if (!targetScene.IsValid())
				{
					Debug.LogError(Maintainer.ConstructError("Can't open scene since path leads to invalid scene!"));
					return result;
				}
				result.sceneWasLoaded = true;
			}

			result.success = true;

			if (activate)
			{
				SceneManager.SetActiveScene(targetScene);
			}

			return result;
		}

		public static bool IsOpenedSceneNeedsToBeClosed(OpenSceneResult lastOpenSceneResult, string nextScenePath = null, bool forceClose = false)
		{
			if (lastOpenSceneResult == null)
			{
				return false;
			}

			if (!string.IsNullOrEmpty(nextScenePath) && nextScenePath == lastOpenSceneResult.scenePath)
			{
				return false;
			}

			return (lastOpenSceneResult.sceneWasLoaded || forceClose) && EditorSceneManager.loadedSceneCount > 1;
		}

		public static void CloseOpenedSceneIfNeeded(OpenSceneResult lastOpenSceneResult, string nextScenePath = null, bool forceClose = false)
		{
			if (IsOpenedSceneNeedsToBeClosed(lastOpenSceneResult, nextScenePath, forceClose))
			{
				EditorSceneManager.CloseScene(lastOpenSceneResult.scene, lastOpenSceneResult.sceneWasLoaded || forceClose);
			}
		}

		public static bool SaveCurrentModifiedScenesIfUserWantsTo()
		{
			return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
		}

		public static bool SaveCurrentModifiedScenes(bool skipUntitled)
		{
			var scenesToSave = new List<Scene>();
			for (var i = 0; i < EditorSceneManager.loadedSceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (IsSceneUntitled(scene) && skipUntitled)
				{
					continue;
				}
				scenesToSave.Add(scene);
			}

			return SaveDirtyScenesWithPrompt(scenesToSave.ToArray());
		}

		public static void SaveScene(Scene scene)
		{
			EditorSceneManager.SaveScene(scene);
		}

		public static bool SaveDirtyScenesWithPrompt(Scene[] scenesToSave)
		{
			var anySceneIsDirty = false;
			var scenesPaths = string.Empty;
			for (var i = 0; i < scenesToSave.Length; i++)
			{
				var scene = scenesToSave[i];
				if (!scene.IsValid()) continue;
				if (!scene.isDirty) continue;

				if (IsSceneUntitled(scene))
				{
					scenesPaths += "\nUntitled";
				}
				else
				{
					scenesPaths += "\n" + scene.path;
				}

				anySceneIsDirty = true;
			}

			if (!anySceneIsDirty) return true;

			var selection = EditorUtility.DisplayDialogComplex("Scene(s) Have Been Modified",
				"Do you want to save the changes you made in the scenes:" +
				scenesPaths + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save", "Cancel");
			if (selection == 0)
			{
				EditorSceneManager.SaveScenes(scenesToSave.ToArray());
			}

			return selection != 2;
		}

		public static string[] GetScenesInBuildGUIDs(bool includeDisabled = false)
		{
			var scenesInBuild = GetScenesInBuild(includeDisabled);
			return CSAssetTools.GetAssetsGUIDs(scenesInBuild);
		}
		
		public static string[] GetScenesInBuild(bool includeDisabled = false)
		{
			var scenesForBuild = EditorBuildSettings.scenes;
			var scenesInBuild = new List<string>(scenesForBuild.Length);

			foreach (var sceneInBuild in scenesForBuild)
			{
				if (sceneInBuild.enabled || includeDisabled)
				{
					scenesInBuild.Add(CSPathTools.EnforceSlashes(sceneInBuild.path));
				}
			}
			return scenesInBuild.ToArray();
		}

		public static void MarkSceneDirty()
		{
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}

		public static SceneSetup[] GetScenesSetup()
		{
			var scenesSetup = EditorSceneManager.GetSceneManagerSetup();
			if (scenesSetup != null && scenesSetup.Length > 0 && !scenesSetup.Any(s => s.isActive))
			{
				var firstLoaded = scenesSetup.FirstOrDefault(s => s.isLoaded);
				if (firstLoaded != null)
				{
					firstLoaded.isActive = true;
				}
				else
				{
					scenesSetup[0].isActive = true;
					scenesSetup[0].isLoaded = true;
				}
			}
			return scenesSetup;
		}

		public static void ReSaveAllScenes()
		{
			EditorUtility.DisplayProgressBar("Re-saving scenes...", "Looking for scenes...", 0);
			var allScenesGuids = AssetDatabase.FindAssets("t:Scene");

			for (var i = 0; i < allScenesGuids.Length; i++)
			{
				var guid = allScenesGuids[i];
				EditorUtility.DisplayProgressBar("Re-saving scenes...", string.Format("Scene {0} of {1}", i + 1, allScenesGuids.Length), (float)i / allScenesGuids.Length);

				var scenePath = AssetDatabase.GUIDToAssetPath(guid);
				scenePath = CSPathTools.EnforceSlashes(scenePath);

				var result = OpenScene(scenePath);

				EditorSceneManager.MarkSceneDirty(result.scene);
				EditorSceneManager.SaveScene(result.scene);

				CloseOpenedSceneIfNeeded(result);
			}

			EditorUtility.ClearProgressBar();
		}

		public static void CloseUntitledSceneIfNotDirty()
		{
			for (var i = 0; i < EditorSceneManager.loadedSceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (IsSceneUntitled(scene) && !scene.isDirty)
				{
					SceneManager.UnloadSceneAsync(scene);
				}
			}
		}
		
		public static Scene GetUntitledScene()
		{
			for (var i = 0; i < EditorSceneManager.loadedSceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (IsSceneUntitled(scene))
				{
					return scene;
				}
			}

			return default(Scene);
		}

		private static bool IsSceneUntitled(Scene scene)
		{
			return string.IsNullOrEmpty(scene.name) && string.IsNullOrEmpty(scene.path);
		}
	}
}