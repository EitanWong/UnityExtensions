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
	/// IScheduledItem represents a scheduled event.  You can query it for the number of events that occur
	/// in a time interval and for the remaining interval before the next event.
	/// </summary>
	public interface IScheduledItem
	{
		/// <summary>
		/// Returns the times of the events that occur in the given time interval.  The interval is closed
		/// at the start and open at the end so that intervals can be stacked without overlapping.
		/// </summary>
		/// <param name="Begin">The beginning of the interval</param>
		/// <param name="End">The end of the interval</param>
		/// <returns>All events >= Begin and &lt; End </returns>
		void AddEventsInInterval(DateTime Begin, DateTime End, ArrayList List);

		/// <summary>
		/// Returns the next run time of the scheduled item.  Optionally excludes the starting time.
		/// </summary>
		/// <param name="time">The starting time of the interval</param>
		/// <param name="IncludeStartTime">if true then the starting time is included in the query false, it is excluded.</param>
		/// <returns>The next execution time either on or after the starting time.</returns>
		DateTime NextRunTime(DateTime time, bool IncludeStartTime);
	}

}
