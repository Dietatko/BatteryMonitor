using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ImpruvIT.Contracts;

namespace ImpruvIT.Diagnostics
{
	/// <summary>
	/// The helper for creating formatted trace (and other) messages.
	/// </summary>
	public class TraceBuilder
	{
		private readonly StringBuilder stringBuilder;
		private int indent;
		private bool beginningOfLine;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceBuilder"/> class.
		/// </summary>
		public TraceBuilder()
		{
			stringBuilder = new StringBuilder();
			beginningOfLine = true;
		}

		/// <summary>
		/// Appends the specified formatted text to the message.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		public TraceBuilder Append(string format, params object[] args)
		{
			Contract.Requires(format, nameof(format)).NotToBeNull();
			Contract.Requires(args, nameof(args)).NotToBeNull();

			// Format arguments
			var argStrings = FormatArgs(args).ToArray();

			// Append text
			var stringToAppend = string.Format(CultureInfo.InvariantCulture, format, argStrings);
			var linesToAppend = stringToAppend.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			if (linesToAppend.Length > 0)
			{
				AppendInternal(linesToAppend[0]);
				for (var i = 1; i < linesToAppend.Length; i++)
				{
					AppendLine();
					AppendInternal(linesToAppend[i]);
				}
			}

			return this;
		}

		/// <summary>
		/// Appends either true or false text depending on the condition.
		/// </summary>
		/// <param name="condition">The condition that decides which text is appended.</param>
		/// <param name="trueText">The text appended when condition is <b>true</b>.</param>
		/// <param name="falseText">The text appended when condition is <b>false</b>.</param>
		/// <returns></returns>
		public TraceBuilder AppendConditional(bool condition, string trueText, string falseText = null)
		{
			Contract.Requires(trueText, nameof(trueText)).NotToBeNull();

			if (condition)
				Append(trueText);
			else if (falseText != null)
				Append(falseText);

			return this;
		}

		/// <summary>
		/// Appends an empty line.
		/// </summary>
		/// <returns></returns>
		public TraceBuilder AppendLine()
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
				beginningOfLine = true;
			}

			return this;
		}

		/// <summary>
		/// Appends the specified formatted text on a new line to the message.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		public TraceBuilder AppendLine(string format, params object[] args)
		{
			return AppendLine().Append(format, args);
		}

		/// <summary>
		/// Appends either true or false text on a new line depending on the condition.
		/// </summary>
		/// <param name="condition">The condition that decides which text is appended.</param>
		/// <param name="trueText">The text appended when condition is <b>true</b>.</param>
		/// <param name="falseText">The text appended when condition is <b>false</b>.</param>
		/// <returns></returns>
		public TraceBuilder AppendLineConditional(bool condition, string trueText, string falseText = null)
		{
			Contract.Requires(trueText, nameof(trueText)).NotToBeNull();

			if (condition)
				AppendLine(trueText);
			else if (falseText != null)
				AppendLine(falseText);

			return this;
		}

		/// <summary>
		/// Appends a formatted line for each item in set.
		/// </summary>
		/// <typeparam name="T">The type of items in the set.</typeparam>
		/// <param name="set">The set of items.</param>
		/// <param name="format">The format.</param>
		/// <param name="argFormatters">The items formatters producing formatted arguments.</param>
		/// <returns></returns>
		public TraceBuilder AppendLineForEach<T>(IEnumerable<T> set, string format, params Func<T, object>[] argFormatters)
		{
			Contract.Requires(set, nameof(set)).NotToBeNull();
			Contract.Requires(format, nameof(format)).NotToBeNull();

			foreach (var item in set)
			{
				var tmpItem = item;
				AppendLine(format, argFormatters.Select(f => f(tmpItem)).ToArray());
			}

			return this;
		}

		private void AppendInternal(string text)
		{
			if (beginningOfLine)
			{
				AppendIndent();
				beginningOfLine = false;
			}
			stringBuilder.Append(text);
		}


		/// <summary>
		/// Executes specified <see cref="TraceBuilder" /> action for every item in the set.
		/// </summary>
		/// <typeparam name="T">The type of items in the set.</typeparam>
		/// <param name="set">The set of items.</param>
		/// <param name="actions">The action to execute.</param>
		/// <returns></returns>
		public TraceBuilder ForEach<T>(IEnumerable<T> set, Action<TraceBuilder, T> actions)
		{
			Contract.Requires(set, nameof(set)).NotToBeNull();
			Contract.Requires(actions, nameof(actions)).NotToBeNull();

			foreach (var item in set)
				actions(this, item);

			return this;
		}

		/// <summary>
		/// Executes either true or false <see cref="TraceBuilder" /> action.
		/// </summary>
		/// <param name="condition">The condition that decides which action is executed.</param>
		/// <param name="trueAction">The action that is executed when condition is <b>true</b>.</param>
		/// <param name="falseAction">The action that is executed when condition is <b>false</b>.</param>
		/// <returns></returns>
		public TraceBuilder ExecuteConditional(bool condition, Action<TraceBuilder> trueAction, Action<TraceBuilder> falseAction = null)
		{
			Contract.Requires(trueAction, nameof(trueAction)).NotToBeNull();

			if (condition)
				trueAction(this);
			else
				falseAction?.Invoke(this);

			return this;
		}


		/// <summary>
		/// Indents following messages.
		/// </summary>
		/// <returns></returns>
		public TraceBuilder Indent()
		{
			indent++;
			return this;
		}

		/// <summary>
		/// Unindents following messages.
		/// </summary>
		/// <returns></returns>
		public TraceBuilder Unindent()
		{
			indent = Math.Max(indent - 1, 0);
			return this;
		}

		/// <summary>
		/// Executes specified <see cref="TraceBuilder" /> action with increased indentation. Previous identation is restored after action is executed.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		/// <returns></returns>
		public TraceBuilder Indent(Action<TraceBuilder> action)
		{
			Contract.Requires(action, nameof(action)).NotToBeNull();

			Indent();
			action(this);
			Unindent();

			return this;
		}

		private void AppendIndent()
		{
			for (var i = indent; i > 0; i--)
				stringBuilder.Append("   ");
		}


		/// <summary>
		/// Outputs the current trace message.
		/// </summary>
		/// <returns>The trace message.</returns>
		public string Trace()
		{
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Outputs the current trace message.
		/// </summary>
		/// <returns>The trace message.</returns>
		public override string ToString()
		{
			return Trace();
		}

		private static IEnumerable<string> FormatArgs(IEnumerable<object> args)
		{
			return args.Select(x =>
				x != null
					? x is ITraceable traceable ? traceable.Trace() : x.ToString()
					: "<NULL>");
		}
	}
}
