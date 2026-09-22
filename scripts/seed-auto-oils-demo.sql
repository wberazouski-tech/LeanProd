-- Adds local demo master data for automotive oil production to the current LeanProd database.
-- The script is idempotent by business codes/articles and does not create stock balances or production postings.
IF DB_NAME() <> N'LeanProd'
    THROW 51000, 'Auto oils demo seed is restricted to the local LeanProd database.', 1;

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

DECLARE @Now datetime2 = SYSUTCDATETIME();

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

DECLARE @RootDepartment uniqueidentifier;
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'AUTO')
    INSERT Departments (Id, Code, Name, Description, ParentDepartmentId, IsActive, CreatedAtUtc)
    VALUES (NEWID(), 'AUTO', 'Automotive Oils Demo', 'Demo organization branch for automotive lubricants.', NULL, 1, @Now);
SELECT @RootDepartment = Id FROM Departments WHERE Code = 'AUTO';

DECLARE @Departments TABLE (Code nchar(4), Name nvarchar(200), Description nvarchar(1000));
INSERT @Departments VALUES
('OILP', 'Oil Production', 'Blending and filling of automotive oils.'),
('OILQ', 'Oil Quality Laboratory', 'Incoming and in-process quality control for automotive oils.'),
('OILW', 'Oil Warehouse', 'Raw materials, packaging and finished goods storage for the demo line.'),
('OILM', 'Oil Maintenance', 'Maintenance of tanks, pumps and filling equipment.');

INSERT Departments (Id, Code, Name, Description, ParentDepartmentId, IsActive, CreatedAtUtc)
SELECT NEWID(), d.Code, d.Name, d.Description, @RootDepartment, 1, @Now
FROM @Departments d
WHERE NOT EXISTS (SELECT 1 FROM Departments x WHERE x.Code = d.Code);

DECLARE @ProductionDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'OILP');
DECLARE @QualityDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'OILQ');
DECLARE @WarehouseDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'OILW');
DECLARE @MaintenanceDepartment uniqueidentifier = (SELECT Id FROM Departments WHERE Code = 'OILM');
DECLARE @Premises uniqueidentifier = (SELECT Id FROM StorageLocationKinds WHERE Code = 'Premises');
DECLARE @Zone uniqueidentifier = (SELECT Id FROM StorageLocationKinds WHERE Code = 'Zone');

DECLARE @Locations TABLE (
    Code nchar(4), Name nvarchar(200), Description nvarchar(1000),
    DepartmentId uniqueidentifier, KindId uniqueidentifier, TypeCode nvarchar(50));
INSERT @Locations VALUES
('OIRM', 'Oil Raw Materials Warehouse', 'Base oils and additive packages for demo production.', @WarehouseDepartment, @Premises, 'Warehouse'),
('OIPK', 'Oil Packaging Warehouse', 'Bottles, caps, labels and cartons for automotive oils.', @WarehouseDepartment, @Premises, 'Warehouse'),
('OIW1', 'Oil Blending WIP Zone', 'Work-in-progress zone beside the blending tank.', @ProductionDepartment, @Zone, 'Production'),
('OIQH', 'Oil Quality Hold Area', 'Quarantine area for samples and non-released batches.', @QualityDepartment, @Zone, 'QualityHold'),
('OIFG', 'Oil Finished Goods Warehouse', 'Released automotive oil finished goods.', @WarehouseDepartment, @Premises, 'Warehouse'),
('OISP', 'Oil Spare Parts Store', 'Spare parts for pumps, filters and filling line.', @MaintenanceDepartment, @Premises, 'Warehouse'),
('OIRS', 'Oil Waste Collection', 'Rejected oil, filters and packaging scrap.', @ProductionDepartment, @Zone, 'Waste');

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

IF NOT EXISTS (SELECT 1 FROM EquipmentTypes WHERE Name = 'Automotive oil production equipment')
    INSERT EquipmentTypes (Id, Name, Description, IsActive, CreatedAtUtc)
    VALUES (NEWID(), 'Automotive oil production equipment', 'Demo equipment type for blending, filtration and filling.', 1, @Now);

DECLARE @OilEquipmentType uniqueidentifier = (SELECT Id FROM EquipmentTypes WHERE Name = 'Automotive oil production equipment');
DECLARE @Equipment TABLE (
    Name nvarchar(200), InventoryNumber nvarchar(50), DepartmentId uniqueidentifier,
    Manufacturer nvarchar(200), Model nvarchar(100), Description nvarchar(1000));
