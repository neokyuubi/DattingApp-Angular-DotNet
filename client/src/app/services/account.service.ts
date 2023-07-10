import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http:HttpClient) { }

  login(model:any)
  {
    return this.http.post(environment.apiBasedUrl + "account/login", model)
  }

}
