# Import Plugin to Sync Enterprise Products and Categories Into Sitecore Commerce 9 Catalog

*by* Sergey Yatsenko, November 20, 2019



## Overview

When a new system is introduced at an enterprise company, at least some kind of custom development is usually needed to integrate that new system with existing internal systems and third-party providers. Sitecore Commerce 9 is no exception as it may need to be integrated with an existing catalog, inventory, order processing, payment gateways, etc. When it comes to product catalogs (Product and Category records), most companies already have their PIM[1](https://xcentium.com/blog/2019/11/21/import-plugin-to-sync-enterprise-products-and-categories-into-sitecore-commerce-9-catalog#dfref-footnote-1) of choice in place and they often prefer to keep managing catalogs in those systems, at least for a time being.

Sitecore Experience Commerce 9 (XC) provides a good set of tools and APIs to manage its catalog data out of the box. [XC Business Tools](https://doc.sitecore.com/users/92/sitecore-experience-commerce/en/xc-business-tools.html) allows us to manage all merchandising functions from a single user interface, where catalog content can be manually created and managed. Managing Catalog records manually in Business Tools is possible, but not be feasible, when XC is not the primary source and those records are maintained in external PIM. [XC's integration endpoints](https://doc.sitecore.com/developers/92/sitecore-experience-commerce/en/commerce-integration.html) expose a set of RESTful APIs which allow us to create and manage XC Catalog content programmatically. XC Catalog management APIs are meant to be used for integrations, but it may take quite a bit of development work to properly build and maintain such integrations.

After delivering a number of XC Catalog integrations for various clients, it became clear that even though each integration is unique, there are many similarities and use cases, which could be addressed with the same set of reusable code blocks on different projects. XCentium's Sync Plugin has been created to address some of the most common Catalog integration scenarios and to allow the ability to significantly cut down, or even eliminate, the custom development needed.

The Sync Plugin enables synchronization of Product and Category records from external systems and third-party providers into Sitecore Commerce Catalog. It adds a set of pipelines and APIs which take in external data in JSON format[2](https://xcentium.com/blog/2019/11/21/import-plugin-to-sync-enterprise-products-and-categories-into-sitecore-commerce-9-catalog#dfref-footnote-2), map that data to the XC Catalog schema using its mapping configuration and then creates or updates records in the XC Catalog.

 

## How The Sync Plugin Works

Below high-level diagram explains how XCentium's Sync Plugin for Sitecore Experience Commerce performs import process.

![Sync Plugin Flow Diagram](https://cdn.xcentium.com/-/media/images/blog-images/import-plugin-to-sync-enterprise-products-and-categories-into-sitecore-commerce-catalog/sync-plugin-flow-diagram.ashx?h=401&w=1001&la=en&hash=95306533DB2B8C68BED2454AD0F11A00E456FC67&vs=1&d=20191121T221049Z)

The Sync Plugin adds custom APIs to enable the import of products, product variants and categories into the XC Catalog. Those APIs are called ImportSellableItem(), ImportSellableItemVariant() and ImportCategory(). Each API will call its own pipeline, which will run a set of pipeline blocks, shown in blue in the about diagram. Some of those pipeline blocks are shared by the above pipelines and some are not because they need to be specific for each given entity. For example, "Map JSON data to Commerce Entity" and "Update Composer Template Fields" are shared by the Import Category, Product and Product Variant pipelines, but "Create Entity" and "Update OOTB Entity fields" need to be different to address differences in Category, Product and Variant entities.

### RESTFUL APIS AND RELATED IMPORT PIPELINES ADDED BY SYNC PLUGIN

- **ImportCategory() API and ImportCategoryPipeline pipeline**: Map input data using configuration in "CategoryMappingPolicy", create new or update existing Category and update its Composer Template fields (if applicable)
- I**mportSellableItemCategory() API and ImportSellableItemPipeline pipeline**: Map input data using configuration in "CategoryMappingPolicy", create new or update existing Category and update its Composer Template fields (if applicable)
- **ImportSellableItem() API and ImportSellableItemVariantPipeline pipeline**: Map input data using configuration in "CategoryMappingPolicy", create new or update existing Category and update its Composer Template fields (if applicable)

### PIPELINE BLOCKS ADDED BY SYNC PLUGIN

- **ExtractCatalogEntityFieldsFromJsonDataBlock**: Parse input JSON and using Category, SellableItem or SellableItemVariant's mapping policy extract data and create a data model for a given entity. Data Model is added to the pipeline execution context, so it can be utilized by other pipeline blocks. This pipeline block is shared by all three import pipelines, described above, it will use mapping policy, specified by the caller method in pipeline argument.
- **CreateOrUpdateCategoryBlock**: If Commerce Category with given ID doesn't exist in XC Catalog then create new, otherwise update an existing one. Update OOTB Category entity fields, such as Display Name and Description with values provided in input JSON.
- **CreateOrUpdateSellableItemBlock**: If Commerce SellableItem with given ID doesn't exist in XC Catalog then create new, otherwise update an existing one. Update OOTB SellableItem entity fields, such as Display Name, Description, Brand, Manufacturer, TypeOfGoods, with values provided in input JSON.
- **CreateOrUpdateSellableItemVariantBlock**: If parent SellableItem doesn't exist, it will throw an error. If Commerce SellableItemVariant with given ID doesn't exist in XC Catalog then create a new one, otherwise, update an existing one. Update OOTB SellableItem entity fields, such as Display Name and Description with values provided in input JSON.
- **UpdateComposerFieldsBlock**: Iterate through [Entity Composer Views](https://community.sitecore.net/technical_blogs/b/technical-marketing/posts/experience-commerce-entity-composer) on given Commerce Entity (if present) and update their fields with values coming from input JSON data (if present)
- **UpdateCustomComponentsBlock**: Sync Plugin was designed to cover most Catalog integration needs and it can be easily extended with additional custom pipeline blocks, which can be added into the appropriate pipeline(s) as described [here](https://doc.sitecore.com/developers/92/sitecore-experience-commerce/en/registering-a-custom-commerce-pipeline-or-block.html). This block provides boilerplate code, which can be used as a foundation for creating such custom pipeline blocks.

### HOW FIELD MAPPING CONFIGURATION WORKS

Sync Plugin is using three separate mapping policies, one for each kind of entity it supports. Those mapping policies are essentially collections of [JSON paths](https://restfulapi.net/json-jsonpath/) to where entity fields can be found in input JSON. Similarly to any other Policy in XC, mapping Policies are configured in their JSON configs, which can be found under wwwroot/data/environments. When policy configuration is updated, then [bootstrap operation](https://doc.sitecore.com/developers/92/sitecore-experience-commerce/en/bootstrap-the-commerce-engine.html) needs to be performed. The below details are important but assume good familiarity with JSON structure and JSON path. This might need some explaining, which goes beyond the scope of this post...

There are two ways to configure mappings in the policy file:

- Add name-value pair where the left side is a mapped field name and the right side is a JSON path to where given field value can be found in input JSON
- Add one or more "root paths" where all immediate child elements of each given (JSON) path will be added into mapping configuration

Mapped fields are separated into three different kinds:

- OOTB entity fields, which are added at the root of mapping policy JSON
- Entity Composer fields, which are under ComposerFieldPaths and/or ComposerFieldRootPath
- Any kind of additional fields will go underCustomFieldPaths and/or CustomFieldRootPaths

Mapping Policy configuration is defined in the following files

- Category: CategoryMappingPolicy, configured in PlugIn.CatalogImport.CategoryMappingPolicySet-1.0.0.json
- SellableItem: SellableItemMappingPolicy, configured in PlugIn.CatalogImport.SellableItemMappingPolicySet-1.0.0.json
- SellableItemVariant: SellableItemVariantMappingVariantPolicy, configured in PlugIn.CatalogImport.SellableItemVariantMappingPolicySet-1.0.0.json

Mapping Policy entities and their configurations share the same structure.
