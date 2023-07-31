import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User } from '../models/user';
import { environment } from '../../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class AdminService
{

	constructor(private http: HttpClient) { }

	getUsersWithRoles()
	{
		return this.http.get<User[]>(environment.apiBasedUrl + 'admin/users-with-roles');
	}


	updateUserRoles(username: string, roles: string[])
	{
		return this.http.post<string[]>(environment.apiBasedUrl + 'admin/edit-roles/' + username + "?roles=" + roles, {} );
	}
}
