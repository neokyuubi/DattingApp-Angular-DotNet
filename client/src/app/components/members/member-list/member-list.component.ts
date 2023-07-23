import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/models/member';
import { Pagination } from 'src/app/models/pagination';
import { MembersService } from 'src/app/services/members.service';
import { UserParams } from '../../../models/userParams';
import { User } from '../../../models/user';
import { AccountService } from '../../../services/account.service';

@Component({
	selector: 'app-member-list',
	templateUrl: './member-list.component.html',
	styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit
{

	members: Member[] = [];
	pagination: Pagination | undefined;
	userParams: UserParams | undefined;
	user: User | undefined;
	genderList = [{ value: 'male', display: 'Males' }, { value: 'female', display: 'Females' }];

	constructor(private memberService: MembersService, accountService: AccountService)
	{
		accountService.currentUser$.pipe(take(1)).subscribe((user) =>
		{
			if (user)
			{
				this.userParams = new UserParams(user);
				this.user = user;
			}
		});
	}

	ngOnInit(): void
	{
		this.loadMembers();
	}

	loadMembers()
	{
		if (!this.userParams) return;
		this.memberService.getMembers(this.userParams).subscribe((response) =>
		{
			if (response.result && response.pagination)
			{
				this.members = response.result;
				this.pagination = response.pagination;
				console.log("this.pageSize", this.pagination);
				console.log("this.pagination", this.pagination.itemsPerPage);
			}
		})
	}

	resetFilters()
	{
		if (this.user)
		{
			this.userParams = new UserParams(this.user);
			this.loadMembers();
		}
	}

	pageChanged(event: any)
	{

		if (this.userParams && this.userParams.pageNumber != event.page)
		{
			this.userParams.pageNumber = event.page;
			this.loadMembers();
		}
	}

}
