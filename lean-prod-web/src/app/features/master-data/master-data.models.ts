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
