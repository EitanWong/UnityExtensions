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

	internal static class CSArrayTools
	{
		public static bool AddIfNotExists<T>(ref T[] items, T newItem)
		{
			if (Array.IndexOf(items, newItem) != -1) return false;
			ArrayUtility.Add(ref items, newItem);
			return true;
		}

		public static bool TryAddIfNotExists<T>(ref List<T> items, List<T> newItems)
		{
			for (var i = newItems.Count - 1; i >= 0; i--)
			{
				if (items.Contains(newItems[i]))
				{
					newItems.RemoveAt(i);
				}
			}

			if (newItems.Count > 0)
			{
				items.AddRange(newItems);
				return true;
			}

			return false;
		}

		public static bool IsItemContainsAnyStringFromArray(string item, ICollection<string> items)
		{
			if (items == null) return false;

			var result = false;

			foreach (var str in items)
			{
				if (item.Contains(str))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public static T[] FindDuplicatesInArray<T>(T[] array)
		{
			return array.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
		}

		public static List<T> FindDuplicatesInArray<T>(List<T> array)
		{
			return array.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
		}

		public static T[] RemoveAt<T>(T[] source, int index)
		{
			var newArray = new T[source.Length - 1];
			if (index > 0)
				Array.Copy(source, 0, newArray, 0, index);

			if (index < source.Length - 1)
				Array.Copy(source, index + 1, newArray, index, source.Length - index - 1);

			return newArray;
		}
	}
}