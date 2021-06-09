#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using UnityEngine.SceneManagement;

	[Serializable]
	internal class ReferencedAtOpenedSceneInfo : ReferencedAtInfo
	{
		public Scene openedScene = default(Scene);
	}
}