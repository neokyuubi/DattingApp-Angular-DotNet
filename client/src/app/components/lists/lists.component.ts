import { Component, OnInit } from '@angular/core';
import { Member } from '../../models/member';
import { MembersService } from '../../services/members.service';

@Component({
	selector: 'app-lists',
	templateUrl: './lists.component.html',
	styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit
{

	members: Member[] | undefined;
	predicate = "liked";

	constructor(private memberService:MembersService) { }

	ngOnInit(): void
	{
		this.loadLikes();
	}

	loadLikes()
	{
		this.memberService.getLikes(this.predicate).subscribe((member)=>
		{
			this.members = member;
		});
	}

}
