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
using System.Diagnostics;

namespace NightOwl.Schedule.ScheduledItems
{
	/// <summary>
	/// The simple interval represents the simple scheduling that .net supports natively.  It consists of a start
	/// absolute time and an interval that is counted off from the start time.
	/// </summary>
	[Serializable]
	public class SimpleInterval : IScheduledItem
	{
		public SimpleInterval(DateTime StartTime, TimeSpan Interval)
		{
			_Interval = Interval;
			_StartTime = StartTime;
			_EndTime = DateTime.MaxValue;
		}
		public SimpleInterval(DateTime StartTime, TimeSpan Interval, int count)
		{
			_Interval = Interval;
			_StartTime = StartTime;
			_EndTime = StartTime + TimeSpan.FromTicks(Interval.Ticks*count);
		}
		public SimpleInterval(DateTime StartTime, TimeSpan Interval, DateTime EndTime)
		{
			_Interval = Interval;
			_StartTime = StartTime;
			_EndTime = EndTime;
		}
		public void AddEventsInInterval(DateTime Begin, DateTime End, ArrayList List)
		{
			if (End <= _StartTime)
				return;
			DateTime Next = NextRunTime(Begin, true);
			while (Next < End)
			{
				List.Add(Next);
				Next = NextRunTime(Next, false);
			}
		}

		public DateTime NextRunTime(DateTime time, bool AllowExact)
		{
			DateTime returnTime = NextRunTimeInt(time, AllowExact);
			Debug.WriteLine(time);
			Debug.WriteLine(returnTime);
			Debug.WriteLine(_EndTime);
			return (returnTime >= _EndTime) ? DateTime.MaxValue : returnTime;
		}

		private DateTime NextRunTimeInt(DateTime time, bool AllowExact)
		{
			TimeSpan Span = time-_StartTime;
			if (Span < TimeSpan.Zero)
				return _StartTime;
			if (ExactMatch(time))
				return AllowExact ? time : time + _Interval;
			uint msRemaining = (uint)(_Interval.TotalMilliseconds - ((uint)Span.TotalMilliseconds % (uint)_Interval.TotalMilliseconds));
			return time.AddMilliseconds(msRemaining);
		}

		private bool ExactMatch(DateTime time)
		{
			TimeSpan Span = time-_StartTime;
			if (Span < TimeSpan.Zero)
				return false;
			return (Span.TotalMilliseconds % _Interval.TotalMilliseconds) == 0;
		}

		private TimeSpan _Interval;
		private DateTime _StartTime;
		private DateTime _EndTime;
	}
}