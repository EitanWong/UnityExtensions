#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Interface used for all Dependencies Parsers.
	/// </summary>
	public interface IDependenciesParser
	{
		/// <summary>
		/// Parser target asset type.
		/// Return null to match all assets types.
		/// </summary>
		Type Type { get; }
		
		/// <summary>
		/// Called by Maintainer in order to get passed asset dependencies.
		/// </summary>
		/// <param name="kind">Asset kind (is it regular asset from Assets folder, Settings asset or Package asset).</param>
		/// <param name="type">Asset's managed type.</param>
		/// <param name="path">AssetDatabase-friendly asset path.</param>
		/// <returns>AssetDatabase GUIDs of all assets used in the target asset at the specified path.</returns>
		List<string> GetDependenciesGUIDs(AssetKind kind, Type type, string path);
	}
}