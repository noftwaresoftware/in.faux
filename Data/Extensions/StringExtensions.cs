﻿// Ignore Spelling: Noftware Faux

using System.Text.RegularExpressions;

namespace Noftware.In.Faux.Data.Extensions
{
    /// <summary>
    /// String extensions/helpers.
    /// </summary>
    public static class StringExtensions
	{
		/// <summary>
		/// Replace all text within the string while ignoring the case sensitivity.
		/// </summary>
		/// <param name="source">source string.</param>
		/// <param name="oldValue">Old value to be replaced.</param>
		/// <param name="newValue">New value to replace the old value.</param>
		/// <returns><see cref="string"/></returns>
		public static string ReplaceIgnoreCase(this string source, string oldValue, string newValue)
		{
			// https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
			return Regex.Replace(source, Regex.Escape(oldValue), newValue.Replace("$", "$$"), RegexOptions.IgnoreCase);
		}
    }
}
