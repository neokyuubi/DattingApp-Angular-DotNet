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
    return this.http.get<Member[]>(environment.apiBasedUrl + "users", this.getHttpOptions());
  }

  getMember(username: string)
  {
    return this.http.get<Member>(environment.apiBasedUrl + "users/" + username, this.getHttpOptions());
  }

  getHttpOptions()
  {
    const userString = localStorage.getItem("user");
    if (!userString) return;

    const user = JSON.parse(userString);
    return {
      headers:new HttpHeaders({
        Authorization: 'Bearer ' + user.token
      })
    };
  }
}
