using System;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class EntryIdentifier : IEquatable<EntryIdentifier>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntryIdentifier"/> class.
		/// </summary>
		/// <param name="namespaceUri">The namespace URI.</param>
		/// <param name="entryName">The entry name.</param>
		public EntryIdentifier(string namespaceUri, string entryName)
		{
			Contract.Requires(namespaceUri, "namespaceUri")
				.IsNotNull();
			Contract.Requires(entryName, "entryName")
				.IsNotNull()
				.IsNotEmpty();

			this.NamespaceUri = namespaceUri;
			this.EntryName = entryName;
		}

		public string NamespaceUri { get; private set; }
		public string EntryName { get; private set; }

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <returns><b>true</b> if the specified object is equal to the current object; otherwise, <b>false</b>.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj)
		{
			var other = obj as EntryIdentifier;
			if (other == null)
				return false;

			return this.Equals(other);
		}

		public bool Equals(EntryIdentifier other)
		{
			return Object.Equals(this.NamespaceUri, other.NamespaceUri) && object.Equals(this.EntryName, other.EntryName);
		}

		/// <summary>
		/// Serves as the default hash function. 
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.NamespaceUri.GetHashCode() ^ this.EntryName.GetHashCode();
		}
	}
}
