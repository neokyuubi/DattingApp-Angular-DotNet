import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../models/member';
import { environment } from 'src/environments/environment';
import { map, of, take } from 'rxjs';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/userParams';
import { AccountService } from './account.service';
import { User } from '../models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService
{
  members: Member[] = [];
  memberCache = new Map();
  userParams: UserParams | undefined;
  user: User | undefined;

  constructor(private http: HttpClient, accountService:AccountService)
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

  getUserParams()
  {
	return this.userParams;
  }

  setUserParams(params:UserParams)
  {
	this.userParams = params;
  }

  resetUserParams()
  {
	if (this.user)
	{
		this.userParams = new UserParams(this.user);
		return this.userParams;
	}
	return;
  }

  getMembers(userParams: UserParams)
  {
	const response = this.memberCache.get(Object.values(userParams).join("-"));
	if(response) return of(response);


	let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

	// TODO :: maybe add to getPaginationHeaders()
	params = params.append("minAge", userParams.minAge);
	params = params.append("maxAge", userParams.maxAge);
	params = params.append("gender", userParams.gender);
	params = params.append("orderBy", userParams.orderBy);

	return getPaginatedResult<Member[]>(environment.apiBasedUrl + "users", params, this.http).pipe(map((paginatedResultResponse)=>
	{
		this.memberCache.set(Object.values(userParams).join('-'), paginatedResultResponse);
		return paginatedResultResponse;
	}));
  }

  getMember(username: string)
  {
	const member = [...this.memberCache.values()]
	.reduce((arr, elem) => arr.concat(elem.result), [])
	.find((member:Member) => member.userName == username);
	if(member) return of(member);

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

  addLike(username:string)
  {
	return this.http.post(environment.apiBasedUrl + "likes/" + username, {});
  }

  getLikes(predicate:string, pageNumber:number, pageSize:number)
  {
	let params = getPaginationHeaders(pageNumber, pageSize);
	params = params.append("predicate", predicate);
	return getPaginatedResult<Member[]>(environment.apiBasedUrl + "likes", params, this.http);
  }
}
