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
	/// There have been quite a few requests to allow scheduling of multiple delegates and method parameter data
	/// from the same timer.  This class allows you to match the event with the time that it fired.  I want to keep
	/// the same simple implementation of the EventQueue and interval classes since they can be reused elsewhere.
	/// The timer should be responsible for matching this data up.
	/// </summary>
	public class EventInstance : IComparable
	{
		public EventInstance(DateTime time, IScheduledItem scheduleItem, object data)
		{
			Time = time;
			ScheduleItem = scheduleItem;
			Data = data;
		}
		public DateTime Time;
		public IScheduledItem ScheduleItem;
		public object Data;

		public int CompareTo(object obj)
		{
			if (obj is EventInstance)
				return Time.CompareTo(((EventInstance)obj).Time);
			if (obj is DateTime)
				return Time.CompareTo((DateTime)obj);
			return 0;
		}
	}

}
 
 
