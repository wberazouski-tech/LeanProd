namespace LeanProd.Domain.Common;

public static class DatabaseSchemaVersions
{
    public const string BusinessBaselineMigrationId = "20260922113355_BusinessInitialCreate";
    public const string InternalBaselineMigrationId = "20260922133158_InternalInitialCreate";

    public static IReadOnlySet<string> LegacyBusinessMigrationIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "20260812161630_InitialIdentity",
        "20260812215826_AddRefreshTokenRotation",
        "20260817142903_AddUserAdministration",
        "20260817153116_AddPreferredLanguage",
        "20260817160758_AddDepartmentsAndStorageLocations",
        "20260817184703_SetDepartmentCodeLength",
        "20260817190833_AddUserDefaultMasterData",
        "20260818111654_SetStorageLocationCodeLength",
        "20260818133403_AddUnitsOfMeasure",
        "20260819145011_AddEquipment",
        "20260820133234_MakeEquipmentStateIntervalsEditable",
        "20260820142343_AddOrganizationAndAddresses",
        "20260821160657_AddCatalogItems",
        "20260823204817_AddCatalogItemCostHistory",
        "20260823211506_SetCatalogItemCostScale",
        "20260824182019_AddCatalogItemClasses",
        "20260826213319_AddWorkforceAndBrigades",
        "20260828231256_AddCatalogTechnologies",
        "20260903130122_AddCatalogTechnologyStatus",
        "20260905194916_NormalizeTechnologyStagesAndTransitions",
        "20260906144323_MoveStageDepartmentToTechnologyStage",
        "20260915095358_AddWorkSchedulesAndProductionCalendars",
        "20260921191147_AddItemPropertiesAndBatches",
    };
}
