import { Component } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';


@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TranslocoPipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {}

