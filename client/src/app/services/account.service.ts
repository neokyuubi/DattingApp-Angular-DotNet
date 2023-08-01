import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';
import { PresenceService } from './presence.service';

@Injectable({
	providedIn: 'root'
})
export class AccountService
{

	private currentUserSource = new BehaviorSubject<User | null>(null);
	currentUser$ = this.currentUserSource.asObservable();

	constructor(private http: HttpClient, private presenceService:PresenceService) { }

	login(model: any)
	{
		return this.http.post<User>(environment.apiBasedUrl + "account/login", model).pipe(
			map((user: User) =>
			{
				this.setCurrent(user);
				return user;
			})
		);
	}

	register(model: any)
	{
		return this.http.post<User>(environment.apiBasedUrl + "account/register", model).pipe(map(user =>
		{
			this.setCurrent(user);
		}))
	}

	setCurrent(user: User)
	{
		// if (user)
		// {
		user.roles = [];
		const roles = this.getDecodedToken(user.token).role;
		Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
		localStorage.setItem("user", JSON.stringify(user));
		this.currentUserSource.next(user);
		this.presenceService.createHubConnection(user);
		// }
	}

	logout()
	{
		localStorage.removeItem("user");
		this.currentUserSource.next(null);
		this.presenceService.stopHubConnection();
	}

	getDecodedToken(token: string)
	{
		return JSON.parse(atob(token.split(".")[1]));
	}


}
