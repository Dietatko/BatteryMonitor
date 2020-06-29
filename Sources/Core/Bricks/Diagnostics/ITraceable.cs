namespace ImpruvIT.Diagnostics
{
	/// <summary>
	/// Denotes and object that can trace details about itself.
	/// </summary>
	public interface ITraceable
	{
		/// <summary>
		/// Traces the details about this.
		/// </summary>
		/// <param name="traceDetail">The detail level to trace out.</param>
		/// <returns>The trace string.</returns>
		string Trace(TraceDetail traceDetail = TraceDetail.Details);
	}
}
