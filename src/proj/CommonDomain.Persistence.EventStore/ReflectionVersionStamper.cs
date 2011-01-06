namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;

	public sealed class ReflectionVersionStamper : IStampAggregateVersion
	{
		private const string DefaultPropertyName = "Version";
		private static readonly IDictionary<Type, PropertyInfo> Cache = new Dictionary<Type, PropertyInfo>();
		private readonly string propertyName;

		public ReflectionVersionStamper()
			: this(null)
		{
		}
		public ReflectionVersionStamper(string propertyName)
		{
			this.propertyName = propertyName ?? DefaultPropertyName;
		}

		public void SetVersion(ICollection events, int startingVersion)
		{
			if (events == null || events.Count <= 0)
				return;

			foreach (var @event in events)
				this.SetEventVersion(@event, startingVersion++);
		}
		private void SetEventVersion(object @event, int version)
		{
			if (@event == null)
				return;

			var property = this.GetProperty(@event.GetType());
			if (null != property)
				property.SetValue(@event, version, null);
		}
		private PropertyInfo GetProperty(Type type)
		{
			PropertyInfo property;
			if (Cache.TryGetValue(type, out property))
				return property;

			lock (Cache)
			{
				if (Cache.TryGetValue(type, out property))
					return property;

				property = type.GetProperty(this.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				return Cache[type] = property;
			}
		}
	}
}