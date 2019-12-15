using System.Collections.Generic;

namespace Plugin.Sync.Commerce.EntitiesMigration.Models
{
    /// <summary>
    /// Composer View
    /// </summary>
    public class CustomEntityView
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public CustomEntityView()
        {
            Properties = new List<CustomViewProperty>();
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Display Rank
        /// </summary>
        public int DisplayRank { get; set; }

        /// <summary>
        /// Item ID
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Entity Id
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Entity Version
        /// </summary>
        public int EntityVersion { get; set; }

        /// <summary>
        /// Properties
        /// </summary>
        public IEnumerable<CustomViewProperty> Properties { get; set; }
    }
}
