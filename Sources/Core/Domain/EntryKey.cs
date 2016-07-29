using System;
using System.Diagnostics;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	[DebuggerDisplay("{NamespaceUri}:{Name}")]
	public class EntryKey : IEquatable<EntryKey>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntryKey"/> class.
		/// </summary>
		/// <param name="namespaceUri">The NamespaceUriName URI.</param>
		/// <param name="name">The entry name.</param>
		public EntryKey(string namespaceUri, string name)
		{
			Contract.Requires(namespaceUri, "NamespaceUriName")
				.NotToBeNull();
			Contract.Requires(name, "Name")
				.NotToBeNull()
				.NotToBeEmpty();

			this.NamespaceUri = namespaceUri;
			this.Name = name;
		}

		public string NamespaceUri { get; private set; }
		public string Name { get; private set; }

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <returns><b>true</b> if the specified object is equal to the current object; otherwise, <b>false</b>.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj)
		{
			var other = obj as EntryKey;
			if (other == null)
				return false;

			return this.Equals(other);
		}

		public bool Equals(EntryKey other)
		{
			return Object.Equals(this.NamespaceUri, other.NamespaceUri) && object.Equals(this.Name, other.Name);
		}

		/// <summary>
		/// Serves as the default hash function. 
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.NamespaceUri.GetHashCode() ^ this.Name.GetHashCode();
		}
	}
}