INSERT @Equipment VALUES
('BLT-3000 blending tank', 'OIL-BLT-3000', @ProductionDepartment, 'LeanProd Demo', 'BLT-3000', 'Stainless blending tank for 3000 litre batches.'),
('FIL-10 inline filter skid', 'OIL-FIL-10', @ProductionDepartment, 'LeanProd Demo', 'FIL-10', 'Inline filtration skid after blending.'),
('FLL-8 automatic filling line', 'OIL-FLL-8', @ProductionDepartment, 'LeanProd Demo', 'FLL-8', 'Automatic bottle filling, capping and labelling line.'),
('VIS-40 viscosity tester', 'OIL-VIS-40', @QualityDepartment, 'LeanProd Demo', 'VIS-40', 'Quality lab viscosity tester.');

INSERT Equipment (Id, Name, InventoryNumber, EquipmentTypeId, DepartmentId, ParentEquipmentId,
    SerialNumber, Manufacturer, Model, CommissionedOn, Description, IsActive, CreatedAtUtc)
SELECT NEWID(), e.Name, e.InventoryNumber, @OilEquipmentType, e.DepartmentId, NULL,
    NULL, e.Manufacturer, e.Model, '2026-01-15', e.Description, 1, @Now
FROM @Equipment e
WHERE NOT EXISTS (SELECT 1 FROM Equipment x WHERE x.InventoryNumber = e.InventoryNumber);

INSERT EquipmentStateEvents (Id, EquipmentId, State, StartedAtUtc, EndedAtUtc, Comment, CreatedAtUtc)
SELECT NEWID(), e.Id, 'Operational', @Now, NULL, 'Initial demo operating state.', @Now
FROM Equipment e
WHERE e.InventoryNumber IN ('OIL-BLT-3000', 'OIL-FIL-10', 'OIL-FLL-8', 'OIL-VIS-40')
  AND NOT EXISTS (SELECT 1 FROM EquipmentStateEvents x WHERE x.EquipmentId = e.Id);

DECLARE @Items TABLE (
    Type nvarchar(40), ArticleNumber nvarchar(100), WorkingName nvarchar(200),
    FullName nvarchar(500), UnitLetterCode nvarchar(10), Cost decimal(18,2), Description nvarchar(1000));

INSERT @Items VALUES
('Product','OIL-5W30-1L','Motor oil 5W-30 1 L','Synthetic motor oil SAE 5W-30, 1 litre bottle','H87',18.90,'Finished automotive oil for retail sale.'),
('Product','OIL-5W30-4L','Motor oil 5W-30 4 L','Synthetic motor oil SAE 5W-30, 4 litre canister','H87',68.00,'Finished automotive oil for retail sale.'),
('Product','OIL-10W40-4L','Motor oil 10W-40 4 L','Semi-synthetic motor oil SAE 10W-40, 4 litre canister','H87',54.00,'Finished automotive oil for retail sale.'),
('SemiFinishedProduct','OIL-BLEND-5W30','Bulk blend 5W-30','Filtered bulk blend for SAE 5W-30 before packaging','LTR',10.80,'Intermediate automotive oil batch.'),
('PrimaryMaterial','OIL-BASE-SN150','Base oil SN150','Mineral base oil SN150','LTR',4.10,'Primary base oil.'),
('PrimaryMaterial','OIL-BASE-SN500','Base oil SN500','Mineral base oil SN500','LTR',4.55,'Primary base oil.'),
('PrimaryMaterial','OIL-PAO-4','PAO synthetic base oil','Polyalphaolefin PAO 4 synthetic base oil','LTR',8.70,'Synthetic base oil component.'),
('PrimaryMaterial','OIL-ADD-DI','Detergent inhibitor additive','Detergent and inhibitor additive package','KGM',16.40,'Primary additive package.'),
('PrimaryMaterial','OIL-ADD-VII','Viscosity index improver','Viscosity index improver polymer','KGM',18.20,'Primary additive package.'),
('PrimaryMaterial','OIL-ADD-PPD','Pour point depressant','Low temperature pour point depressant','KGM',14.90,'Primary additive package.'),
('AuxiliaryMaterial','OIL-ANTIFOAM','Anti-foam additive','Silicone anti-foam additive for lubricant blending','KGM',22.00,'Auxiliary additive.'),
('AuxiliaryMaterial','OIL-QC-SAMPLE','Oil sample bottle','Clean sample bottle for quality laboratory retain samples','H87',0.35,'Auxiliary quality material.'),
('Packaging','OIL-BOTTLE-1L','1 L oil bottle','HDPE one litre bottle for motor oil','H87',0.42,'Primary packaging.'),
('Packaging','OIL-CAN-4L','4 L oil canister','HDPE four litre canister for motor oil','H87',1.35,'Primary packaging.'),
('Packaging','OIL-CAP-38','38 mm oil cap','Tamper evident cap for oil bottles and canisters','H87',0.08,'Packaging component.'),
('Packaging','OIL-LABEL-5W30','5W-30 product label','Self-adhesive product label for SAE 5W-30','H87',0.06,'Printed label.'),
('Packaging','OIL-CARTON-12','Shipping carton 12 x 1 L','Corrugated carton for twelve one litre oil bottles','H87',0.90,'Secondary packaging.'),
('ToolingAndTools','OIL-NOZZLE-1L','1 L oil filling nozzle','Filling nozzle set for one litre bottles','H87',95.00,'Production tooling.'),
('SparePart','OIL-PUMP-SEAL','Oil transfer pump seal kit','Seal kit for lubricant transfer pump','H87',120.00,'Maintenance spare part.'),
('PurchasedService','OIL-LAB-EXT','External oil analysis','External laboratory analysis of lubricant sample','HUR',85.00,'Purchased quality service.'),
('Waste','OIL-WASTE-OFFSPEC','Off-spec oil blend','Automotive oil blend outside specification','LTR',0.00,'Controlled production waste.'),
('Waste','OIL-WASTE-FILTER','Used oil filter media','Spent filter media from oil filtration','KGM',0.00,'Controlled maintenance waste.');

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

