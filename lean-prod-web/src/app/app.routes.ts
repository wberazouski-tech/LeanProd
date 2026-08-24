import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { permissionGuard } from './core/auth/permission.guard';
import { Permissions } from './core/auth/permissions';
import { AdministrationComponent } from './features/identity/administration/administration.component';
import { ForbiddenComponent } from './features/identity/forbidden/forbidden.component';
import { LoginComponent } from './features/identity/login/login.component';
import { RegisterComponent } from './features/identity/register/register.component';
import { SampleFormComponent } from './features/sample-form/sample-form.component';
import { HomeComponent } from './home/home.component';
import { ShellComponent } from './shell/shell.component';
import { DepartmentsComponent } from './features/master-data/departments/departments.component';
import { StorageLocationsComponent } from './features/master-data/storage-locations/storage-locations.component';
import { UnitsOfMeasureComponent } from './features/master-data/units-of-measure/units-of-measure.component';
import { EquipmentComponent } from './features/master-data/equipment/equipment.component';
import { OrganizationComponent } from './features/organization/organization.component';
import { CatalogItemsComponent } from './features/master-data/catalog-items/catalog-items.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forbidden', component: ForbiddenComponent },
  { path: '', component: ShellComponent, canActivate: [authGuard], children: [
    { path: '', component: HomeComponent },
    { path: 'sample-form', component: SampleFormComponent },
    { path: 'master-data/departments', component: DepartmentsComponent, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/storage-locations', component: StorageLocationsComponent, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/units-of-measure', component: UnitsOfMeasureComponent, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'master-data/equipment', component: EquipmentComponent, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/products', component: CatalogItemsComponent, data: { type: 'Product' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/works', component: CatalogItemsComponent, data: { type: 'Work' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/primary-materials', component: CatalogItemsComponent, data: { type: 'PrimaryMaterial' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/auxiliary-materials', component: CatalogItemsComponent, data: { type: 'AuxiliaryMaterial' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/semi-finished-products', component: CatalogItemsComponent, data: { type: 'SemiFinishedProduct' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/packaging', component: CatalogItemsComponent, data: { type: 'Packaging' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/tooling-and-tools', component: CatalogItemsComponent, data: { type: 'ToolingAndTools' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/spare-parts', component: CatalogItemsComponent, data: { type: 'SparePart' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/purchased-services', component: CatalogItemsComponent, data: { type: 'PurchasedService' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'catalog/waste', component: CatalogItemsComponent, data: { type: 'Waste' }, canActivate: [permissionGuard(Permissions.masterDataView)] },
    { path: 'administration/organization', component: OrganizationComponent, canActivate: [permissionGuard(Permissions.organizationView)] },
    { path: 'administration', component: AdministrationComponent, canActivate: [permissionGuard(Permissions.usersManage)] }
  ] },
  { path: '**', redirectTo: '' }
];
