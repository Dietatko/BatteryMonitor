using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public abstract class ViewLogicBase : INotifyPropertyChanged
	{
		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the chnaged property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handlers = this.PropertyChanged;
			if (handlers != null)
				handlers(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetPropertyValue<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			T oldValue = currentValue;

			// Check equality
			if ((typeof(T).IsValueType && Object.Equals(oldValue, newValue))
				|| (!typeof(T).IsValueType && Object.ReferenceEquals(oldValue, newValue)))
			{
				return false;
			}
			
			currentValue = newValue;
			this.OnPropertyChanged(propertyName);

			return true;
		}

		protected void PassThroughPropertyChangeNotification<TObject, TSource, TTarget>(TObject sourceObject, Expression<Func<TObject, TSource>> sourcePropertyExpr, Expression<Func<TTarget>> thisPropertyExpr)
			where TObject : INotifyPropertyChanged
		{
			Contract.Requires(sourcePropertyExpr, "sourcePropertyExpr").IsNotNull();
			Contract.Requires(thisPropertyExpr, "thisPropertyExpr").IsNotNull();

			string sourcePropertyName = ((MemberExpression)sourcePropertyExpr.Body).Member.Name;
			string thisPropertyName = ((MemberExpression)thisPropertyExpr.Body).Member.Name;
			this.PassThroughPropertyChangeNotification(sourceObject, sourcePropertyName, thisPropertyName);
		}

		protected void PassThroughPropertyChangeNotification<TObject>(TObject sourceObject, string sourcePropertyName, string thisPropertyName)
			where TObject : INotifyPropertyChanged
		{
			Contract.Requires(sourceObject, "sourceObject").IsNotNull();
			Contract.Requires(sourcePropertyName, "sourcePropertyName").IsNotNull().IsNotEmpty();
			Contract.Requires(thisPropertyName, "thisPropertyName").IsNotNull().IsNotEmpty();

			sourceObject.PropertyChanged += (sender, args) =>
				{
					if (args.PropertyName == sourcePropertyName)
					{
						if (Application.Current == null || Application.Current.Dispatcher == null)
							this.OnPropertyChanged(thisPropertyName);
						else
							Application.Current.Dispatcher.InvokeAsync(() => this.OnPropertyChanged(thisPropertyName));
					}
				};
		}
	}
}
