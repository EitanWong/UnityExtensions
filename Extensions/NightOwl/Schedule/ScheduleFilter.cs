/***************************************************************************
 * Copyright Andy Brummer 2004-2005
 * 
 * This code is provided "as is", with absolutely no warranty expressed
 * or implied. Any use is at your own risk.
 *
 * This code may be used in compiled form in any way you desire. This
 * file may be redistributed unmodified by any means provided it is
 * not sold for profit without the authors written consent, and
 * providing that this notice and the authors name is included. If
 * the source code in  this file is used in any commercial application
 * then a simple email would be nice.
 * 
 **************************************************************************/

using System;
using System.Collections;

namespace NightOwl.Schedule
{
	/// <summary>
	/// This is an empty filter that does not filter any of the events.
	/// </summary>
	public class Filter : IResultFilter
	{
		public static IResultFilter Empty = new Filter();
		private Filter() {}

		public void FilterResultsInInterval(DateTime Start, DateTime End, ArrayList List)
		{
			if (List == null)
				return;
			List.Sort();
		}
	}

	/// <summary>
	/// This causes only the first event of the interval to be counted.
	/// </summary>
	public class FirstEventFilter : IResultFilter
	{
		public static IResultFilter Filter = new FirstEventFilter();
		private FirstEventFilter() {}

		public void FilterResultsInInterval(DateTime Start, DateTime End, ArrayList List)
		{
			if (List == null)
				return;
			if (List.Count < 2)
				return;
			List.Sort();
			List.RemoveRange(1, List.Count-1);
		}
	}

	/// <summary>
	/// This causes only the last event of the interval to be counted.
	/// </summary>
	public class LastEventFilter : IResultFilter
	{
		public static IResultFilter Filter = new LastEventFilter();
		private LastEventFilter() {}

		public void FilterResultsInInterval(DateTime Start, DateTime End, ArrayList List)
		{
			if (List == null)
				return;
			if (List.Count < 2)
				return;
			List.Sort();
			List.RemoveRange(0, List.Count-1);
		}
	}

}