INSERT CatalogItemCostHistory (Id, CatalogItemId, Amount, EffectiveFromUtc, CreatedAtUtc)
SELECT NEWID(), ci.Id, i.Cost, @Now, @Now
FROM @Items i
JOIN CatalogItems ci ON ci.Type = i.Type AND ci.ArticleNumber = i.ArticleNumber
WHERE NOT EXISTS (SELECT 1 FROM CatalogItemCostHistory ch WHERE ch.CatalogItemId = ci.Id);

DECLARE @TechStageDefinitions TABLE (
    Code nvarchar(50), Name nvarchar(200), DepartmentId uniqueidentifier, Description nvarchar(1000));
INSERT @TechStageDefinitions VALUES
('OIL-WEIGH','Weighing and dosing', @ProductionDepartment, 'Dosing of base oils and additive packages.'),
('OIL-BLEND','Blending and heating', @ProductionDepartment, 'Controlled blending, circulation and heating.'),
('OIL-FILTER','Filtration', @ProductionDepartment, 'Inline filtration before quality release.'),
('OIL-QC','Quality control', @QualityDepartment, 'Laboratory viscosity and appearance checks.'),
('OIL-FILL','Filling and packing', @ProductionDepartment, 'Bottle filling, capping, labelling and carton packing.');

INSERT TechnologyStages (Id, Code, Name, Description, IsActive, CreatedAtUtc, DepartmentId)
SELECT NEWID(), s.Code, s.Name, s.Description, 1, @Now, s.DepartmentId
FROM @TechStageDefinitions s
WHERE NOT EXISTS (SELECT 1 FROM TechnologyStages x WHERE x.Code = s.Code);

DECLARE @Product5W301L uniqueidentifier = (SELECT Id FROM CatalogItems WHERE Type = 'Product' AND ArticleNumber = 'OIL-5W30-1L');

IF NOT EXISTS (SELECT 1 FROM CatalogTechnologies WHERE Code = 'TECH-OIL-5W30-1L')
    INSERT CatalogTechnologies (Id, Code, Name, CatalogItemId, CatalogItemClassId, VersionNo,
        ValidFrom, ValidTo, IsDefault, Description, IsActive, CreatedAtUtc, Status)
    VALUES (NEWID(), 'TECH-OIL-5W30-1L', 'Technology for motor oil 5W-30 1 L',
        @Product5W301L, NULL, 1, '2026-01-01', NULL, 1,
        'Demo routing and bill of materials for automotive motor oil blending and packing.', 1, @Now, 'Active');

