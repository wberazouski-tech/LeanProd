namespace LeanProd.Application.Features.Identity;

public static class RolePermissionMatrix
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> Matrix =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            [RoleNames.SystemAdministrator] = [.. Permissions.All],
            [RoleNames.MasterDataAdministrator] =
            [
                Permissions.MasterDataView, Permissions.MasterDataManage,
                Permissions.ReportsView
            ],
            [RoleNames.EquipmentOperator] =
            [
                Permissions.MasterDataView, Permissions.ShiftReportsView,
                Permissions.ShiftReportsCreate, Permissions.ShiftReportsEditOwn,
                Permissions.ShiftReportsSubmit, Permissions.ReportsView
            ],
            [RoleNames.ShiftReportEditor] =
            [
                Permissions.MasterDataView, Permissions.ShiftReportsView,
                Permissions.ShiftReportsCreate, Permissions.ShiftReportsEditOwn,
                Permissions.ShiftReportsEditAny, Permissions.ShiftReportsSubmit,
                Permissions.ReportsView
            ],
            [RoleNames.QualityRegistrar] =
            [
                Permissions.MasterDataView, Permissions.QualityView,
                Permissions.QualityManage, Permissions.ReportsView
            ],
            [RoleNames.Viewer] =
            [
                Permissions.MasterDataView, Permissions.ShiftReportsView,
                Permissions.QualityView, Permissions.ReportsView
            ]
        };

    public static IReadOnlyCollection<string> GetPermissions(IEnumerable<string> roles) => roles
        .Where(Matrix.ContainsKey)
        .SelectMany(role => Matrix[role])
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();

    public static bool HasPermission(IEnumerable<string> roles, string permission) =>
        roles.Any(role => Matrix.TryGetValue(role, out var permissions) && permissions.Contains(permission));
}
