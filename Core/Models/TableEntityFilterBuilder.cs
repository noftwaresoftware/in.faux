using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Models
{
	/// <summary>
	/// Azure table entity filter builder.
	/// </summary>
	public class TableEntityFilterBuilder
	{
		/// <summary>
		/// Based on one or more filter expressions, build a filter for an Azure table query.
		/// </summary>
		/// <param name="filters">One of more filters.</param>
		/// <returns><see cref="string"/> filter or null, if the filters parameter is empty.</returns>
		public string Build(IEnumerable<TableEntityFilter> filters)
		{
			if (filters?.Any() == false)
			{
				return null;
			}

			var filterText = new StringBuilder();
			foreach (var filter in filters)
			{
				// ~~~~~
				// Boolean operator
				switch (filter.BooleanOperator)
				{
					case BooleanOperator.And:
						{
							filterText.Append("and ");
							break;
						}
					case BooleanOperator.Or:
						{
							filterText.Append("or ");
							break;
						}
					default:
						{
							filterText.Append(" ");
							break;
						}
				} // switch (filter.BooleanOperator)

				// ~~~~~
				// Field name
				filterText.Append($"{filter.Field} ");

				// ~~~~~
				// Comparison operator
				switch (filter.ComparisonOperator)
				{
					case ComparisonOperator.EqualTo:
						{
							filterText.Append("eq ");
							break;
						}
					case ComparisonOperator.GreaterThan:
						{
							filterText.Append("gt ");
							break;
						}
					case ComparisonOperator.GreaterThanOrEqualTo:
						{
							filterText.Append("ge ");
							break;
						}
					case ComparisonOperator.LessThan:
						{
							filterText.Append("lt ");
							break;
						}
					case ComparisonOperator.LessThanOrEqualTo:
						{
							filterText.Append("le ");
							break;
						}
					case ComparisonOperator.NotEqualTo:
						{
							filterText.Append("ne ");
							break;
						}
				} // switch (filter.ComparisonOperator)

				// ~~~~~
				// Value
				filterText.Append($"{filter.Value} ");

			} // foreach (var filter in filters)

			return filterText.ToString().Trim();
		}
	}

	/// <summary>
	/// Boolean operators.
	/// </summary>
	public enum BooleanOperator
	{
		/// <summary>
		/// No boolean operator. This should only be used for the first expression.
		/// </summary>
		None = 0,

		/// <summary>
		/// And operator.
		/// </summary>
		And = 1,

		/// <summary>
		/// Or operator.
		/// </summary>
		Or = 2
	}

	/// <summary>
	/// Comparison operators.
	/// </summary>
	public enum ComparisonOperator
	{
		/// <summary>
		/// Equal to.
		/// </summary>
		EqualTo = 0,

		/// <summary>
		/// Not equal to.
		/// </summary>
		NotEqualTo = 1,

		/// <summary>
		/// Greater than.
		/// </summary>
		GreaterThan = 5,

		/// <summary>
		/// Greater than or Equal to.
		/// </summary>
		GreaterThanOrEqualTo = 6,

		/// <summary>
		/// Greater than.
		/// </summary>
		LessThan = 10,

		/// <summary>
		/// Greater than or Equal to.
		/// </summary>
		LessThanOrEqualTo = 11
	}

}