DECLARE @Technology uniqueidentifier = (SELECT Id FROM CatalogTechnologies WHERE Code = 'TECH-OIL-5W30-1L');
DECLARE @BlendingTank uniqueidentifier = (SELECT Id FROM Equipment WHERE InventoryNumber = 'OIL-BLT-3000');
DECLARE @FilterSkid uniqueidentifier = (SELECT Id FROM Equipment WHERE InventoryNumber = 'OIL-FIL-10');
DECLARE @FillingLine uniqueidentifier = (SELECT Id FROM Equipment WHERE InventoryNumber = 'OIL-FLL-8');
DECLARE @ViscosityTester uniqueidentifier = (SELECT Id FROM Equipment WHERE InventoryNumber = 'OIL-VIS-40');

DECLARE @TechStages TABLE (
    StageNumber int, StageCode nvarchar(50), PlannedDurationMinutes decimal(18,3),
    EquipmentId uniqueidentifier NULL, Description nvarchar(1000));
INSERT @TechStages VALUES
(10, 'OIL-WEIGH', 45, NULL, 'Prepare and dose all materials for one 1000 litre batch.'),
(20, 'OIL-BLEND', 180, @BlendingTank, 'Blend and circulate the batch until homogeneous.'),
(30, 'OIL-FILTER', 60, @FilterSkid, 'Filter the blend before quality release.'),
(40, 'OIL-QC', 90, @ViscosityTester, 'Confirm viscosity, density and visual condition.'),
(50, 'OIL-FILL', 240, @FillingLine, 'Fill, cap, label and pack one litre bottles.');

INSERT CatalogTechnologyStages
    (Id, CatalogTechnologyId, StageNumber, PlannedDurationMinutes, EquipmentId,
     Description, CreatedAtUtc, TechnologyStageId)
SELECT NEWID(), @Technology, s.StageNumber, s.PlannedDurationMinutes, s.EquipmentId,
    s.Description, @Now, ts.Id
FROM @TechStages s
JOIN TechnologyStages ts ON ts.Code = s.StageCode
WHERE NOT EXISTS (
    SELECT 1 FROM CatalogTechnologyStages x
    WHERE x.CatalogTechnologyId = @Technology AND x.StageNumber = s.StageNumber);

DECLARE @Stage10 uniqueidentifier = (SELECT Id FROM CatalogTechnologyStages WHERE CatalogTechnologyId = @Technology AND StageNumber = 10);
DECLARE @Stage20 uniqueidentifier = (SELECT Id FROM CatalogTechnologyStages WHERE CatalogTechnologyId = @Technology AND StageNumber = 20);
DECLARE @Stage30 uniqueidentifier = (SELECT Id FROM CatalogTechnologyStages WHERE CatalogTechnologyId = @Technology AND StageNumber = 30);
DECLARE @Stage40 uniqueidentifier = (SELECT Id FROM CatalogTechnologyStages WHERE CatalogTechnologyId = @Technology AND StageNumber = 40);
DECLARE @Stage50 uniqueidentifier = (SELECT Id FROM CatalogTechnologyStages WHERE CatalogTechnologyId = @Technology AND StageNumber = 50);

DECLARE @Transitions TABLE (FromStageId uniqueidentifier, ToStageId uniqueidentifier);
INSERT @Transitions VALUES (@Stage10, @Stage20), (@Stage20, @Stage30), (@Stage30, @Stage40), (@Stage40, @Stage50);

INSERT CatalogTechnologyStageTransitions
    (Id, CatalogTechnologyId, FromCatalogTechnologyStageId, ToCatalogTechnologyStageId, CreatedAtUtc)
SELECT NEWID(), @Technology, t.FromStageId, t.ToStageId, @Now
FROM @Transitions t
WHERE NOT EXISTS (
    SELECT 1 FROM CatalogTechnologyStageTransitions x
    WHERE x.CatalogTechnologyId = @Technology
      AND x.FromCatalogTechnologyStageId = t.FromStageId
      AND x.ToCatalogTechnologyStageId = t.ToStageId);

