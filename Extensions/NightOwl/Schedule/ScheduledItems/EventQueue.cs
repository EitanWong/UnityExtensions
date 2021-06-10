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

namespace NightOwl.Schedule.ScheduledItems
{
	/// <summary>
	/// The event queue is a collection of scheduled items that represents the union of all child scheduled items.
	/// This is useful for events that occur every 10 minutes or at multiple intervals not covered by the simple
	/// scheduled items.
	/// </summary>
	public class EventQueue : IScheduledItem
	{
		public EventQueue()
		{
			_List = new ArrayList();
		}
		/// <summary>
		/// Adds a ScheduledTime to the queue.
		/// </summary>
		/// <param name="time">The scheduled time to add</param>
		public void Add(IScheduledItem time)
		{
			_List.Add(time);
		}

		/// <summary>
		/// Clears the list of scheduled times.
		/// </summary>
		public void Clear()
		{
			_List.Clear();
		}

		/// <summary>
		/// Adds the running time for all events in the list.
		/// </summary>
		/// <param name="Begin">The beginning time of the interval</param>
		/// <param name="End">The end time of the interval</param>
		/// <param name="List">The list to add times to.</param>
		public void AddEventsInInterval(DateTime Begin, DateTime End, ArrayList List)
		{
			foreach(IScheduledItem st in _List)
				st.AddEventsInInterval(Begin, End, List);
			List.Sort();
		}

		/// <summary>
		/// Returns the first time after the starting time for all events in the list.
		/// </summary>
		/// <param name="time">The starting time.</param>
		/// <param name="AllowExact">If this is true then it allows the return time to match the time parameter, false forces the return time to be greater then the time parameter</param>
		/// <returns>Either the next event after the input time or greater or equal to depending on the AllowExact parameter.</returns>
		public DateTime NextRunTime(DateTime time, bool AllowExact)
		{
			DateTime next = DateTime.MaxValue;
			//Get minimum datetime from the list.
			foreach(IScheduledItem st in _List)
			{
				DateTime Proposed = st.NextRunTime(time, AllowExact);
				next = (Proposed < next) ? Proposed : next;
			}
			return next;
		}
		private ArrayList _List;
	}	 
}