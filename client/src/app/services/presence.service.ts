import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../models/user';
import { BehaviorSubject } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class PresenceService
{
	private hubConnection?: HubConnection
	private onlineUsersSource = new BehaviorSubject<string[]>([]);
	onlineUsers$ = this.onlineUsersSource.asObservable();

	constructor(private toaster:ToastrService) { }

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
			this.toaster.info(username + " has connected");
		});

		this.hubConnection.on("UserIsOffline", username =>
		{
			this.toaster.warning(username + " has disconnected");
		});

		this.hubConnection.on("GetOnlineUsers", usernames =>
		{
			this.onlineUsersSource.next(usernames);
		});
	}

	stopHubConnection()
	{
		this.hubConnection?.stop().catch(error => console.log(error));
	}
}
