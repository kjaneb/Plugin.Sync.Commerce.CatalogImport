namespace Plugin.Sync.Commerce.EntitiesMigration.Models
{
    /// <summary>
    /// Composer Property
    /// </summary>
    public class CustomViewProperty
    {
        /// <summary>
        /// c'tor
        /// </summary>
        public CustomViewProperty()
        {

        }

        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Orinial Type
        /// </summary>
        public string OriginalType { get; set; }

        /// <summary>
        /// Raw Value
        /// </summary>
        public object RawValue { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Available Selection Policy
        /// </summary>
        public CustomAvailableSelectionPolicy AvailableSelectionPolicy { get; set; }
    }
}
