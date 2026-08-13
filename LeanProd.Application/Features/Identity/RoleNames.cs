namespace LeanProd.Application.Features.Identity;

public static class RoleNames
{
    public const string SystemAdministrator = nameof(SystemAdministrator);
    public const string MasterDataAdministrator = nameof(MasterDataAdministrator);
    public const string EquipmentOperator = nameof(EquipmentOperator);
    public const string QualityRegistrar = nameof(QualityRegistrar);
    public const string ShiftReportEditor = nameof(ShiftReportEditor);
    public const string Viewer = nameof(Viewer);

    public static readonly string[] All =
    [
        SystemAdministrator, MasterDataAdministrator, EquipmentOperator,
        QualityRegistrar, ShiftReportEditor, Viewer
    ];
}
