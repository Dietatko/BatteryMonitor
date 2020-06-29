using System;
using System.Linq.Expressions;

namespace ImpruvIT.Contracts
{
	/// <summary>
	/// Contract specification factory class.
	/// </summary>
	public static class Contract
	{
		/// <summary>
		/// Creates argument value requirement.
		/// </summary>
		/// <typeparam name="T">Type of argument value.</typeparam>
		/// <param name="value">The actual value of the argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		/// <returns>A new <see cref="ArgumentValueRequirement{T}">argument value requirement</see>.</returns>
		public static ArgumentValueRequirement<T> Requires<T>(T value, string argumentName)
		{
			return new ArgumentValueRequirement<T>(value, argumentName);
		}

		/// <summary>
		/// Creates argument value requirement.
		/// </summary>
		/// <typeparam name="T">Type of argument value.</typeparam>
		/// <param name="valueExpression">The argument expression.</param>
		/// <returns>A new <see cref="ArgumentValueRequirement{T}">argument value requirement</see>.</returns>
		/// <exception cref="System.ArgumentException">Only member expressions are supported.</exception>
		public static ArgumentValueRequirement<T> Requires<T>(Expression<Func<T>> valueExpression)
		{
			Contract.Requires(valueExpression, nameof(valueExpression))
				.NotToBeNull();

			T value = valueExpression.Compile()();

			if (!(valueExpression.Body is MemberExpression bodyExpr))
				throw new ArgumentException("Only member expressions are supported.");

			var memberExpr = bodyExpr.Member;
			var argName = memberExpr.Name;

			return new ArgumentValueRequirement<T>(value, argName);
		}
	}
}
