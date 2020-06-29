using System;

namespace ImpruvIT
{
	/// <summary>
	/// Represents a generic subscription.
	/// </summary>
	public interface ISubscription : IDisposable
	{
		/// <summary>
		/// Cancels the subscription.
		/// </summary>
		void Unsubscribe();
	}
}
