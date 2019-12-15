using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.EntityViews;


namespace Plugin.Sync.Commerce.EntitiesMigration.Extensions
{
    /// <summary>
    /// Entity View Extension
    /// </summary>
    public static class EntityViewExtensions
    {
        /// <summary>
        /// To Custom Entity View
        /// </summary>
        /// <param name="input">Entity View</param>
        /// <returns>Custom Entity View</returns>
        public static CustomEntityView ToCustomEntityView(this EntityView input)
        {
            return new CustomEntityView()
            {
                DisplayName = input.DisplayName,
                DisplayRank = input.DisplayRank,
                EntityId = input.EntityId,
                EntityVersion = input.EntityVersion,
                ItemId = input.ItemId,
                Name = input.Name,
                Properties = input.Properties.ToCustomViewProperties()
            };
        }

        public static EntityView ToEntityView(this CustomEntityView input)
        {
            return new EntityView()
            {
                DisplayName = input.DisplayName,
                DisplayRank = input.DisplayRank,
                EntityId = input.EntityId,
                EntityVersion = input.EntityVersion,
                Name = input.Name,
                ItemId = input.ItemId,
                Properties = input.Properties.ToViewProperties()
            };
        }
    }
}
