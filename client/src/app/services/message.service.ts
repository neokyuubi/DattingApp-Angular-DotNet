import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { environment } from '../../environments/environment';
import { Message } from '../models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../models/user';
import { BehaviorSubject, take } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class MessageService
{
	private hubConnection?: HubConnection;
	private messageThreadSource = new BehaviorSubject<Message[]>([]);
	messageThread$ = this.messageThreadSource.asObservable();

	constructor(private http: HttpClient) { }

	createHubConnection(user: User, otherUsername: string)
	{
		this.hubConnection = new HubConnectionBuilder()
		.withUrl(environment.hubUrl + 'message?user=' + otherUsername,
		{
			accessTokenFactory: () => user.token
		})
		.withAutomaticReconnect()
		.build();

		this.hubConnection.start().catch(error => console.log(error));

		this.hubConnection.on("ReceiveMessageThread", messages =>
		{
			this.messageThreadSource.next(messages);
		});

		this.hubConnection.on("NewMessage", message =>
		{
			this.messageThread$.pipe(take(1)).subscribe((messages)=>
			{
				this.messageThreadSource.next([...messages, message]);
			});
		});
	}

	stopHubConnection()
	{
		if (this.hubConnection)
		{
			this.hubConnection.stop().catch(error => console.log(error));
		}
	}

	getMessages(pageNumber: number, pageSize: number, container: string)
	{
		let params = getPaginationHeaders(pageNumber, pageSize);
		params = params.append("container", container);
		return getPaginatedResult<Message[]>(environment.apiBasedUrl + "messages", params, this.http);
	}

	getMessageThread(username: string)
	{
		return this.http.get<Message[]>(environment.apiBasedUrl + 'messages/thread/' + username);
	}

	async sendMessage(username: string, content: string)
	{
		// return this.http.post<Message>(environment.apiBasedUrl + 'messages', { recipientUsername: username, content });
		return this.hubConnection?.invoke("SendMessage", {recipientUsername:username, content}).catch(error => console.log(error));
	}

	deleteMessage(id: number)
	{
		return this.http.delete(environment.apiBasedUrl + 'messages/' + id);
	}


}