DECLARE @Litre uniqueidentifier = (SELECT Id FROM UnitOfMeasures WHERE LetterCode = 'LTR');
DECLARE @Kilogram uniqueidentifier = (SELECT Id FROM UnitOfMeasures WHERE LetterCode = 'KGM');
DECLARE @Piece uniqueidentifier = (SELECT Id FROM UnitOfMeasures WHERE LetterCode = 'H87');
DECLARE @RawMaterials uniqueidentifier = (SELECT Id FROM StorageLocations WHERE Code = 'OIRM');
DECLARE @PackagingWarehouse uniqueidentifier = (SELECT Id FROM StorageLocations WHERE Code = 'OIPK');
DECLARE @Wip uniqueidentifier = (SELECT Id FROM StorageLocations WHERE Code = 'OIW1');
DECLARE @FinishedGoods uniqueidentifier = (SELECT Id FROM StorageLocations WHERE Code = 'OIFG');

DECLARE @Materials TABLE (
    StageId uniqueidentifier, ArticleNumber nvarchar(100), UnitId uniqueidentifier,
    Quantity decimal(18,3), ScrapPercent decimal(9,4), IsOptional bit,
    SourceStorageLocationId uniqueidentifier, Note nvarchar(1000));
INSERT @Materials VALUES
(@Stage20, 'OIL-PAO-4', @Litre, 620.000, 0.5000, 0, @RawMaterials, 'Synthetic base oil for the batch.'),
(@Stage20, 'OIL-BASE-SN150', @Litre, 250.000, 0.5000, 0, @RawMaterials, 'Mineral base oil fraction.'),
(@Stage20, 'OIL-ADD-DI', @Kilogram, 75.000, 0.2500, 0, @RawMaterials, 'Detergent inhibitor package.'),
(@Stage20, 'OIL-ADD-VII', @Kilogram, 42.000, 0.2500, 0, @RawMaterials, 'Viscosity index improver.'),
(@Stage20, 'OIL-ADD-PPD', @Kilogram, 8.000, 0.2500, 0, @RawMaterials, 'Low-temperature performance additive.'),
(@Stage20, 'OIL-ANTIFOAM', @Kilogram, 0.600, 0.0000, 1, @RawMaterials, 'Used only when foam appears during circulation.'),
(@Stage40, 'OIL-QC-SAMPLE', @Piece, 3.000, 0.0000, 0, @RawMaterials, 'Retain, lab and counter samples.'),
(@Stage50, 'OIL-BOTTLE-1L', @Piece, 1000.000, 1.0000, 0, @PackagingWarehouse, 'Primary bottles.'),
(@Stage50, 'OIL-CAP-38', @Piece, 1000.000, 1.0000, 0, @PackagingWarehouse, 'Caps for bottles.'),
(@Stage50, 'OIL-LABEL-5W30', @Piece, 1000.000, 1.0000, 0, @PackagingWarehouse, 'Front/back product labels.'),
(@Stage50, 'OIL-CARTON-12', @Piece, 84.000, 0.5000, 0, @PackagingWarehouse, 'Cartons for finished bottles.');

INSERT CatalogTechnologyMaterials
    (Id, TechnologyStageId, CatalogItemId, UnitOfMeasureId, Quantity,
     ConsumptionTrackingMode, DefaultSourceStorageLocationId, ScrapPercent,
     IsOptional, Note, CreatedAtUtc)
SELECT NEWID(), m.StageId, ci.Id, m.UnitId, m.Quantity,
    'ActualConsumptionTracked', m.SourceStorageLocationId, m.ScrapPercent,
    m.IsOptional, m.Note, @Now
FROM @Materials m
JOIN CatalogItems ci ON ci.ArticleNumber = m.ArticleNumber
WHERE NOT EXISTS (
    SELECT 1
    FROM CatalogTechnologyMaterials x
    WHERE x.TechnologyStageId = m.StageId AND x.CatalogItemId = ci.Id);

INSERT CatalogTechnologyMaterialSupplyRouteSteps
    (Id, TechnologyMaterialId, [LineNo], FromStorageLocationId, ToStorageLocationId,
     IsConsumptionPoint, MovementKind, LeadTimeMinutes, Note, CreatedAtUtc)
SELECT NEWID(), m.Id, 10, m.DefaultSourceStorageLocationId, NULL,
    1, 'IssueToProduction', 30.000, 'Issue material from demo source location to the consuming stage.', @Now
FROM CatalogTechnologyMaterials m
JOIN CatalogTechnologyStages s ON s.Id = m.TechnologyStageId
WHERE s.CatalogTechnologyId = @Technology
  AND m.DefaultSourceStorageLocationId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM CatalogTechnologyMaterialSupplyRouteSteps r
      WHERE r.TechnologyMaterialId = m.Id AND r.[LineNo] = 10);

