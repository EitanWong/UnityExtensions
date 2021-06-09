#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Cleaner
{
	using System;
	using System.Text;

	/// <summary>
	/// Base Project Cleaner search results item.
	/// </summary>
	[Serializable]
	public abstract class CleanerRecord : RecordBase
	{
		/// <summary>
		/// Type of the item.
		/// </summary>
		public RecordType Type
		{
			get { return type; }
		}

		internal RecordType type;
		internal bool cleaned;

		/// <summary>
		/// Call to try cleaning item (it will be removed according to the current settings).
		/// </summary>
		/// <returns>True if removal was successful / possible, false otherwise.</returns>
		public bool Clean()
		{
			cleaned = PerformClean();
			return cleaned;
		}

		// -----------------------------------------------------------------------------
		// base constructors
		// -----------------------------------------------------------------------------

		protected CleanerRecord(RecordType type, RecordLocation location):base(location)
		{
			this.type = type;
		}

		// -----------------------------------------------------------------------------
		// header generation
		// -----------------------------------------------------------------------------

		protected override void ConstructHeader(StringBuilder header)
		{
			switch (type)
			{
				case RecordType.EmptyFolder:
					header.Append("Empty Folder");
					break;
				case RecordType.UnreferencedAsset:
					header.Append("Unused ");
					break;
				case RecordType.Warning:
					header.Append("Warning");
					break;
				case RecordType.Error:
					header.Append("Error!");
					break;
				case RecordType.Other:
					header.Append("Other");
					break;
				default:
					header.Append("Unknown issue!");
					break;
			}
		}

		protected abstract bool PerformClean();
	}
}