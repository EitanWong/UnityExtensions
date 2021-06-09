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
	/// This class will be used to implement a filter that enables a window of activity.  For cases where you want to 
	/// run every 15 minutes between 6:00 AM and 5:00 PM.  Or just on weekdays or weekends.
	/// </summary>
	public class BlockWrapper : IScheduledItem
	{
		public BlockWrapper(IScheduledItem item, string StrBase, string BeginOffset, string EndOffset)
		{
			_Item = item;
			_Begin = new ScheduledTime(StrBase, BeginOffset);
			_End = new ScheduledTime(StrBase, EndOffset);
		}
		public void AddEventsInInterval(DateTime Begin, DateTime End, ArrayList List)
		{
			DateTime Next = NextRunTime(Begin, true);
			while (Next < End)
			{
				List.Add(Next);
				Next = NextRunTime(Next, false);
			}
		}

		public DateTime NextRunTime(DateTime time, bool AllowExact)
		{
			return NextRunTime(time, 100, AllowExact);
		}

		DateTime NextRunTime(DateTime time, int count, bool AllowExact)
		{
			if (count == 0)
				throw new Exception("Invalid block wrapper combination.");

			DateTime
				temp = _Item.NextRunTime(time, AllowExact),
				begin = _Begin.NextRunTime(time, true),
				end = _End.NextRunTime(time, true);
			System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2} {3}", time, begin, end, temp));
			bool A = temp > end, B = temp < begin, C = end < begin;
			System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2}", A, B, C));
			if (C)
			{
				if (A && B)
					return NextRunTime(begin, --count, false);
				else
					return temp;
			} 
			else
			{
				if (!A && !B)
					return temp;
				if (!A)
					return NextRunTime(begin, --count, false);
				else
					return NextRunTime(end, --count, false);
			}
		}
		private IScheduledItem _Item;
		private ScheduledTime _Begin, _End;
	}
}