-- This fixture may only modify an explicitly isolated demo database.
IF DB_NAME() <> N'LeanProd_Demo' AND DB_NAME() NOT LIKE N'LeanProd[_]Demo[_]%'
    THROW 51000, 'Demo seed is restricted to LeanProd_Demo or LeanProd_Demo_<name>.', 1;

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

-- Units copied from the international unit template used by LeanProd.
IF NOT EXISTS (SELECT 1 FROM UnitOfMeasures WHERE Code = '166')
    INSERT UnitOfMeasures (Id, Code, Name, Symbol, LetterCode, QuantityType, DecimalPlaces, IsActive)
    VALUES (NEWID(), '166', 'kilogram', 'kg', 'KGM', 'Mass', 3, 1);
IF NOT EXISTS (SELECT 1 FROM UnitOfMeasures WHERE Code = '112')
    INSERT UnitOfMeasures (Id, Code, Name, Symbol, LetterCode, QuantityType, DecimalPlaces, IsActive)
    VALUES (NEWID(), '112', 'litre', 'l', 'LTR', 'Volume', 3, 1);
IF NOT EXISTS (SELECT 1 FROM UnitOfMeasures WHERE Code = '356')
    INSERT UnitOfMeasures (Id, Code, Name, Symbol, LetterCode, QuantityType, DecimalPlaces, IsActive)
    VALUES (NEWID(), '356', 'hour', 'h', 'HUR', 'Time', 2, 1);
IF NOT EXISTS (SELECT 1 FROM UnitOfMeasures WHERE Code = '796')
    INSERT UnitOfMeasures (Id, Code, Name, Symbol, LetterCode, QuantityType, DecimalPlaces, IsActive)
    VALUES (NEWID(), '796', 'piece', 'pc', 'H87', 'Count', 0, 1);

-- Additional common units, also copied from the same international template.
DECLARE @AdditionalUnits TABLE (
    Code nvarchar(4), Name nvarchar(200), Symbol nvarchar(30), LetterCode nvarchar(10),
    QuantityType nvarchar(50), DecimalPlaces tinyint);
INSERT @AdditionalUnits VALUES
('003', 'millimetre', 'mm', 'MMT', 'Length', 3),
('004', 'centimetre', 'cm', 'CMT', 'Length', 3),
('055', 'square metre', 'm2', 'MTK', 'Area', 3),
('113', 'cubic metre', 'm3', 'MTQ', 'Volume', 3),
('163', 'gram', 'g', 'GRM', 'Mass', 3),
('168', 'tonne (metric ton)', 't', 'TNE', 'Mass', 3),
('354', 'second [unit of time]', 's', 'SEC', 'Time', 2),
('355', 'minute [unit of time]', 'min', 'MIN', 'Time', 2),
('359', 'day', 'd', 'DAY', 'Time', 2),
('360', 'week', NULL, 'WEE', 'Time', 2),
('641', 'dozen', 'Doz' + CHAR(10) + '12', 'DZN', 'Count', 0),
('704', 'set', NULL, 'SET', 'Count', 0),
('715', 'pair', 'pr' + CHAR(10) + '2', 'PR', 'Count', 0);

INSERT UnitOfMeasures (Id, Code, Name, Symbol, LetterCode, QuantityType, DecimalPlaces, IsActive)
SELECT NEWID(), u.Code, u.Name, u.Symbol, u.LetterCode, u.QuantityType, u.DecimalPlaces, 1
FROM @AdditionalUnits u
WHERE NOT EXISTS (SELECT 1 FROM UnitOfMeasures x WHERE x.Code = u.Code);

-- Keep multiline international symbols exactly as stored in the template.
UPDATE UnitOfMeasures SET Symbol = 'l' + CHAR(10) + 'L' + CHAR(10) + 'dm3' WHERE Code = '112';
UPDATE UnitOfMeasures SET Symbol = 'pc' + CHAR(10) + '1' WHERE Code = '796';

DECLARE @Now datetime2 = SYSUTCDATETIME();

-- English demo organization structure. Existing rows remain intact because user defaults may reference them.
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'HEAD')
    INSERT Departments (Id, Code, Name, Description, ParentDepartmentId, IsActive, CreatedAtUtc)
    VALUES (NEWID(), 'HEAD', 'Head Office', 'Company administration and management.', NULL, 1, @Now);

