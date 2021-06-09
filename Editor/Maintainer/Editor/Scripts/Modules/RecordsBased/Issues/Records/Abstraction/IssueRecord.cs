#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Result of the issue fix operation.
	/// </summary>
	[Serializable]
	public class FixResult
	{
		/// <summary>
		/// Returns true if fix was successful and confirmed, false otherwise.
		/// </summary>
		public bool Success { get; private set; }

		/// <summary>
		/// Contains error text in case fix was not successful. May be empty if fail cause is not known.
		/// </summary>
		public string ErrorText { get; private set; }

		internal FixResult(bool success, string errorText = null)
		{
			Success = success;
			ErrorText = errorText;
		}

		internal static FixResult CreateError(string errorText)
		{
			return new FixResult(false, errorText);
		}

		internal void SetErrorText(string errorText)
		{
			ErrorText = errorText;
		}
	}

	/// <summary>
	/// Base class for all Issues Finder results items.
	/// </summary>
	[Serializable]
	public abstract class IssueRecord: RecordBase
	{
		private static readonly Dictionary<IssueKind, RecordSeverity> RecordTypeSeverity = new Dictionary<IssueKind, RecordSeverity>
		{
			{IssueKind.MissingComponent, RecordSeverity.Error},
			{IssueKind.MissingPrefab, RecordSeverity.Error},
#if UNITY_2019_1_OR_NEWER
			{IssueKind.ShaderError, RecordSeverity.Error},
#endif
			{IssueKind.MissingReference, RecordSeverity.Warning},
			{IssueKind.DuplicateComponent, RecordSeverity.Warning},
			{IssueKind.InconsistentTerrainData, RecordSeverity.Warning},
			{IssueKind.UnnamedLayer, RecordSeverity.Info},
			{IssueKind.HugePosition, RecordSeverity.Warning},
			{IssueKind.DuplicateLayers, RecordSeverity.Info},
			{IssueKind.Other, RecordSeverity.Info},
			{IssueKind.Error, RecordSeverity.Error}
		};

		/// <summary>
		/// Describes found issue's kind.
		/// </summary>
		public IssueKind Kind { get; private set; }

		/// <summary>
		/// Describes found issue's severity.
		/// </summary>
		public RecordSeverity Severity { get; private set; }
		
		internal FixResult fixResult;

		/// <summary>
		/// Perform fix attempt. Call only if #IsFixable returns true.
		/// </summary>
		/// <param name="batchMode">Pass true when fixing more than 1 issue at a time to improve fixing performance using batch approach.</param>
		/// <returns>Fixing attempt result.</returns>
		public FixResult Fix(bool batchMode)
		{
			fixResult = PerformFix(batchMode);
			return fixResult;
		}

		/// <summary>
		/// Returns true if current issue type is potentially fixable, returns false otherwise.
		/// </summary>
		public abstract bool IsFixable { get; }

		// -----------------------------------------------------------------------------
		// base constructors
		// -----------------------------------------------------------------------------

		protected IssueRecord(IssueKind kind, RecordLocation location):base(location)
		{
			Kind = kind;
			Severity = RecordTypeSeverity[kind];
		}

		// -----------------------------------------------------------------------------
		// issue compact line generation
		// -----------------------------------------------------------------------------

		protected override void ConstructCompactLine(StringBuilder text)
		{
			ConstructHeader(text);
		}

		// -----------------------------------------------------------------------------
		// issue header generation
		// -----------------------------------------------------------------------------

		protected override void ConstructHeader(StringBuilder text)
		{
			switch (Kind)
			{
				case IssueKind.MissingComponent:
					text.Append(headerFormatArgument > 1 ? string.Format("{0} missing components", headerFormatArgument) : "Missing component");
					break;
				case IssueKind.MissingReference:
					text.Append("Missing reference");
					break;
				case IssueKind.DuplicateComponent:
					text.Append("Duplicate component");
					break;
				case IssueKind.MissingPrefab:
					text.Append("Instance of missing prefab");
					break;
				case IssueKind.UnnamedLayer:
					text.Append("GameObject with unnamed layer");
					break;
				case IssueKind.HugePosition:
					text.Append("GameObject with huge position");
					break;
				case IssueKind.InconsistentTerrainData:
					text.Append("Terrain and TerrainCollider with different Terrain Data");
					break;
				case IssueKind.DuplicateLayers:
					text.Append("Duplicate layer(s) found at the 'Tags and Layers' settings");
					break;
#if UNITY_2019_1_OR_NEWER
				case IssueKind.ShaderError:
					text.Append("Shader with error(s)");
					break;
#endif
				case IssueKind.Error:
					text.Append("Error!");
					break;
				case IssueKind.Other:
					text.Append("Other");
					break;
				default:
					text.Append("Unknown issue!");
					break;
			}
		}

		internal virtual FixResult PerformFix(bool batchMode)
		{
			return null;
		}
	}
}