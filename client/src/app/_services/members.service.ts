import { HttpClient, HttpHandler, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  private httpr = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
 
  getMembers() {
    return this.httpr.get<Member[]>(this.baseUrl + 'users');
  }

  getMember(username: string) {
    return this.httpr.get<Member>(this.baseUrl + 'users/' + username);
  }


    
    
  //  updateMember(member: any) {
  //   return this.httpr.put(this.baseUrl + 'users', member);
  // }

  // setMainPhoto(photoId: number) {
  //   return this.httpr.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  // }

  // deletePhoto(photoId: number) {
  //   return this.httpr.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  // } 
}
