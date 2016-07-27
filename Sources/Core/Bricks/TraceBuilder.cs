using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor
{
	public class TraceBuilder
	{
		private readonly StringBuilder m_stringBuilder;
		private int m_indent;
		private bool m_beginningOfLine;

		public TraceBuilder()
		{
			this.m_stringBuilder = new StringBuilder();
			this.m_beginningOfLine = true;
		}

		public TraceBuilder Append(string format, params object[] args)
		{
			Contract.Requires(format, "format").IsNotNull();
			Contract.Requires(args, "args").IsNotNull();

			// Format arguments
			string[] argStrings = FormatArgs(args).ToArray();

			// Append text
			string stringToAppend = string.Format(CultureInfo.InvariantCulture, format, argStrings);
			string[] linesToAppend = stringToAppend.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			if (linesToAppend.Length > 0)
			{
				this.AppendInternal(linesToAppend[0]);
				for (int i = 1; i < linesToAppend.Length; i++)
				{
					this.AppendLine();
					this.AppendInternal(linesToAppend[i]);
				}
			}

			return this;
		}

		public TraceBuilder AppendConditional(bool condition, string trueText, string falseText = null)
		{
			Contract.Requires(trueText, "trueText").IsNotNull();

			if (condition)
				this.Append(trueText);
			else if (falseText != null)
				this.Append(falseText);

			return this;
		}

		public TraceBuilder AppendLine()
		{
			if (this.m_stringBuilder.Length > 0)
			{
				this.m_stringBuilder.AppendLine();
				this.m_beginningOfLine = true;
			}

			return this;
		}

		public TraceBuilder AppendLine(string format, params object[] args)
		{
			return this.AppendLine().Append(format, args);
		}

		public TraceBuilder AppendLineConditional(bool condition, string trueText, string falseText = null)
		{
			Contract.Requires(trueText, "trueText").IsNotNull();

			if (condition)
				this.AppendLine(trueText);
			else if (falseText != null)
				this.AppendLine(falseText);

			return this;
		}

		public TraceBuilder AppendLineForEach<T>(IEnumerable<T> set, string format, params Func<T, object>[] args)
		{
			Contract.Requires(set, "set").IsNotNull();
			Contract.Requires(format, "format").IsNotNull();

			foreach (T item in set)
			{
				var tmpItem = item;
				this.AppendLine(format, args.Select(f => f(tmpItem)));
			}

			return this;
		}

		private void AppendInternal(string text)
		{
			if (this.m_beginningOfLine)
			{
				this.AppendIndent();
				this.m_beginningOfLine = false;
			}
			this.m_stringBuilder.Append(text);
		}


		public TraceBuilder ForEach<T>(IEnumerable<T> set, Action<TraceBuilder, T> actions)
		{
			Contract.Requires(set, "set").IsNotNull();
			Contract.Requires(actions, "actions").IsNotNull();

			foreach (T item in set)
				actions(this, item);

			return this;
		}

		public TraceBuilder ExecuteConditional(bool condition, Action<TraceBuilder> trueAction, Action<TraceBuilder> falseAction = null)
		{
			Contract.Requires(trueAction, "trueAction").IsNotNull();

			if (condition)
				trueAction(this);
			else if (falseAction != null)
				falseAction(this);

			return this;
		}

		
		public TraceBuilder Indent()
		{
			this.m_indent++;
			return this;
		}

		public TraceBuilder Unindent()
		{
			this.m_indent = Math.Max(this.m_indent - 1, 0);
			return this;
		}

		public TraceBuilder Indent(Action<TraceBuilder> action)
		{
			Contract.Requires(action, "actions").IsNotNull();

			this.Indent();
			action(this);
			this.Unindent();

			return this;
		}

		private void AppendIndent()
		{
			for (int i = this.m_indent; i > 0; i--)
				this.m_stringBuilder.Append("   ");
		}


		public string Trace()
		{
			return this.m_stringBuilder.ToString();
		}

		public override string ToString()
		{
			return this.Trace();
		}

		private static IEnumerable<string> FormatArgs(IEnumerable<object> args)
		{
			return args.Select(x =>
				x != null
					? x is ITraceable ? ((ITraceable)x).Trace(TraceDetail.Details) : x.ToString()
					: "<NULL>");
		}
	}

	public interface ITraceable
	{
		string Trace(TraceDetail traceDetail = TraceDetail.Details);
	}

	public enum TraceDetail
	{
		Identifier,
		Details,
		Debug
	}
}
