using System;
using System.Collections.Generic;
using System.Text;

namespace Noftware.In.Faux.Shared.Models
{
    /// <summary>
    /// Data item.
    /// </summary>
    public class Quote
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// The text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Keywords or tags for search lookups.
        /// </summary>
        public IEnumerable<string> Keywords { get; set; }

        /// <summary>
        /// File name of the image.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Associated display image.
        /// </summary>
        public string Base64Image { get; set; }

        /// <summary>
        /// Associated thumbnail image.
        /// </summary>
        public string Base64ThumbnailImage { get; set; }

        /// <summary>
        /// In the event that the service is temporarily unavailable, get a default one.
        /// </summary>
        /// <returns><see cref="Quote"/></returns>
        public static Quote GetDefault()
        {
            return new Quote()
            {
                Text = "Take it offline.",
                Description = "When a discussion goes too deep into detail or the topic is digressed, this is said to get everybody back on topic.",
                Keywords = new string[] { "detail", "reduce", "deep", "digress", "topic" },
                Key = System.Guid.Empty
            };
        }
    }
}
