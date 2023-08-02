import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/models/member';
import { MembersService } from '../../../services/members.service';
import { ToastrService } from 'ngx-toastr';
import { PresenceService } from '../../../services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {

  @Input() member:Member | undefined;

  constructor(private memberService:MembersService, private toaster:ToastrService, public presenceService:PresenceService) { }

  ngOnInit(): void {
  }

  addLike(member:Member)
  {
	this.memberService.addLike(member.userName).subscribe(()=>
	{
		this.toaster.success("You have liked " + member.knownAs);
	});
  }

}
