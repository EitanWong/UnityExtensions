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

namespace NightOwl.Schedule.ScheduledItems
{
	/// <summary>Single event represents an event which only fires once.</summary>
	public class SingleEvent : IScheduledItem
	{
		public SingleEvent(DateTime eventTime)
		{
			_EventTime = eventTime;
		}
		#region IScheduledItem Members

		public void AddEventsInInterval(DateTime Begin, DateTime End, System.Collections.ArrayList List)
		{
			if (Begin <= _EventTime && End > _EventTime)
				List.Add(_EventTime);
		}

		public DateTime NextRunTime(DateTime time, bool IncludeStartTime)
		{
			if (IncludeStartTime)
				return (_EventTime >= time) ? _EventTime : DateTime.MaxValue;
			else
				return (_EventTime >  time) ? _EventTime : DateTime.MaxValue;
		}
		private DateTime _EventTime;

		#endregion
	}
}
