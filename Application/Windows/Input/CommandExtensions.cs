using System;
using System.Windows.Input;

namespace CarinaStudio.Windows.Input
{
	/// <summary>
	/// Extensions for <see cref="ICommand"/>.
	/// </summary>
	public static class CommandExtensions
	{
		/// <summary>
		/// Check whether command can be executed and execute.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="parameter">Command parameter.</param>
		/// <returns>True if command can be executed and has been executed.</returns>
		public static bool TryExecute(this ICommand command, object? parameter = null)
		{
			if (!command.CanExecute(parameter))
				return false;
			command.Execute(parameter);
			return true;
		}
	}
}
