using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Models
{
	/// <summary>
	/// A single Azure table entity filter to be used in the filter builder.
	/// </summary>
	public class TableEntityFilter
	{
		/// <summary>
		/// Boolean operator.
		/// </summary>
		public BooleanOperator BooleanOperator { get; set; }

		/// <summary>
		/// Name of field.
		/// </summary>
		public string Field { get; set; }

		/// <summary>
		/// Comparison operator.
		/// </summary>
		public ComparisonOperator ComparisonOperator { get; set; }

		/// <summary>
		/// Value. If string or DateTime, surround by single quotes.
		/// </summary>
		public string Value { get; set; }
	}
}
