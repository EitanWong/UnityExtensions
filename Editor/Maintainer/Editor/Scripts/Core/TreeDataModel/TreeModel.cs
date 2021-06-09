#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;

	internal class TreeModel<T> where T : TreeItem
	{
		public event Action ModelChanged;

		private T[] data;
		public T root;
		private int maxID;

		public int NumberOfDataElements
		{
			get { return data.Length; }
		}

		public TreeModel(T[] data)
		{
			SetData(data);
		}

		public T Find(int id)
		{
			return data.FirstOrDefault (element => element.id == id);
		}

		public void SetData(T[] newData)
		{
			Init(newData);
		}

		private void Init(T[] newData)
		{
			if (newData == null)
				throw new ArgumentNullException("newData", Maintainer.ConstructError("Input data is null. Ensure input is a non-null list!"));

			data = newData;
			if (data.Length > 0)
				root = TreeItemUtility.ArrayToTree(data);

			maxID = data.Max(e => e.id);
		}

		public int GenerateUniqueID()
		{
			return ++maxID;
		}

		public IList<int> GetAncestors(int id)
		{
			var parents = new List<int>();
			TreeItem T = Find(id);
			if (T != null)
			{
				while (T.Parent != null)
				{
					parents.Add(T.Parent.id);
					T = T.Parent;
				}
			}
			return parents;
		}

		public IList<int> GetDescendantsThatHaveChildren(int id)
		{
			var searchFromThis = Find(id);
			if (searchFromThis != null)
			{
				return GetParentsBelowStackBased(searchFromThis);
			}
			return new List<int>();
		}

		private IList<int> GetParentsBelowStackBased(TreeItem searchFromThis)
		{
			var stack = new Stack<TreeItem>();
			stack.Push(searchFromThis);

			var parentsBelow = new List<int>();
			while (stack.Count > 0)
			{
				var current = stack.Pop();
				if (current.HasChildren)
				{
					parentsBelow.Add(current.id);
					foreach (var T in current.Children)
					{
						stack.Push(T);
					}
				}
			}

			return parentsBelow;
		}

		public void RemoveElements(IList<int> elementIDs)
		{
			IList<T> elements = data.Where (element => elementIDs.Contains (element.id)).ToArray ();
			RemoveElements(elements);
		}

		public void RemoveElements(IList<T> elements)
		{
			foreach (var element in elements)
			{
				if (element == root)
				{
					throw new ArgumentException(Maintainer.ConstructError("It is not allowed to remove the root element!"));
				}
			}

			var commonAncestors = TreeItemUtility.FindCommonAncestorsWithinList (elements);

			foreach (var element in commonAncestors)
			{
				element.Parent.Children.Remove (element);
				element.Parent = null;
			}

			TreeItemUtility.TreeToList(root, data);

			Changed();
		}

		public void AddElements(IList<T> elements, TreeItem parent, int insertPosition)
		{
			if (elements == null)
				throw new ArgumentNullException("elements", Maintainer.ConstructError("elements is null!"));
			if (elements.Count == 0)
				throw new ArgumentNullException("elements", Maintainer.ConstructError("elements Count is 0: nothing to add!"));
			if (parent == null)
				throw new ArgumentNullException("parent", Maintainer.ConstructError("parent is null!"));

			if (parent.Children == null)
				parent.Children = new List<TreeItem>();

			parent.Children.InsertRange(insertPosition, elements.Cast<TreeItem> ());
			foreach (var element in elements)
			{
				element.Parent = parent;
				element.depth = parent.depth + 1;
				TreeItemUtility.UpdateDepthValues(element);
			}

			TreeItemUtility.TreeToList(root, data);

			Changed();
		}

		public void AddRoot(T newRoot)
		{
			if (newRoot == null)
				throw new ArgumentNullException("newRoot", Maintainer.ConstructError("newRoot is null!"));

			if (data == null)
				throw new InvalidOperationException(Maintainer.ConstructError("Internal Error: data list is null!"));

			if (data.Length != 0)
				throw new InvalidOperationException(Maintainer.ConstructError("AddRoot is only allowed on empty data list!"));

			newRoot.id = GenerateUniqueID();
			newRoot.depth = -1;
			ArrayUtility.Add(ref data, newRoot);
		}

		public void AddElement(T element, TreeItem parent, int insertPosition)
		{
			if (element == null)
				throw new ArgumentNullException("element", Maintainer.ConstructError("element is null!"));
			if (parent == null)
				throw new ArgumentNullException("parent", Maintainer.ConstructError("parent is null!"));

			if (parent.Children == null)
				parent.Children = new List<TreeItem> ();

			parent.Children.Insert (insertPosition, element);
			element.Parent = parent;

			TreeItemUtility.UpdateDepthValues(parent);
			TreeItemUtility.TreeToList(root, data);

			Changed();
		}

		public void MoveElements(TreeItem parentItem, int insertionIndex, List<TreeItem> elements)
		{
			if (insertionIndex < 0)
				throw new ArgumentException(Maintainer.ConstructError("Invalid input: insertionIndex is -1, client needs to decide what index elements should be reparented at!"));

			if (parentItem == null)
				return;

			if (insertionIndex > 0)
				insertionIndex -= parentItem.Children.GetRange(0, insertionIndex).Count(elements.Contains);

			foreach (var draggedItem in elements)
			{
				draggedItem.Parent.Children.Remove(draggedItem);
				draggedItem.Parent = parentItem;
			}

			if (parentItem.Children == null)
				parentItem.Children = new List<TreeItem>();

			parentItem.Children.InsertRange(insertionIndex, elements);

			TreeItemUtility.UpdateDepthValues (root);
			TreeItemUtility.TreeToList(root, data);

			Changed();
		}

		private void Changed()
		{
			if (ModelChanged != null)
				ModelChanged();
		}
	}
}