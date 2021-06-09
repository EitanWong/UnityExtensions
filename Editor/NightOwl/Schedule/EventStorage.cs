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
using System.Xml;
using System.Xml.XPath;

namespace NightOwl.Schedule
{
	/// <summary>
	/// Null event strorage disables error recovery by returning now for the last time an event fired.
	/// </summary>
	public class NullEventStorage : IEventStorage
	{
		public NullEventStorage()
		{
		}

		public void RecordLastTime(DateTime Time)
		{
		}

		public DateTime ReadLastTime()
		{
			return DateTime.Now;
		}
	}

	/// <summary>
	/// Local event strorage keeps the last time in memory so that skipped events are not recovered.
	/// </summary>
	public class LocalEventStorage : IEventStorage
	{
		public LocalEventStorage()
		{
			_LastTime = DateTime.MaxValue;
		}

		public void RecordLastTime(DateTime Time)
		{
			_LastTime = Time;
		}

		public DateTime ReadLastTime()
		{
			if (_LastTime == DateTime.MaxValue)
				_LastTime = DateTime.Now;
			return _LastTime;
		}

		DateTime _LastTime;
	}

	/// <summary>
	/// FileEventStorage saves the last time in an XmlDocument so that recovery will include periods that the 
	/// process is shutdown.
	/// </summary>
	public class FileEventStorage : IEventStorage
	{
		public FileEventStorage(string FileName, string XPath)
		{
			_FileName = FileName;
			_XPath = XPath;
		}

		public void RecordLastTime(DateTime Time)
		{
			_Doc.SelectSingleNode(_XPath).Value = Time.ToString();
			_Doc.Save(_FileName);
		}

		public DateTime ReadLastTime()
		{
			_Doc.Load(_FileName);
			string Value = _Doc.SelectSingleNode(_XPath).Value;
			if (Value == null || Value == string.Empty)
				return DateTime.Now;
			return DateTime.Parse(Value);
		}

		string _FileName;
		string _XPath;
		XmlDocument _Doc = new XmlDocument();
	}
}
