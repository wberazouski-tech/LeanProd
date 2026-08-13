import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [RouterLink],
  template: `<h1>Недастаткова правоў</h1><p>У вас няма доступу да гэтай функцыі.</p><a routerLink="/">На галоўную</a>`
})
export class ForbiddenComponent {}
