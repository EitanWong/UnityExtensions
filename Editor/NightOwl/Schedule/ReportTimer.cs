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

namespace NightOwl.Schedule
{
	public class ReportEventArgs : EventArgs
	{
		public ReportEventArgs(DateTime Time, int reportNo) { EventTime = Time; ReportNo = reportNo; }
		public int ReportNo;
		public DateTime EventTime;
	}

	public delegate void ReportEventHandler(object sender, ReportEventArgs e);

	/// <summary>
	/// Summary description for ReportTimer.
	/// </summary>
	public class ReportTimer : ScheduleTimerBase
	{
		public void AddReportEvent(IScheduledItem Schedule, int reportNo)
		{
			if (Elapsed == null)
				throw new Exception("You must set elapsed before adding Events");
			AddJob(new TimerJob(Schedule, new DelegateMethodCall(Handler, Elapsed, reportNo)));
		}

		public void AddAsyncReportEvent(IScheduledItem Schedule, int reportNo)
		{
			if (Elapsed == null)
				throw new Exception("You must set elapsed before adding Events");
			TimerJob Event = new TimerJob(Schedule, new DelegateMethodCall(Handler, Elapsed, reportNo));
			Event.SyncronizedEvent = false;
			AddJob(Event);
		}

		public event ReportEventHandler Elapsed;

		delegate void ConvertHandler(ReportEventHandler Handler, int ReportNo, object sender, DateTime time);
		static ConvertHandler Handler = new ConvertHandler(Converter);
		static void Converter(ReportEventHandler Handler, int ReportNo, object sender, DateTime time)
		{
			if (Handler == null)
				throw new ArgumentNullException("Handler");
			if (sender == null)
				throw new ArgumentNullException("sender");
			Handler(sender, new ReportEventArgs(time, ReportNo));
		}
	}
}
