using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.Sync.Commerce.EntitiesMigration.Extensions
{
    /// <summary>
    /// View Property Extensions
    /// </summary>
    public static class ViewPropertyExtensions
    {
        /// <summary>
        /// TO Custom View Properties
        /// </summary>
        /// <param name="input">View Properties</param>
        /// <returns>Custom View Properties</returns>
        public static IList<CustomViewProperty> ToCustomViewProperties(this IList<ViewProperty> input)
        {
            return input.Select(element => element.ToCustomViewProperty()).ToList();
        }

        /// <summary>
        /// To Custom View Property
        /// </summary>
        /// <param name="input">View Property</param>
        /// <returns>Custom View Proerty</returns>
        public static CustomViewProperty ToCustomViewProperty(this ViewProperty input)
        {
            AvailableSelectionsPolicy availableSelectionPolicy = input.GetPolicy<AvailableSelectionsPolicy>();
            return new CustomViewProperty()
            {
                DisplayName = input.DisplayName,
                Name = input.Name,
                OriginalType = input.OriginalType,
                RawValue = input.RawValue,
                Value = input.Value,
                AvailableSelectionPolicy = availableSelectionPolicy.List == null || !availableSelectionPolicy.List.Any() ? null : availableSelectionPolicy.ToCustomAvailableSelectionPolicy()
            };
        }

        /// <summary>
        /// TO View Properties
        /// </summary>
        /// <param name="input">Custom View Properties</param>
        /// <returns>View Properties</returns>
        public static List<ViewProperty> ToViewProperties(this IEnumerable<CustomViewProperty> input)
        {
            return input.Select(element => element.ToViewProperty()).ToList();
        }

        /// <summary>
        /// To View Property
        /// </summary>
        /// <param name="input">Custom View Property</param>
        /// <returns>View Proerty</returns>
        public static ViewProperty ToViewProperty(this CustomViewProperty input)
        {
            List<Policy> policies = new List<Policy>();
            CustomAvailableSelectionPolicy customAvailableSelectionPolicy = input.AvailableSelectionPolicy;
                  
            if (customAvailableSelectionPolicy != null)
            {
                policies.Add(customAvailableSelectionPolicy.ToAvailableSelectionPolicy());
            }
            return new ViewProperty()
            {
                DisplayName = input.DisplayName,
                Name = input.Name,
                OriginalType = input.OriginalType,
                RawValue = input.RawValue ?? string.Empty,
                Value = input.Value ?? string.Empty,
                Policies = policies
            };
    }
}
}
