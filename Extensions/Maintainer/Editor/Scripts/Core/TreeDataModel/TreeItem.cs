#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Basic item which is used to build results trees at Maintainer
	/// </summary>
	[Serializable]
	public class TreeItem
	{
		/// <summary>
		/// Item name.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Item depth (-1 for root).
		/// </summary>
		public int Depth
		{
			get { return depth; }
		}

		[SerializeField] internal string name;
		[SerializeField] internal int depth;
		[SerializeField] internal int id;
		[SerializeField] internal int recursionId = -1;

		/// <summary>
		/// Parent item, if any.
		/// </summary>
		[field:NonSerialized]
		public TreeItem Parent { get; internal set; }

		/// <summary>
		/// All child items.
		/// </summary>
		[field:NonSerialized]
		public List<TreeItem> Children { get; internal set; }

		/// <summary>
		/// Shortcut to #Children list count.
		/// </summary>
		public int ChildrenCount
		{
			get
			{
				return Children == null ? 0 : Children.Count;
			}
		}

		/// <summary>
		/// Quick way to check if this item has children.
		/// </summary>
		public bool HasChildren
		{
			get { return Children != null && Children.Count > 0; }
		}

		internal TreeItem()
		{
		}

		internal TreeItem(string name, int depth, int id)
		{
			this.name = name;
			this.depth = depth;
			this.id = id;
		}

		/// <summary>
		/// Use to check if this item's name contains search term.
		/// </summary>
		/// <param name="searchString">Any search term to check name for.</param>
		/// <returns>True if name contains search term and false otherwise.</returns>
		public bool CanBeFoundWith(string searchString)
		{
			return Search(searchString);
		}

		protected virtual bool Search(string searchString)
		{
			return name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}
}