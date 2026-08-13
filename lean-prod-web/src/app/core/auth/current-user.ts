export interface CurrentUser {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
  permissions: string[];
  accessToken: string;
  accessTokenExpiresAtUtc: string;
}
