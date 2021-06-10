#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	using UnityEngine;
	using UnityEngine.Tilemaps;

	internal static class ManualComponentProcessor
	{
		public static void ProcessTilemap(Object inspectedUnityObject, Tilemap target, EntryAddSettings addSettings, ProcessObjectReferenceHandler processReferenceCallback)
		{
			var tilesCount = target.GetUsedTilesCount();
			if (tilesCount == 0) return;

			var usedTiles = new TileBase[tilesCount];
			target.GetUsedTilesNonAlloc(usedTiles);

			foreach (var usedTile in usedTiles)
			{
				processReferenceCallback(inspectedUnityObject, usedTile.GetInstanceID(), addSettings);

				var tile = usedTile as Tile;
				if (tile == null) continue;

				if (tile.sprite != null)
				{
					processReferenceCallback(inspectedUnityObject, tile.sprite.GetInstanceID(), addSettings);
				}
			}
		}
	}
}