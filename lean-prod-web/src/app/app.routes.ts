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

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forbidden', component: ForbiddenComponent },
  { path: '', component: ShellComponent, canActivate: [authGuard], children: [
    { path: '', component: HomeComponent },
    { path: 'sample-form', component: SampleFormComponent },
    { path: 'administration', component: AdministrationComponent, canActivate: [permissionGuard(Permissions.usersManage)] }
  ] },
  { path: '**', redirectTo: '' }
];
