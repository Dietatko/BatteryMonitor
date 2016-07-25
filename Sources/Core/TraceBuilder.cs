using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImpruvIT.BatteryMonitor
{
	public class TraceBuilder
	{
		private readonly StringBuilder m_builder;

		public TraceBuilder()
		{
			this.m_builder = new StringBuilder();
		}

		public TraceBuilder Append(string format, params object[] values)
		{
			this.m_builder.Append(String.Format(format, values));
			return this;
		}

		public TraceBuilder AppendLine(string format, params object[] values)
		{
			this.m_builder.AppendLine(String.Format(format, values));
			return this;
		}

		#region Indentation

		public TraceBuilder Indent()
		{
			return this;
		}

		public TraceBuilder Unindent()
		{
			return this;
		}

		#endregion Indentation

		public string Trace()
		{
			return this.m_builder.ToString();
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return this.m_builder.ToString();
		}
	}
}
