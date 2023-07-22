import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../models/member';
import { environment } from 'src/environments/environment';
import { map, of } from 'rxjs';
import { PaginatedResult } from '../models/pagination';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  members:Member[] = [];
  paginatedResult : PaginatedResult<Member[]> = new PaginatedResult<Member[]>;

  constructor(private http:HttpClient) { }

  getMembers(page?:number, itemsPerPage?: number)
  {
    let params = new HttpParams();

    if (page && itemsPerPage)
    {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    return this.http.get<Member[]>(environment.apiBasedUrl + "users", {observe: 'response', params}).pipe(
      map(response=>
      {
        if (response.body)
        {
          this.paginatedResult.result = response.body;
        }
        const pagination = response.headers.get("Pagination");

        if (pagination)
        {
          this.paginatedResult.pagination = JSON.parse(pagination);
        }
        return this.paginatedResult;
      })
    );
  }

  getMember(username: string)
  {
    const member = this.members.find(user => user.userName == username);
    if (member)
    {
      return of(member);
    }
    return this.http.get<Member>(environment.apiBasedUrl + "users/" + username);
  }

  updateMember(member:Member)
  {
    return this.http.put(environment.apiBasedUrl + "users", member).pipe(map(()=>
    {
      const index = this.members.indexOf(member);
      this.members[index] = {...this.members[index], ...member};
    }));
  }

  setMainPhoto(photoId:number)
  {
    return this.http.put(environment.apiBasedUrl + "users/set-main-photo/" + photoId, {});
  }

  deletePhoto(photoId:number)
  {
    return this.http.delete(environment.apiBasedUrl + "users/delete-photo/" + photoId, {});
  }
}
