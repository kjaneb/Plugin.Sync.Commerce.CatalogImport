using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.Sync.Commerce.EntitiesMigration.Extensions
{
    /// <summary>
    /// Selection Extensions
    /// </summary>
    public static class SelectionExtensions
    {
        /// <summary>
        /// To Custom Selection
        /// </summary>
        /// <param name="input">input selection</param>
        /// <returns>Custom Selection</returns>
        public static IList<CustomSelection> ToCustomSelections(this IList<Selection> input)
        {
            return input.Select(element => element.ToCustomSelection()).ToList();
        }

        /// <summary>
        /// To Custom Selection
        /// </summary>
        /// <param name="input">input selection</param>
        /// <returns>Custom Selection</returns>
        public static CustomSelection ToCustomSelection(this Selection input)
        {
            return new CustomSelection()
            {
                DisplayName = input.DisplayName,
                IsDefault = input.IsDefault,
                Name = input.Name
            };
        }

        /// <summary>
        /// To Selections
        /// </summary>
        /// <param name="input">input custom selections</param>
        /// <returns>Selections</returns>
        public static List<Selection> ToSelections(this IList<CustomSelection> input)
        {
            return input.Select(element => element.ToSelection()).ToList();
        }

        /// <summary>
        /// To Selection
        /// </summary>
        /// <param name="input">input custom selection</param>
        /// <returns>Selection</returns>
        public static Selection ToSelection(this CustomSelection input)
        {
            return new Selection()
            {
                DisplayName = input.DisplayName,
                IsDefault = input.IsDefault,
                Name = input.Name
            };
        }

    }
}
