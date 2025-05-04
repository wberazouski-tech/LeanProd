import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Interpolation } from '@angular/compiler';
import { MembersService } from '../../_services/members.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})

export class PhotoEditorComponent implements OnInit {
  
  private accoutService = inject(AccountService);
  private memberService = inject(MembersService);
  member = input.required<Member>();
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl
  memeberChange = output<Member>();
  

  ngOnInit(): void {
   this.initializeUploader();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  delitePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: _ => {
        const updateMember = {...this.member()} 
        updateMember.photos = updateMember.photos.filter(p => p.id !== photo.id);
        this.memeberChange.emit(updateMember);
      }
    })
  }

  setMainPhoto(photo: Photo){
    this.memberService.setMainPhoto(photo).subscribe({
      next: _ => {
        const user = this.accoutService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accoutService.setCurrentUser(user);
        }
        const updateMember = {...this.member()} 
        updateMember.photoUrl = photo.url;
        updateMember.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memeberChange.emit(updateMember);
      }
      })
  } 

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accoutService.currentUser()?.token,
      isHTML5: true,
      autoUpload: false,
      allowedFileType: ['image'],
      maxFileSize: 10 * 1024 * 1024,
      removeAfterUpload: true,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      const updateMember = {...this.member()} 
      updateMember.photos.push(photo);
      this.memeberChange.emit(updateMember);
    }


  }

}
