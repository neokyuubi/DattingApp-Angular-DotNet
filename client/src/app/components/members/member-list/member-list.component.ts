import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/models/member';
import { Pagination } from 'src/app/models/pagination';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  // members$: Observable<Member[]> | undefined;
  members: Member[] = [];
  pagination:Pagination | undefined;
  pageNumber = 1;
  pageSize = 5;

  constructor(private memberService:MembersService) { }

  ngOnInit(): void
  {
    // this.members$ = this.memberService.getMembers();
    this.loadMembers();
  }

  loadMembers()
  {
    this.memberService.getMembers(this.pageNumber, this.pageSize).subscribe((response)=>
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

  pageChanged(event:any)
  {

    if(this.pageNumber != event.page)
    {
      this.pageNumber = event.page;
      this.loadMembers();
    }
  }

}
