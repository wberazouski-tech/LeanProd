export const Permissions = {
  usersManage: 'Users.Manage',
  masterDataView: 'MasterData.View',
  masterDataManage: 'MasterData.Manage',
  shiftReportsView: 'ShiftReports.View',
  shiftReportsCreate: 'ShiftReports.Create',
  shiftReportsEditOwn: 'ShiftReports.EditOwn',
  shiftReportsEditAny: 'ShiftReports.EditAny',
  shiftReportsSubmit: 'ShiftReports.Submit',
  qualityView: 'Quality.View',
  qualityManage: 'Quality.Manage',
  reportsView: 'Reports.View',
  periodsClose: 'Periods.Close',
  auditView: 'Audit.View'
} as const;

export type Permission = typeof Permissions[keyof typeof Permissions];
