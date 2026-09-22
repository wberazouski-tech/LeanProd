import type { ItemPropertiesComponent } from './features/master-data/item-properties/item-properties.component';
import type { EquipmentComponent } from './features/master-data/equipment/equipment.component';
import type { CatalogItemsComponent } from './features/master-data/catalog-items/catalog-items.component';
import type { TechnologiesComponent } from './features/master-data/technologies/technologies.component';
import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { permissionGuard } from './core/auth/permission.guard';
import { Permissions } from './core/auth/permissions';
import { roleGuard } from './core/auth/role.guard';
import { ShellComponent } from './shell/shell.component';

const catalogItems = () => import('./features/master-data/catalog-items/catalog-items.component').then(m => m.CatalogItemsComponent);

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/identity/login/login.component').then(m => m.LoginComponent) },
  { path: 'setup', loadComponent: () => import('./features/setup/database-setup.component').then(m => m.DatabaseSetupComponent) },
  { path: 'register', loadComponent: () => import('./features/identity/register/register.component').then(m => m.RegisterComponent) },
  { path: 'forbidden', loadComponent: () => import('./features/identity/forbidden/forbidden.component').then(m => m.ForbiddenComponent) },
  { path: '', component: ShellComponent, canActivate: [authGuard], children: [
    { path: '', loadComponent: () => import('./home/home.component').then(m => m.HomeComponent) },
    { path: 'sample-form', loadComponent: () => import('./features/sample-form/sample-form.component').then(m => m.SampleFormComponent) },
    { path: 'master-data/departments', loadComponent: () => import('./features/master-data/departments/departments.component').then(m => m.DepartmentsComponent), canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/storage-locations', loadComponent: () => import('./features/master-data/storage-locations/storage-locations.component').then(m => m.StorageLocationsComponent), canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/units-of-measure', loadComponent: () => import('./features/master-data/units-of-measure/units-of-measure.component').then(m => m.UnitsOfMeasureComponent), canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/equipment', canDeactivate: [(component: EquipmentComponent) => component.requestLeave()], loadComponent: () => import('./features/master-data/equipment/equipment.component').then(m => m.EquipmentComponent), canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/technologies', canDeactivate: [(component: TechnologiesComponent) => component.requestLeave()], loadComponent: () => import('./features/master-data/technologies/technologies.component').then(m => m.TechnologiesComponent), canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/classes/:classId/properties', redirectTo: 'administration/item-properties/:classId', pathMatch: 'full' },
    { path: 'administration/item-properties', loadComponent: () => import('./features/master-data/item-properties/item-property-administration.component').then(m => m.ItemPropertyAdministrationComponent), canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'administration/item-properties/:classId', loadComponent: () => import('./features/master-data/item-properties/item-properties.component').then(m => m.ItemPropertiesComponent), canDeactivate: [(component: ItemPropertiesComponent) => component.requestLeave()], canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/items/:itemId/properties', loadComponent: () => import('./features/master-data/item-properties/item-properties.component').then(m => m.ItemPropertiesComponent), canDeactivate: [(component: ItemPropertiesComponent) => component.requestLeave()], canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/products', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'Product' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/works', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'Work' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/primary-materials', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'PrimaryMaterial' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/auxiliary-materials', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'AuxiliaryMaterial' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/semi-finished-products', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'SemiFinishedProduct' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/packaging', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'Packaging' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/tooling-and-tools', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'ToolingAndTools' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/spare-parts', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'SparePart' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/purchased-services', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'PurchasedService' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/waste', loadComponent: catalogItems, canDeactivate: [(component: CatalogItemsComponent) => component.requestLeave()], data: { type: 'Waste' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'administration/work-schedules', loadComponent: () => import('./features/scheduling/scheduling.component').then(m => m.SchedulingComponent), canActivate: [permissionGuard(Permissions.workSchedulesView)] },
    { path: 'administration/organization', loadComponent: () => import('./features/organization/organization.component').then(m => m.OrganizationComponent), canActivate: [permissionGuard(Permissions.organizationView)] },
    { path: 'administration/employees', loadComponent: () => import('./features/workforce/employees.component').then(m => m.EmployeesComponent), canActivate: [permissionGuard(Permissions.workforceView)] },
    { path: 'administration/brigades', loadComponent: () => import('./features/workforce/brigades.component').then(m => m.BrigadesComponent), canActivate: [permissionGuard(Permissions.workforceView)] },
    { path: 'administration/database-settings', loadComponent: () => import('./features/setup/database-setup.component').then(m => m.DatabaseSetupComponent), canActivate: [roleGuard('SystemAdministrator')] },
    { path: 'administration', loadComponent: () => import('./features/identity/administration/administration.component').then(m => m.AdministrationComponent), canActivate: [permissionGuard(Permissions.usersManage)] }
  ] },
  { path: '**', redirectTo: '' }
];

