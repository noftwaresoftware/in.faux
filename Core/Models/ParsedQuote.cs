// Ignore Spelling: Noftware Faux

using System.Collections.Generic;
using System.Linq;

namespace Noftware.In.Faux.Core.Models
{
    /// <summary>
    /// A quote parsed from a source text file. 
    /// </summary>
    public class ParsedQuote
	{
		/// <summary>
		/// Source line.
		/// </summary>
		public string Line { get; set; }

		/// <summary>
		/// A list of parser errors. If empty, parse was a success.
		/// </summary>
		public IEnumerable<string> Errors { get; set; } = new List<string>();

		/// <summary>
		/// Name of the image file.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Associated keywords.
		/// </summary>
		public IEnumerable<string> KeyWords { get; set; } = new List<string>();

		/// <summary>
		/// The text.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Original image.
		/// </summary>
		public byte[] OriginalImage { get; set; }

		/// <summary>
		/// Original image's width (in pixels).
		/// </summary>
		public int OriginalImageWidth { get; set; }

		/// <summary>
		/// Original image's height (in pixels).
		/// </summary>
		public int OriginalImageHeight { get; set; }

		/// <summary>
		/// Resized image.
		/// </summary>
		public byte[] ResizedImage { get; set; }

		/// <summary>
		/// Resized image's width (in pixels).
		/// </summary>
		public int ResizedImageWidth { get; set; }

		/// <summary>
		/// Resized image's height (in pixels).
		/// </summary>
		public int ResizedImageHeight { get; set; }

		/// <summary>
		/// Thumbnail image.
		/// </summary>
		public byte[] ThumbnailImage { get; set; }

		/// <summary>
		/// Thumbnail image's width (in pixels).
		/// </summary>
		public int ThumbnailImageWidth { get; set; }

		/// <summary>
		/// Thumbnail image's height (in pixels).
		/// </summary>
		public int ThumbnailImageHeight { get; set; }

		/// <summary>
		/// Associated keywords, delimited by a comma.
		/// </summary>
		public string DelimitedKeyWords
		{
			get
			{
				string output = string.Empty;

				if (this.KeyWords?.Any() == true)
				{
					output = string.Join(' ', this.KeyWords);
				}

				return output;
			}
		}

		/// <summary>
		/// The searchable content consisting of the Text, Description, and KeyWords.
		/// </summary>
		public IEnumerable<string> SearchItems { get; set; } = new List<string>();

		/// <summary>
		/// Associated search items (Text, Description, and KeyWords), delimited by a space.
		/// </summary>
		public string DelimitedSearchItems
		{
			get
			{
				string output = string.Empty;

				if (this.SearchItems?.Any() == true)
				{
					output = string.Join(' ', this.SearchItems);
				}

				return output;
			}
		}

		/// <summary>
		/// Formatted output suitable for display.
		/// </summary>
		public override string ToString()
		{
			if (this.KeyWords?.Any() == true)
			{
				return $"{this.Text} | {this.Description} | {this.FileName} | Search index: {this.DelimitedSearchItems}";
			}
			else
			{
				return $"{this.Text} | {this.Description} | {this.FileName} | *No search index*";
			}
		}
	}
}
