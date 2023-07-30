import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { environment } from '../../environments/environment';
import { Message } from '../models/message';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(private http:HttpClient) { }

  getMessages(pageNumber:number, pageSize:number, container:string)
  {
	let params = getPaginationHeaders(pageNumber, pageSize);
	params = params.append("container", container);
	return getPaginatedResult<Message[]>(environment.apiBasedUrl + "messages", params, this.http);
  }

  getMessageThread(username:string)
  {
	return this.http.get<Message[]>(environment.apiBasedUrl + 'messages/thread/' + username);
  }
}
