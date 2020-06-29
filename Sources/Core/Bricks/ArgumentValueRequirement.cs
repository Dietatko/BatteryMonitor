using System;
using System.Collections;

namespace ImpruvIT.Contracts
{
	/// <summary>
	/// Defines contract requirements for method argument.
	/// </summary>
	/// <typeparam name="T">Type of the method argument.</typeparam>
	public class ArgumentValueRequirement<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArgumentValueRequirement{T}"/> class with specified value and argument name.
		/// </summary>
		/// <param name="value">Value of the contract requirement parameter.</param>
		/// <param name="argumentName">Name of the contract requirement parameter.</param>
		public ArgumentValueRequirement(T value, string argumentName)
		{
			if (String.IsNullOrEmpty(argumentName))
				throw new ArgumentNullException(argumentName);

			Value = value;
			ArgumentName = argumentName;
		}


		/// <summary>
		/// Gets the value of the contract requirement parameter.
		/// </summary>
		public T Value { get; }

		/// <summary>
		/// Gets the name of the contract requirement parameter.
		/// </summary>
		public string ArgumentName { get; }


		/// <summary>
		/// Validates that argument value is not null. This contract requirement has sense only for reference types.
		/// </summary>
		/// <returns>This <see cref="ArgumentValueRequirement{T}"/> instance.</returns>
		public ArgumentValueRequirement<T> NotToBeNull()
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull - Calling this method has sense only for reference types
			if (Value == null)
				throw new ArgumentNullException(ArgumentName);
			// ReSharper restore CompareNonConstrainedGenericWithNull

			return this;
		}

		/// <summary>
		/// Validates that argument value is not empty. This contract requirement has sense only for enumerable types.
		/// </summary>
		/// <returns>This <see cref="ArgumentValueRequirement{T}"/> instance.</returns>
		public ArgumentValueRequirement<T> NotToBeEmpty()
		{
			NotToBeNull();

			if (!(Value is IEnumerable enumerable))
				throw new ArgumentException($"Argument '{ArgumentName}' is not enumerable.", ArgumentName);
			if (!enumerable.GetEnumerator().MoveNext())
				throw new ArgumentException($"Argument '{ArgumentName}' cannot be empty.", ArgumentName);

			return this;
		}

		/// <summary>
		/// Validates that argument value is in required range.
		/// </summary>
		/// <param name="predicate">The predicate validating parameter value.</param>
		/// <returns>This <see cref="ArgumentValueRequirement{T}"/> instance.</returns>
		public ArgumentValueRequirement<T> ToBeInRange(Func<T, bool> predicate)
		{
			Contract.Requires(predicate, nameof(predicate))
				.NotToBeNull();

			if (!predicate(this.Value))
				throw new ArgumentOutOfRangeException(ArgumentName, Value, "The parameter is out of required range.");

			return this;
		}

		/// <summary>
		/// Validates that argument value is of specified type.
		/// </summary>
		/// <returns>This <see cref="ArgumentValueRequirement{T}"/> instance.</returns>
		public ArgumentValueRequirement<T> ToBeOfType<TArg>()
		{
			return ToBeOfType(typeof(TArg));
		}

		/// <summary>
		/// Validates that argument value is of specified type.
		/// </summary>
		/// <returns>This <see cref="ArgumentValueRequirement{T}"/> instance.</returns>
		public ArgumentValueRequirement<T> ToBeOfType(Type type)
		{
			Contract.Requires(type, nameof(type))
				.NotToBeNull();

			if (!type.IsInstanceOfType(Value))
				throw new ArgumentException($"Argument value has to be of type '{type.FullName}'.", ArgumentName);

			return this;
		}

		/// <summary>
		/// Validates that argument value satisfy specified conditions and throws specified exception if not.
		/// </summary>
		/// <param name="predicate">Predicate that verifies argument value.</param>
		/// <param name="exceptionFactory">Factory of exception to throw.</param>
		/// <returns>This <see cref="ArgumentValueRequirement{T}"/> instance.</returns>
		public ArgumentValueRequirement<T> ToSatisfy(Func<T, bool> predicate, Func<T, Exception> exceptionFactory)
		{
			if (!predicate(Value))
				throw exceptionFactory(Value);

			return this;
		}

		/// <summary>
		/// Validates that argument value is a defined enumeration value.
		/// </summary>
		/// <typeparam name="T">The type of enum.</typeparam>
		/// <returns>This <see cref="ArgumentValueRequirement{T}" /> instance.</returns>
		public ArgumentValueRequirement<T> ToBeDefinedEnumValue()
		{
			var argType = typeof(T);
			if (!argType.IsEnum)
				throw new InvalidOperationException("The value type is not an enumeration type.");

			if (!argType.IsEnumDefined(Value))
				throw new ArgumentOutOfRangeException(ArgumentName, Value, "The specified enum value is not defined in the enumeration.");

			return this;
		}
	}
}
