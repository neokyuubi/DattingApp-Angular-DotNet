import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http:HttpClient) { }

  login(model:any)
  {
    return this.http.post<User>(environment.apiBasedUrl + "account/login", model).pipe(
      map((user:User)=>
      {
        this.setCurrent(user);
		return user;
      })
    );
  }

  register(model:any)
  {
    return this.http.post<User>(environment.apiBasedUrl + "account/register", model).pipe(map(user=>{
      this.setCurrent(user);
    }))
  }

  setCurrent(user:User)
  {
    if (user)
    {
      localStorage.setItem("user", JSON.stringify(user));
      this.currentUserSource.next(user);
    }
  }

  logout()
  {
    localStorage.removeItem("user");
    this.currentUserSource.next(null);
  }


}
