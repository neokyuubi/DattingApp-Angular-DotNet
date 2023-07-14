import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  member:Member|undefined;

  constructor(private memberService:MembersService, private activatedRoute:ActivatedRoute) { }

  ngOnInit(): void
  {
    this.loadMember();
  }

  loadMember()
  {
    const username = this.activatedRoute.snapshot.paramMap.get('username');
    if(!username) return;
    this.memberService.getMember(username).subscribe((member:Member)=>
    {
      this.member = member;
    });
  }


}
