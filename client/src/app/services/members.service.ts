import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../models/member';
import { environment } from 'src/environments/environment';
import { map, of } from 'rxjs';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/userParams';

@Injectable({
  providedIn: 'root'
})
export class MembersService
{

  members: Member[] = [];

  constructor(private http: HttpClient) { }

  getMembers(userParams: UserParams)
  {
	let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

	// TODO :: maybe add to getPaginationHeaders()
	params = params.append("minAge", userParams.minAge);
	params = params.append("maxAge", userParams.maxAge);
	params = params.append("gender", userParams.gender);

	return this.getPaginatedResult<Member[]>(environment.apiBasedUrl + "users", params);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams)
  {
	const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>;

	return this.http.get<T>(url, { observe: 'response', params }).pipe(
	  map(response =>
	  {
		if (response.body)
		{
		  paginatedResult.result = response.body;
		}
		const pagination = response.headers.get("Pagination");

		if (pagination)
		{
		  paginatedResult.pagination = JSON.parse(pagination);
		}
		return paginatedResult;
	  })
	);
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number)
  {
	let params = new HttpParams();

	params = params.append('pageNumber', pageNumber);
	params = params.append('pageSize', pageSize);

	return params;
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

  updateMember(member: Member)
  {
	return this.http.put(environment.apiBasedUrl + "users", member).pipe(map(() =>
	{
	  const index = this.members.indexOf(member);
	  this.members[index] = { ...this.members[index], ...member };
	}));
  }

  setMainPhoto(photoId: number)
  {
	return this.http.put(environment.apiBasedUrl + "users/set-main-photo/" + photoId, {});
  }

  deletePhoto(photoId: number)
  {
	return this.http.delete(environment.apiBasedUrl + "users/delete-photo/" + photoId, {});
  }
}
