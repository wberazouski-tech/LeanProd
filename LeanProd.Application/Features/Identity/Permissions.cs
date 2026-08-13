namespace LeanProd.Application.Features.Identity;

public static class Permissions
{
    public const string UsersManage = "Users.Manage";
    public const string MasterDataView = "MasterData.View";
    public const string MasterDataManage = "MasterData.Manage";
    public const string ShiftReportsView = "ShiftReports.View";
    public const string ShiftReportsCreate = "ShiftReports.Create";
    public const string ShiftReportsEditOwn = "ShiftReports.EditOwn";
    public const string ShiftReportsEditAny = "ShiftReports.EditAny";
    public const string ShiftReportsSubmit = "ShiftReports.Submit";
    public const string QualityView = "Quality.View";
    public const string QualityManage = "Quality.Manage";
    public const string ReportsView = "Reports.View";
    public const string PeriodsClose = "Periods.Close";
    public const string AuditView = "Audit.View";

    public static readonly HashSet<string> All =
    [
        UsersManage, MasterDataView, MasterDataManage,
        ShiftReportsView, ShiftReportsCreate, ShiftReportsEditOwn,
        ShiftReportsEditAny, ShiftReportsSubmit, QualityView, QualityManage,
        ReportsView, PeriodsClose, AuditView
    ];
}
