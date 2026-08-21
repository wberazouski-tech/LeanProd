export interface UserSummary { id: string; email: string; displayName: string; isActive: boolean; preferredLanguage: string; defaultDepartmentId: string | null; defaultStorageLocationId: string | null; roles: string[]; createdAtUtc: string; lastLoginAtUtc: string | null; }
export interface UserDetails extends UserSummary { permissions: string[]; updatedAtUtc: string | null; concurrencyStamp: string; }
export interface RoleDetails { name: string; permissions: string[]; }
export interface PagedUsers { items: UserSummary[]; page: number; pageSize: number; totalCount: number; }
