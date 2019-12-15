using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.EntitiesMigration.Extensions
{
    /// <summary>
    /// Policy Extensions
    /// </summary>
    public static class PolicyExtensions
    {
        /// <summary>
        /// To Custom Available Selection Policy
        /// </summary>
        /// <param name="input">AvailableSelectionsPolicy</param>
        /// <returns>CustomAvailableSelectionPolicy</returns>
        public static CustomAvailableSelectionPolicy ToCustomAvailableSelectionPolicy(this AvailableSelectionsPolicy input)
        {
            return new CustomAvailableSelectionPolicy()
            {
                AllowMultiSelect = input.AllowMultiSelect,
                PolicyId = input.PolicyId,
                Selection = input.List.ToCustomSelections()
            };
        }

        /// <summary>
        /// To Custom Available Selection Policy
        /// </summary>
        /// <param name="input">AvailableSelectionsPolicy</param>
        /// <returns>CustomAvailableSelectionPolicy</returns>
        public static AvailableSelectionsPolicy ToAvailableSelectionPolicy(this CustomAvailableSelectionPolicy input)
        {
            return input == null
                ? null
                : new AvailableSelectionsPolicy()
                {
                    AllowMultiSelect = input.AllowMultiSelect,
                    PolicyId = input.PolicyId,
                    List = input.Selection.ToSelections()
                };
        }
    }
}
