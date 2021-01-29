using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client.ViewModels
{
    /// <summary>
    /// A quote for the UI.
    /// </summary>
    public class ViewQuote
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
        /// Search results page: Show the busy indicator?
        /// </summary>
        public bool SearchShowBusyIndicator { get; set; }

        /// <summary>
        /// Search results page: Show the resized image?
        /// </summary>
        public bool SearchShowResizedImage { get; set; }

        /// <summary>
        /// Search results page: The class open-icon class to show if the panel is open or closed.
        /// </summary>
        public string SearchPanelVisibilityClass { get; set; }
    }
}
