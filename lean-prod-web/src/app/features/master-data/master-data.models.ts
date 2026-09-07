export interface Page<T> { items: T[]; page: number; pageSize: number; totalCount: number; }
export interface OptionItem { id: string; code: string; name: string; }
export interface StorageOption extends OptionItem { departmentId: string; }
export interface CatalogItem { id: string; code: string; }

export interface UnitCatalogOption { code: string; name: string; symbol: string | null; letterCode: string; isAdded: boolean; }
export interface UnitSummary { id: string; code: string; displayName: string; name: string; symbol: string | null; letterCode: string; quantityType: string; decimalPlaces: number; isActive: boolean; }
export interface UnitTranslation { languageCode: string; name: string; }
export interface UnitDetails extends UnitSummary { translations: UnitTranslation[]; rowVersion: string; }
export interface UnitConversion { id: string; fromUnitId: string; fromUnitName: string; fromLetterCode: string; toUnitId: string; toUnitName: string; toLetterCode: string; multiplier: number; offset: number; rowVersion: string; }
export interface DepartmentSummary { id: string; code: string; name: string; parentDepartmentId: string | null; parentName: string | null; isActive: boolean; }
export interface DepartmentDetails { id: string; code: string; name: string; description: string | null; parentDepartmentId: string | null; isActive: boolean; rowVersion: string; }
export interface StorageSummary { id: string; code: string; name: string; parentStorageLocationId: string | null; departmentName: string; kindCode: string; typeCodes: string[]; isActive: boolean; }
export interface StorageDetails { id: string; code: string; name: string; description: string | null; departmentId: string; kindId: string; parentStorageLocationId: string | null; typeIds: string[]; isActive: boolean; rowVersion: string; }
export interface EquipmentSummary { id: string; name: string; inventoryNumber: string | null; typeName: string | null; departmentName: string; parentName: string | null; parentInventoryNumber: string | null; currentState: string | null; isActive: boolean; }
export interface EquipmentDetails { id: string; name: string; inventoryNumber: string | null; equipmentTypeId: string | null; departmentId: string; parentEquipmentId: string | null; serialNumber: string | null; manufacturer: string | null; model: string | null; commissionedOn: string | null; description: string | null; currentState: string | null; isActive: boolean; rowVersion: string; }
export interface EquipmentOption { id: string; name: string; inventoryNumber: string | null; departmentId: string; }
export interface EquipmentType { id: string; name: string; description: string | null; isActive: boolean; rowVersion: string; }
export interface EquipmentStateEvent { id: string; state: string; startedAtUtc: string; endedAtUtc: string | null; comment: string | null; rowVersion: string; }
export interface AddressDetails { id: string; addressType: string; countryCode: string; locality: string | null; postalCode: string | null; addressLine: string; gln: string | null; isPrimary: boolean; isActive: boolean; rowVersion: string; }
export interface OrganizationDetails { id: number; legalName: string; tradingName: string | null; legalForm: string | null; countryCode: string; taxNumber: string | null; statisticalNumber: string | null; companyRegistrationNumber: string | null; defaultCurrencyCode: string; timeZoneId: string; defaultLanguageCode: string; email: string | null; phone: string | null; website: string | null; logoFileId: string | null; printFooter: string | null; rowVersion: string; }
export type CatalogItemType = 'Product' | 'Work' | 'PrimaryMaterial' | 'AuxiliaryMaterial' | 'SemiFinishedProduct' | 'Packaging' | 'ToolingAndTools' | 'SparePart' | 'PurchasedService' | 'Waste';
export interface CatalogItemClassSummary { id: string; type: CatalogItemType; code: string; name: string; isGroup: boolean; parentId: string | null; isActive: boolean; }
export interface CatalogItemClassDetails extends CatalogItemClassSummary { rowVersion: string; }
export interface CatalogItemClassOption { id: string; type: CatalogItemType; code: string; name: string; isGroup: boolean; parentId: string | null; }
export interface CatalogItemSummary { id: string; workingName: string; fullName: string | null; articleNumber: string | null; type: CatalogItemType; baseUnitOfMeasureId: string; baseUnitName: string; baseUnitSymbol: string | null; catalogItemClassId: string; catalogItemClassCode: string; catalogItemClassName: string; cost: number; isActive: boolean; }
export interface CatalogItemDetails extends CatalogItemSummary { description: string | null; rowVersion: string; }

export type TechnologyMaterialConsumptionTrackingMode = 'NormOnly' | 'ActualConsumptionTracked';
export type TechnologyMaterialMovementKind = 'Transfer' | 'IssueToProduction' | 'InternalMove';
export type CatalogTechnologyStatus = 'InDevelopment' | 'Active' | 'NotUsed';
export interface TechnologyStageTemplate { id: string; code: string; name: string; description: string | null; isActive: boolean; rowVersion: string; }
export interface CatalogTechnologySummary { id: string; code: string; name: string; catalogItemId: string | null; catalogItemName: string | null; catalogItemClassId: string | null; catalogItemClassCode: string | null; catalogItemClassName: string | null; versionNo: number; validFrom: string | null; validTo: string | null; isDefault: boolean; status: CatalogTechnologyStatus; isActive: boolean; }
export interface TechnologyStage { id: string; code: string; name: string; description: string | null; isActive: boolean; departmentId: string | null; departmentName: string | null; }
export interface CatalogTechnologyDetails extends CatalogTechnologySummary { description: string | null; stages: CatalogTechnologyStage[]; stageTransitions: CatalogTechnologyStageTransition[]; rowVersion: string; }
export interface CatalogTechnologyStage { id: string; technologyStageId: string; technologyStageCode: string; technologyStageName: string; stageNumber: number; plannedDurationMinutes: number | null; technologyStageDepartmentId: string | null; technologyStageDepartmentName: string | null; equipmentId: string | null; equipmentName: string | null; description: string | null; materials: CatalogTechnologyMaterial[]; outputs: CatalogTechnologyStageOutput[]; operations: CatalogTechnologyOperation[]; }
export interface CatalogTechnologyStageTransition { id: string; fromCatalogTechnologyStageId: string; toCatalogTechnologyStageId: string; }
export interface CatalogTechnologyMaterial { id: string; catalogItemId: string; catalogItemName: string; unitOfMeasureId: string; unitOfMeasureName: string; quantity: number; consumptionTrackingMode: TechnologyMaterialConsumptionTrackingMode; defaultSourceStorageLocationId: string | null; defaultSourceStorageLocationName: string | null; scrapPercent: number; isOptional: boolean; note: string | null; routeSteps: CatalogTechnologyMaterialSupplyRouteStep[]; }
export interface CatalogTechnologyStageOutput { id: string; catalogItemId: string; catalogItemName: string; unitOfMeasureId: string; unitOfMeasureName: string; quantity: number; receiptStorageLocationId: string; receiptStorageLocationName: string; isPrimary: boolean; note: string | null; }
export interface CatalogTechnologyOperation { id: string; code: string; name: string; departmentId: string | null; departmentName: string | null; equipmentId: string | null; equipmentName: string | null; setupMinutes: number; runMinutes: number; laborMinutes: number; workers: number; note: string | null; }
export interface CatalogTechnologyMaterialSupplyRouteStep { id: string; lineNo: number; fromStorageLocationId: string; fromStorageLocationName: string; toStorageLocationId: string | null; toStorageLocationName: string | null; isConsumptionPoint: boolean; movementKind: TechnologyMaterialMovementKind; leadTimeMinutes: number; note: string | null; }
