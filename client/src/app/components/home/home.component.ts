import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  registerMode = false;
  users:any;

  constructor(private http: HttpClient) { }

  ngOnInit(): void
  {
    this.getUsers();
  }

  registerToggle()
  {
    this.registerMode = !this.registerMode;
  }

  getUsers()
  {
    this.http.get(environment.apiBasedUrl + "users").subscribe({
      next: response =>
      {
        this.users = response;
        console.log(this.users);
      },
      error: errors =>
      {
        console.log(errors);
      },
      complete: () => {
        console.log("completed");
      }
    });
  }

  cancelRegisterMode(event:boolean)
  {
    this.registerMode = event;
  }

}
