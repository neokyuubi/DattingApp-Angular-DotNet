import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../models/member';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  constructor(private http:HttpClient) { }

  getMembers()
  {
    return this.http.get<Member[]>(environment.apiBasedUrl + "users");
  }

  getMember(username: string)
  {
    return this.http.get<Member>(environment.apiBasedUrl + "users/" + username);
  }
}
