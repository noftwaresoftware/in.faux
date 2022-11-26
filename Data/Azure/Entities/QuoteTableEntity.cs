using Noftware.In.Faux.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Azure.Entities
{
	/// <summary>
	/// Azure Table entity for quote items.
	/// </summary>
	public class QuoteTableEntity : BaseTableEntity
    {
		/// <summary>
		/// The text.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Name of the original image file.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Original image's width (in pixels).
		/// </summary>
		public int OriginalImageWidth { get; set; }

		/// <summary>
		/// Original image's height (in pixels).
		/// </summary>
		public int OriginalImageHeight { get; set; }

		/// <summary>
		/// Resized image's width (in pixels).
		/// </summary>
		public int ResizedImageWidth { get; set; }

		/// <summary>
		/// Resized image's height (in pixels).
		/// </summary>
		public int ResizedImageHeight { get; set; }

		/// <summary>
		/// Thumbnail image's width (in pixels).
		/// </summary>
		public int ThumbnailImageWidth { get; set; }

		/// <summary>
		/// Thumbnail image's height (in pixels).
		/// </summary>
		public int ThumbnailImageHeight { get; set; }

		/// <summary>
		/// Associated keywords, separated by commas.
		/// </summary>
		public string KeyWords { get; set; }

		/// <summary>
		/// The searchable content consisting of the Text, Description, and KeyWords.
		/// </summary>
		public string SearchIndex { get; set; }

		// The PartitionKey property stores string values that identify the partition that an entity belongs to. Entities that have the same PartitionKey value are stored in the same partition.
		// The RowKey property stores string values that uniquely identify entities within each partition.
	}
}
