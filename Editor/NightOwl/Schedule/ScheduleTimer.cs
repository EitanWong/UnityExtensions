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
using System.Timers;
using System.Diagnostics;

namespace NightOwl.Schedule
{
	/// <summary>
	/// ScheduleTimer represents a timer that fires on a more human friendly schedule.  For example it is easy to 
	/// set it to fire every day at 6:00PM.  It is useful for batch jobs or alarms that might be difficult to 
	/// schedule with the native .net timers.
	/// It is similar to the .net timer that it is based on with the start and stop methods functioning similarly.
	/// The main difference is the event uses a different delegate and arguement since the .net timer argument 
	/// class is not creatable.
	/// </summary>
	public class ScheduleTimerBase : IDisposable
	{
		public ScheduleTimerBase()
		{
			_Timer = new Timer();
			_Timer.AutoReset = false;
			_Timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
			_Jobs = new TimerJobList();
			_LastTime = DateTime.MaxValue;
		}

		/// <summary>
		/// Adds a job to the timer.  This method passes in a delegate and the parameters similar to the Invoke method of windows forms.
		/// </summary>
		/// <param name="Schedule">The schedule that this delegate is to be run on.</param>
		/// <param name="f">The delegate to run</param>
		/// <param name="Params">The method parameters to pass if you leave any DateTime parameters unbound, then they will be set with the scheduled run time of the 
		/// method.  Any unbound object parameters will get this Job object passed in.</param>
		public void AddJob(IScheduledItem Schedule, Delegate f, params object[] Params)
		{
			_Jobs.Add(new TimerJob(Schedule, new DelegateMethodCall(f, Params)));
		}

		/// <summary>
		/// Adds a job to the timer to operate asyncronously.
		/// </summary>
		/// <param name="Schedule">The schedule that this delegate is to be run on.</param>
		/// <param name="f">The delegate to run</param>
		/// <param name="Params">The method parameters to pass if you leave any DateTime parameters unbound, then they will be set with the scheduled run time of the 
		/// method.  Any unbound object parameters will get this Job object passed in.</param>
		public void AddAsyncJob(IScheduledItem Schedule, Delegate f, params object[] Params)
		{
			TimerJob Event = new TimerJob(Schedule, new DelegateMethodCall(f, Params));
			Event.SyncronizedEvent = false;
			_Jobs.Add(Event);
		}

		/// <summary>
		/// Adds a job to the timer.  
		/// </summary>
		/// <param name="Event"></param>
		public void AddJob(TimerJob Event)
		{
			_Jobs.Add(Event);
		}

		/// <summary>
		/// Clears out all scheduled jobs.
		/// </summary>
		public void ClearJobs()
		{
			_Jobs.Clear();
		}

		/// <summary>
		/// Begins executing all assigned jobs at the scheduled times
		/// </summary>
		public void Start()
		{
			_StopFlag = false;
			QueueNextTime(EventStorage.ReadLastTime());
		}

		/// <summary>
		/// Halts executing all jobs.  When the timer is restarted all jobs that would have run while the timer was stopped are re-tried.
		/// </summary>
		public void Stop()
		{
			_StopFlag = true;
			_Timer.Stop();
		}

		/// <summary>
		/// EventStorage determines the method used to store the last event fire time.  It defaults to keeping it in memory.
		/// </summary>
		public IEventStorage EventStorage = new LocalEventStorage();
		public event ExceptionEventHandler Error;

		#region Private Methods and Fields
		/// <summary>
		/// This is here to enhance accuracy.  Even if nothing is scheduled the timer sleeps for a maximum of 1 minute.
		/// </summary>
		private static TimeSpan MAX_INTERVAL = new TimeSpan(0, 1, 0);

		private DateTime _LastTime;
		private Timer _Timer;
		private TimerJobList _Jobs;
		private volatile bool _StopFlag;

		private double NextInterval(DateTime thisTime)
		{
			TimeSpan interval = _Jobs.NextRunTime(thisTime)-thisTime;
			if (interval > MAX_INTERVAL)
				interval = MAX_INTERVAL;
			//Handles the case of 0 wait time, the interval property requires a duration > 0.
			return (interval.TotalMilliseconds == 0) ? 1 : interval.TotalMilliseconds;
		}

		private void QueueNextTime(DateTime thisTime)
		{
			_Timer.Interval = NextInterval(thisTime);
			System.Diagnostics.Debug.WriteLine(_Timer.Interval);
			_LastTime = thisTime;
			EventStorage.RecordLastTime(thisTime);
			_Timer.Start();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try 
			{
				if (_Jobs == null)
					return;

				_Timer.Stop();

				foreach(TimerJob Event in _Jobs.Jobs)
				{
					try { Event.Execute(this, _LastTime, e.SignalTime, Error); }
					catch (Exception ex) { OnError(DateTime.Now, Event, ex); }
				}
			} 
			catch (Exception ex)
			{
				OnError(DateTime.Now, null, ex);
			}
			finally
			{
				if (_StopFlag == false)
					QueueNextTime(e.SignalTime);
			}
		}

		private void OnError(DateTime eventTime, TimerJob job, Exception e)
		{
			if (Error == null)
				return;

			try { Error(this, new ExceptionEventArgs(eventTime, e)); }
			catch (Exception) {}
		}
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_Timer != null)
				_Timer.Dispose();
		}

		#endregion
	}

	public class ScheduleTimer : ScheduleTimerBase
	{
		/// <summary>
		/// Add event is used in conjunction with the Elaspsed event handler.  Set the Elapsed handler, add your schedule and call start.
		/// </summary>
		/// <param name="Schedule">The schedule to fire the event at.  Adding additional schedules will cause the event to fire whenever either schedule calls for it.</param>
		public void AddEvent(IScheduledItem Schedule)
		{
			if (Elapsed == null)
				throw new ArgumentNullException("Elapsed", "member variable is null.");

			AddJob(new TimerJob(Schedule, new DelegateMethodCall(Elapsed)));
		}

		/// <summary>
		/// The event to fire when you only need to fire one event.
		/// </summary>
		public event ScheduledEventHandler Elapsed;
	}

	/// <summary>
	/// ExceptionEventArgs allows exceptions to be captured and sent to the OnError event of the timer.
	/// </summary>
	public class ExceptionEventArgs : EventArgs
	{
		public ExceptionEventArgs(DateTime eventTime, Exception e)
		{
			EventTime = eventTime;
			Error = e;
		}
		public DateTime EventTime;
		public Exception Error;
	}

	/// <summary>
	/// ExceptionEventHandler is the method type used by the OnError event for the timer.
	/// </summary>
	public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs Args);

	public class ScheduledEventArgs : EventArgs
	{
		public ScheduledEventArgs(DateTime eventTime)
		{
			EventTime = eventTime;
		}
		public DateTime EventTime;
	}

	public delegate void ScheduledEventHandler(object sender, ScheduledEventArgs e);

	/// <summary>
	/// The IResultFilter interface represents filters that either sort the events for an interval or
	/// remove duplicate events either selecting the first or the last event.
	/// </summary>
	public interface IResultFilter
	{
		void FilterResultsInInterval(DateTime Start, DateTime End, ArrayList List);
	}

	/// <summary>
	/// IEventStorage is used to provide persistance of schedule during service shutdowns.
	/// </summary>
	public interface IEventStorage
	{ 
		void RecordLastTime(DateTime Time);
		DateTime ReadLastTime();
	}


}