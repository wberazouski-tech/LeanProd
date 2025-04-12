import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemderListComponent } from './members/memder-list/memder-list.component';
import { MemderDetailComponent } from './members/memder-detail/memder-detail.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { authGuard } from './_guards/auth.guard';
import { TestErrorsComponent } from './errors/test-errors/test-errors.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            { path: 'members', component: MemderListComponent },
            { path: 'members/:id', component: MemderDetailComponent },
            { path: 'lists', component: ListsComponent },
            { path: 'messages', component: MessagesComponent },
        ]
    }, 
    {path: "errors", component: TestErrorsComponent},  
    {path: "not-found", component: NotFoundComponent},  
    {path: "server-error", component: ServerErrorComponent},   
    { path: '**', component: HomeComponent, pathMatch: 'full' },

];
