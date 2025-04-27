import { Component, inject, OnInit } from '@angular/core';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';
import { MemberCardComponent } from "../member-card/member-card.component";

@Component({
  selector: 'app-memder-list',
  standalone: true,
  imports: [MemberCardComponent],
  templateUrl: './memder-list.component.html',
  styleUrl: './memder-list.component.css'
})
export class MemderListComponent implements OnInit{

  private memberservice = inject(MembersService);
  members: Member[] = [];

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberservice.getMembers().subscribe({
      next: members => this.members = members,
    })
  }
}
