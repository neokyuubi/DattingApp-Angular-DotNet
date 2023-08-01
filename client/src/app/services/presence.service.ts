import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../models/user';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
	providedIn: 'root'
})
export class PresenceService
{
	private hubConnection?: HubConnection;
	private onlineUsersSource = new BehaviorSubject<string[]>([]);
	onlineUsers$ = this.onlineUsersSource.asObservable();

	constructor(private toaster:ToastrService, private router:Router) { }

	createHubConnection(user:User)
	{
		this.hubConnection = new HubConnectionBuilder()
		.withUrl(environment.hubUrl + 'presence', {
			accessTokenFactory : () => user.token
		})
		.withAutomaticReconnect()
		.build();

		this.hubConnection.start().catch(error => console.log(error));

		this.hubConnection.on("UserIsOnline", username =>
		{
			this.onlineUsers$.pipe(take(1)).subscribe((usernames)=>
			{
				this.onlineUsersSource.next([...usernames, username]);
			});
		});

		this.hubConnection.on("UserIsOffline", username =>
		{
			this.onlineUsers$.pipe(take(1)).subscribe((usernames)=>
			{
				this.onlineUsersSource.next(usernames.filter(u => u != username));
			});
		});

		this.hubConnection.on("GetOnlineUsers", usernames =>
		{
			this.onlineUsersSource.next(usernames);
		});

		this.hubConnection.on("NewMessageReceived", ({username, knownAs}) =>
		{
			this.toaster.info(knownAs + " has sent you a new message! \n Click to read the new incoming messages")
			.onTap
			.pipe(take(1))
			.subscribe(() =>
			{
				console.log(username, knownAs);
				console.log("/members/" + username + "?tab=Messages");

				this.router.navigateByUrl("/members/" + username + "?tab=Messages");
			})
			;
		});
	}

	stopHubConnection()
	{
		this.hubConnection?.stop().catch(error => console.log(error));
	}
}
