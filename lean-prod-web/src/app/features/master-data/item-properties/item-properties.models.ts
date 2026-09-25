export type ItemPropertyType = 'Number' | 'Text' | 'Choice' | 'Boolean' | 'Range';
export interface PropertyOption { id: string | null; label: string; }
export interface PropertyDefinition {
  id: string; catalogItemClassId: string; name: string; type: ItemPropertyType;
  decimalPlaces: number | null; maxLength: number | null; minimum: string | null; maximum: string | null;
  isBatchProperty: boolean; isActive: boolean; options: PropertyOption[]; rowVersion: string; isInUse?: boolean;
}
export interface PropertyValue {
  propertyId: string; number: string | null; upper: string | null; text: string | null;
  boolean: boolean | null; optionId: string | null;
}
export interface PropertyValues { rowVersion: string; definitions: PropertyDefinition[]; values: PropertyValue[]; batch?: ItemBatch | null; isEditable?: boolean; }
export interface ItemBatch {
  id: string; catalogItemId: string; number: string; receiptDate: string; receiptReference: string; rowVersion: string;
}
