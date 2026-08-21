export interface CurrentUser {
  id: string;
  email: string;
  displayName: string;
  preferredLanguage: string;
  defaultDepartmentId: string | null;
  defaultStorageLocationId: string | null;
  roles: string[];
  permissions: string[];
  accessToken: string;
  accessTokenExpiresAtUtc: string;
}
