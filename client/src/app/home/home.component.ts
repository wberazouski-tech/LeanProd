import { Component, inject, OnInit } from '@angular/core';
import { RegisterComponent } from "../register/register.component";


@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  
  registerMode = false;
  
  // ngOnInit(): void {
  //     this.getUser();

  // }
  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  canselRegisterMode(event: boolean)  {
    this.registerMode = event;
  }

  // getUser() {
  //   this.http.get('https://localhost:5001/api/users').subscribe({
  //     next: renponse => this.users = renponse,
  //     error: error => console.log(error),
  //     complete: () => console.log('Request has complited')
  //   })
  }

