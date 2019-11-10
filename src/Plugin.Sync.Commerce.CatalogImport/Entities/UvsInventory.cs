using System;

namespace Plugin.Sync.Commerce.CatalogImport.Entities
{
    [Serializable]
    public class UvsInventory
    {
        #region Core Product Info
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
        public string VehicleId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string BrandName { get; set; }
        public string Manufacturer { get; set; }
        public string TypeOfGood { get; set; }
        public string ListPrice { get; set; }
        public string SamClass { get; set; }
        #endregion

        #region BASICINFO
        public string BreakType { get; set; }
        public string Color { get; set; }
        public string Gvw { get; set; }
        public string ManufacturerCode { get; set; }
        public string ManufacturerName { get; set; }
        public string Model { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductStatus { get; set; }
        public string SpecialEquipmentCategoryCode { get; set; }
        public string SpecialEquipmentCategoryName { get; set; }
        public string VinNumber { get; set; }
        public string Year { get; set; }
        #endregion

        #region ADDITIONALFEATURES


        public string SeatType { get; set; }
        public string CurbSideDoor { get; set; }
        public string RoadSideDoor { get; set; }
        public string Etrac { get; set; }
        public string LogisticPosts { get; set; }
        public string CurtainSideDoor { get; set; }
        public string AirConditioner { get; set; }
        public string AluminiumWheels { get; set; }
        public string Apu { get; set; }
        public string BellyBox { get; set; }
        public string DoubleSideDoor { get; set; }
        public string Headboard { get; set; }
        public string PowerLocks { get; set; }
        public string PowerMirrors { get; set; }
        public string PowerWindows { get; set; }
        public string RoadReadyPackage { get; set; }
        public string SideKitTarp { get; set; }

        #endregion

        #region BODY

        public string BodyHeight { get; set; }
        public string BodyLength { get; set; }
        public string BodyManufacturer { get; set; }
        public string BodyModel { get; set; }
        public string BodyType { get; set; }
        public string BodyWidth { get; set; }
        public string DropDeck { get; set; }
        public string FloorType { get; set; }
        public string InsulatedBody { get; set; }
        public string PlywoodLining { get; set; }
        public string RearDoorType { get; set; }
        public string TranslucentRoof { get; set; }
        public string WheelPlan { get; set; }
        public string WoodSlats { get; set; }

        #endregion


        #region DATES

        public string AcceptedDate { get; set; }
        public string SoldDate { get; set; }
        public string HighestPriorityChangeDate { get; set; }

        #endregion

    }
}