DECLARE @Head uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'HEAD');
DECLARE @Departments TABLE (Code nvarchar(4), Name nvarchar(200), Description nvarchar(1000));
INSERT @Departments VALUES
('PROD', 'Production', 'Main manufacturing department.'),
('WHSE', 'Warehouse Operations', 'Material and finished goods warehousing.'),
('QUAL', 'Quality Assurance', 'Incoming, in-process and final quality control.'),
('MAIN', 'Maintenance', 'Equipment maintenance and repair.'),
('SALE', 'Sales', 'Customer and sales operations.'),
('PROC', 'Procurement', 'Purchasing and supplier management.'),
('LOGI', 'Logistics', 'Internal and external logistics.');

INSERT Departments (Id, Code, Name, Description, ParentDepartmentId, IsActive, CreatedAtUtc)
SELECT NEWID(), d.Code, d.Name, d.Description, @Head, 1, @Now
FROM @Departments d
WHERE NOT EXISTS (SELECT 1 FROM Departments x WHERE x.Code = d.Code);

DECLARE @WarehouseDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'WHSE');
DECLARE @ProductionDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'PROD');
DECLARE @QualityDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'QUAL');
DECLARE @MaintenanceDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'MAIN');
DECLARE @Premises uniqueidentifier = (SELECT Id FROM StorageLocationKinds WHERE Code = 'Premises');
DECLARE @Zone uniqueidentifier = (SELECT Id FROM StorageLocationKinds WHERE Code = 'Zone');

DECLARE @Locations TABLE (Code nvarchar(4), Name nvarchar(200), Description nvarchar(1000), DepartmentId uniqueidentifier, KindId uniqueidentifier, TypeCode nvarchar(50));
INSERT @Locations VALUES
('RAWM', 'Raw Materials Warehouse', 'Main storage for production raw materials.', @WarehouseDepartment, @Premises, 'Warehouse'),
('PACK', 'Packaging Warehouse', 'Packaging and labels storage.', @WarehouseDepartment, @Premises, 'Warehouse'),
('FGDS', 'Finished Goods Warehouse', 'Released finished products awaiting shipment.', @WarehouseDepartment, @Premises, 'Warehouse'),
('SPAR', 'Spare Parts Store', 'Spare parts for machines and equipment.', @MaintenanceDepartment, @Premises, 'Warehouse'),
('TOOL', 'Tool Crib', 'Production tooling and hand tools.', @MaintenanceDepartment, @Premises, 'Warehouse'),
('WIP1', 'Production WIP Zone', 'Work in progress near production lines.', @ProductionDepartment, @Zone, 'Production'),
('QHLD', 'Quality Hold Area', 'Quarantined and non-released materials.', @QualityDepartment, @Zone, 'QualityHold'),
('WAST', 'Waste Collection Area', 'Sorted production waste.', @ProductionDepartment, @Zone, 'Waste'),
('RECV', 'Receiving Area', 'Inbound goods receiving and inspection.', @WarehouseDepartment, @Zone, 'Warehouse'),
('SHIP', 'Shipping Area', 'Outbound order consolidation and loading.', @WarehouseDepartment, @Zone, 'Warehouse');

INSERT StorageLocations (Id, Code, Name, Description, DepartmentId, KindId, ParentStorageLocationId, IsActive, CreatedAtUtc)
SELECT NEWID(), l.Code, l.Name, l.Description, l.DepartmentId, l.KindId, NULL, 1, @Now
FROM @Locations l
WHERE NOT EXISTS (SELECT 1 FROM StorageLocations x WHERE x.Code = l.Code);

INSERT StorageLocationTypeAssignments (StorageLocationId, StorageLocationTypeId)
SELECT s.Id, t.Id
FROM @Locations l
JOIN StorageLocations s ON s.Code = l.Code
JOIN StorageLocationTypes t ON t.Code = l.TypeCode
WHERE NOT EXISTS (
    SELECT 1 FROM StorageLocationTypeAssignments a
    WHERE a.StorageLocationId = s.Id AND a.StorageLocationTypeId = t.Id);