DECLARE @Outputs TABLE (
    StageId uniqueidentifier, ArticleNumber nvarchar(100), UnitId uniqueidentifier,
    Quantity decimal(18,3), ReceiptStorageLocationId uniqueidentifier, IsPrimary bit, Note nvarchar(1000));
INSERT @Outputs VALUES
(@Stage30, 'OIL-BLEND-5W30', @Litre, 1000.000, @Wip, 0, 'Filtered bulk blend available for quality release.'),
(@Stage50, 'OIL-5W30-1L', @Piece, 1000.000, @FinishedGoods, 1, 'Primary finished output for the demo technology.');

INSERT CatalogTechnologyStageOutputs
    (Id, TechnologyStageId, CatalogItemId, UnitOfMeasureId, Quantity,
     ReceiptStorageLocationId, IsPrimary, Note, CreatedAtUtc)
SELECT NEWID(), o.StageId, ci.Id, o.UnitId, o.Quantity,
    o.ReceiptStorageLocationId, o.IsPrimary, o.Note, @Now
FROM @Outputs o
JOIN CatalogItems ci ON ci.ArticleNumber = o.ArticleNumber
WHERE NOT EXISTS (
    SELECT 1 FROM CatalogTechnologyStageOutputs x
    WHERE x.TechnologyStageId = o.StageId AND x.CatalogItemId = ci.Id);

DECLARE @Operations TABLE (
    StageId uniqueidentifier, Code nvarchar(50), Name nvarchar(200),
    DepartmentId uniqueidentifier, EquipmentId uniqueidentifier NULL,
    SetupMinutes decimal(18,3), RunMinutes decimal(18,3), LaborMinutes decimal(18,3),
    Workers decimal(18,3), Note nvarchar(1000));
INSERT @Operations VALUES
(@Stage10, 'OIL-OP-010', 'Material staging and dosing', @ProductionDepartment, NULL, 15, 30, 90, 2, 'Prepare batch ticket and dose materials.'),
(@Stage20, 'OIL-OP-020', 'Blend circulation', @ProductionDepartment, @BlendingTank, 30, 150, 150, 1, 'Blend under controlled temperature and agitation.'),
(@Stage30, 'OIL-OP-030', 'Inline filtration', @ProductionDepartment, @FilterSkid, 10, 50, 50, 1, 'Filter blend into WIP tank.'),
(@Stage40, 'OIL-OP-040', 'Laboratory quality check', @QualityDepartment, @ViscosityTester, 10, 80, 80, 1, 'Check viscosity, density and appearance.'),
(@Stage50, 'OIL-OP-050', 'Filling, capping and labelling', @ProductionDepartment, @FillingLine, 45, 195, 390, 2, 'Pack released blend into one litre bottles.');

INSERT CatalogTechnologyOperations
    (Id, TechnologyStageId, Code, Name, DepartmentId, EquipmentId,
     SetupMinutes, RunMinutes, LaborMinutes, Workers, Note, CreatedAtUtc)
SELECT NEWID(), o.StageId, o.Code, o.Name, o.DepartmentId, o.EquipmentId,
    o.SetupMinutes, o.RunMinutes, o.LaborMinutes, o.Workers, o.Note, @Now
FROM @Operations o
WHERE NOT EXISTS (
    SELECT 1 FROM CatalogTechnologyOperations x
    WHERE x.TechnologyStageId = o.StageId AND x.Code = o.Code);

COMMIT TRANSACTION;

SELECT 'Departments' AS Entity, COUNT(*) AS DemoRows FROM Departments WHERE Code LIKE 'OIL%' OR Code = 'AUTO';
SELECT 'StorageLocations' AS Entity, COUNT(*) AS DemoRows FROM StorageLocations WHERE Code LIKE 'OI%';
SELECT 'Equipment' AS Entity, COUNT(*) AS DemoRows FROM Equipment WHERE InventoryNumber LIKE 'OIL-%';
SELECT 'CatalogItems' AS Entity, COUNT(*) AS DemoRows FROM CatalogItems WHERE ArticleNumber LIKE 'OIL-%';
SELECT 'CatalogTechnologies' AS Entity, COUNT(*) AS DemoRows FROM CatalogTechnologies WHERE Code = 'TECH-OIL-5W30-1L';
SELECT 'TechnologyStages' AS Entity, COUNT(*) AS DemoRows FROM TechnologyStages WHERE Code LIKE 'OIL-%';