DECLARE @Items TABLE (
    Type nvarchar(40), ArticleNumber nvarchar(100), WorkingName nvarchar(200),
    FullName nvarchar(500), UnitLetterCode nvarchar(10), Description nvarchar(1000));

INSERT @Items VALUES
-- Products
('Product','PRD-001','Industrial cleaner 1 L','Industrial multipurpose cleaner, 1 litre bottle','H87','Standard finished product.'),
('Product','PRD-002','Industrial cleaner 5 L','Industrial multipurpose cleaner, 5 litre canister','H87','Standard finished product.'),
('Product','PRD-003','Degreaser 750 ml','Heavy-duty degreasing spray, 750 ml','H87','Standard finished product.'),
('Product','PRD-004','Hand soap 500 ml','Liquid hand soap, 500 ml dispenser','H87','Standard finished product.'),
('Product','PRD-005','Floor cleaner 5 L','Concentrated floor cleaner, 5 litre canister','H87','Standard finished product.'),
('Product','PRD-006','Glass cleaner 750 ml','Ready-to-use glass cleaner, 750 ml','H87','Standard finished product.'),
('Product','PRD-007','Sanitizer 1 L','Surface sanitizer, 1 litre bottle','H87','Standard finished product.'),
('Product','PRD-008','Laundry detergent 3 L','Liquid laundry detergent, 3 litre bottle','H87','Standard finished product.'),
('Product','PRD-009','Dishwashing liquid 1 L','Manual dishwashing liquid, 1 litre bottle','H87','Standard finished product.'),
('Product','PRD-010','Machine cleaner 10 L','Industrial machine cleaner, 10 litre canister','H87','Standard finished product.'),
-- Customer work
('Work','WRK-001','Contract liquid blending','Contract blending of customer liquid formulation','HUR','Customer production work.'),
('Work','WRK-002','Contract powder blending','Contract blending of customer powder formulation','HUR','Customer production work.'),
('Work','WRK-003','Bottle filling','Automated filling of customer product into bottles','HUR','Customer production work.'),
('Work','WRK-004','Canister filling','Automated filling of customer product into canisters','HUR','Customer production work.'),
('Work','WRK-005','Product labelling','Application of customer labels to finished packs','HUR','Customer production work.'),
('Work','WRK-006','Shrink wrapping','Shrink wrapping of customer product batches','HUR','Customer production work.'),
('Work','WRK-007','Repacking','Repacking finished goods into customer packaging','HUR','Customer production work.'),
('Work','WRK-008','Batch coding','Application of batch and expiry coding','HUR','Customer production work.'),
('Work','WRK-009','Promotional kitting','Assembly of promotional product kits','HUR','Customer production work.'),
('Work','WRK-010','Private-label production','Production of private-label finished goods','HUR','Customer production work.'),
-- Primary materials
('PrimaryMaterial','MAT-001','Sodium hydroxide','Sodium hydroxide solution for industrial formulations','KGM','Primary production material.'),
('PrimaryMaterial','MAT-002','Citric acid','Food and technical grade citric acid','KGM','Primary production material.'),
('PrimaryMaterial','MAT-003','Ethanol','Industrial ethanol used as formulation base','LTR','Primary production material.'),
('PrimaryMaterial','MAT-004','Hydrogen peroxide','Hydrogen peroxide solution','LTR','Primary production material.'),
('PrimaryMaterial','MAT-005','Surfactant A','Primary anionic surfactant','KGM','Primary production material.'),
('PrimaryMaterial','MAT-006','Surfactant B','Primary non-ionic surfactant','KGM','Primary production material.'),
('PrimaryMaterial','MAT-007','Sodium carbonate','Technical grade sodium carbonate','KGM','Primary production material.'),
('PrimaryMaterial','MAT-008','Glycerin','Refined glycerin for liquid formulations','KGM','Primary production material.'),
('PrimaryMaterial','MAT-009','Demineralized water','Demineralized process water','LTR','Primary production material.'),
('PrimaryMaterial','MAT-010','Sodium hypochlorite','Sodium hypochlorite solution','LTR','Primary production material.'),
-- Auxiliary materials
('AuxiliaryMaterial','AUX-001','Blue dye','Concentrated blue formulation dye','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-002','Green dye','Concentrated green formulation dye','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-003','Lemon fragrance','Lemon fragrance composition','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-004','Pine fragrance','Pine fragrance composition','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-005','Preservative','Liquid formulation preservative','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-006','Foam stabilizer','Foam stabilizing additive','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-007','Viscosity modifier','Liquid viscosity modifier','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-008','pH indicator','Process pH indicator solution','LTR','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-009','Anti-foaming agent','Process anti-foaming additive','KGM','Auxiliary production material.'),
('AuxiliaryMaterial','AUX-010','Corrosion inhibitor','Equipment-safe corrosion inhibitor','KGM','Auxiliary production material.'),
-- Semi-finished products
('SemiFinishedProduct','SFP-001','Cleaner base','Uncoloured multipurpose cleaner base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-002','Degreaser base','Concentrated degreaser base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-003','Soap base','Unscented liquid soap base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-004','Floor cleaner base','Concentrated floor cleaner base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-005','Glass cleaner base','Uncoloured glass cleaner base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-006','Sanitizer base','Unperfumed sanitizer base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-007','Detergent concentrate','Laundry detergent concentrate','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-008','Dishwashing base','Dishwashing liquid base','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-009','Fragrance premix','Prepared fragrance premix','KGM','Intermediate production batch.'),
('SemiFinishedProduct','SFP-010','Colour premix','Prepared colour premix','KGM','Intermediate production batch.'),
-- Packaging
('Packaging','PKG-001','Bottle 500 ml','Transparent HDPE bottle, 500 ml','H87','Primary packaging material.'),
('Packaging','PKG-002','Bottle 1 L','Transparent HDPE bottle, 1 litre','H87','Primary packaging material.'),
('Packaging','PKG-003','Bottle 3 L','HDPE bottle with handle, 3 litres','H87','Primary packaging material.'),
('Packaging','PKG-004','Canister 5 L','HDPE canister, 5 litres','H87','Primary packaging material.'),
('Packaging','PKG-005','Canister 10 L','HDPE canister, 10 litres','H87','Primary packaging material.'),
('Packaging','PKG-006','Trigger sprayer','Standard trigger sprayer closure','H87','Primary packaging component.'),
('Packaging','PKG-007','Screw cap 28 mm','Tamper-evident screw cap, 28 mm','H87','Primary packaging component.'),
('Packaging','PKG-008','Product label','Self-adhesive product label','H87','Printed packaging material.'),
('Packaging','PKG-009','Shipping carton','Corrugated shipping carton','H87','Secondary packaging material.'),
('Packaging','PKG-010','Stretch film','Machine stretch film','KGM','Tertiary packaging material.'),
-- Tooling and tools
('ToolingAndTools','TOL-001','Filling nozzle 10 mm','Stainless steel filling nozzle, 10 mm','H87','Production tooling.'),
('ToolingAndTools','TOL-002','Filling nozzle 20 mm','Stainless steel filling nozzle, 20 mm','H87','Production tooling.'),
('ToolingAndTools','TOL-003','Cap chuck 28 mm','Capping machine chuck, 28 mm','H87','Production tooling.'),
('ToolingAndTools','TOL-004','Label guide','Adjustable labelling machine guide','H87','Production tooling.'),
('ToolingAndTools','TOL-005','Torque wrench','Calibrated production torque wrench','H87','Maintenance tool.'),
('ToolingAndTools','TOL-006','Digital caliper','Digital caliper, 0–150 mm','H87','Inspection tool.'),
('ToolingAndTools','TOL-007','Sampling cup','Stainless steel process sampling cup','H87','Quality tool.'),
('ToolingAndTools','TOL-008','Mixing paddle','Removable stainless steel mixing paddle','H87','Production tooling.'),
('ToolingAndTools','TOL-009','Hose crimping die','Interchangeable hose crimping die','H87','Maintenance tooling.'),
('ToolingAndTools','TOL-010','Alignment fixture','Bottle conveyor alignment fixture','H87','Production tooling.'),
-- Spare parts
('SparePart','SPR-001','Conveyor belt','Food-grade modular conveyor belt','H87','Machine spare part.'),
('SparePart','SPR-002','Electric motor 1.5 kW','Three-phase electric motor, 1.5 kW','H87','Machine spare part.'),
('SparePart','SPR-003','Gearbox assembly','Inline helical gearbox assembly','H87','Machine spare part.'),
('SparePart','SPR-004','Pump seal kit','Mechanical seal kit for transfer pump','H87','Machine spare part.'),
('SparePart','SPR-005','Photoelectric sensor','Diffuse photoelectric sensor','H87','Machine spare part.'),
('SparePart','SPR-006','Pneumatic cylinder','Double-acting pneumatic cylinder','H87','Machine spare part.'),
('SparePart','SPR-007','Solenoid valve','Pneumatic solenoid valve, 24 VDC','H87','Machine spare part.'),
('SparePart','SPR-008','Ball bearing','Sealed deep-groove ball bearing','H87','Machine spare part.'),
('SparePart','SPR-009','Heating element','Shrink tunnel heating element','H87','Machine spare part.'),
('SparePart','SPR-010','Emergency stop button','Red emergency stop push button','H87','Electrical spare part.'),
-- Purchased services
('PurchasedService','SRV-001','Equipment calibration','External calibration of measuring equipment','HUR','Purchased technical service.'),
('PurchasedService','SRV-002','Laboratory analysis','External laboratory product analysis','HUR','Purchased quality service.'),
('PurchasedService','SRV-003','Electrical maintenance','External electrical maintenance work','HUR','Purchased maintenance service.'),
('PurchasedService','SRV-004','Mechanical maintenance','External mechanical maintenance work','HUR','Purchased maintenance service.'),
('PurchasedService','SRV-005','Pest control','Scheduled industrial pest control','HUR','Purchased facility service.'),
('PurchasedService','SRV-006','Waste collection','Licensed collection of industrial waste','HUR','Purchased environmental service.'),
('PurchasedService','SRV-007','Transport service','External road freight transport','HUR','Purchased logistics service.'),
('PurchasedService','SRV-008','Cleaning service','Industrial premises cleaning','HUR','Purchased facility service.'),
('PurchasedService','SRV-009','Software support','External production software support','HUR','Purchased IT service.'),
('PurchasedService','SRV-010','Safety inspection','External occupational safety inspection','HUR','Purchased compliance service.'),
-- Waste
('Waste','WST-001','Plastic bottle scrap','Rejected and damaged HDPE bottles','KGM','Sorted production waste.'),
('Waste','WST-002','Plastic cap scrap','Rejected plastic caps and closures','KGM','Sorted production waste.'),
('Waste','WST-003','Cardboard waste','Clean corrugated cardboard waste','KGM','Sorted production waste.'),
('Waste','WST-004','Stretch film waste','Used polyethylene stretch film','KGM','Sorted production waste.'),
('Waste','WST-005','Off-spec liquid product','Liquid product outside specification','KGM','Production waste requiring controlled disposal.'),
('Waste','WST-006','Chemical residue','Concentrated process chemical residue','KGM','Hazardous production waste.'),
('Waste','WST-007','Used filter media','Spent process filter media','KGM','Maintenance waste.'),
('Waste','WST-008','Contaminated absorbent','Used absorbent contaminated by chemicals','KGM','Hazardous maintenance waste.'),
('Waste','WST-009','Metal scrap','Mixed clean ferrous metal scrap','KGM','Recyclable maintenance waste.'),
('Waste','WST-010','Wastewater concentrate','Concentrated process wastewater','LTR','Liquid production waste.');

INSERT CatalogItems
    (Id, WorkingName, FullName, ArticleNumber, Type, BaseUnitOfMeasureId,
     CatalogItemClassId, Description, IsActive, CreatedAtUtc)
SELECT NEWID(), i.WorkingName, i.FullName, i.ArticleNumber, i.Type, u.Id,
       c.Id, i.Description, 1, @Now
FROM @Items i
JOIN UnitOfMeasures u ON u.LetterCode = i.UnitLetterCode
JOIN CatalogItemClasses c ON c.Type = i.Type AND c.Code = 'GENERAL' AND c.IsGroup = 0 AND c.IsActive = 1
WHERE NOT EXISTS (
    SELECT 1 FROM CatalogItems x
    WHERE x.Type = i.Type AND x.ArticleNumber = i.ArticleNumber);

COMMIT TRANSACTION;

SELECT Type, COUNT(*) AS ItemCount
FROM CatalogItems
GROUP BY Type
ORDER BY Type;
