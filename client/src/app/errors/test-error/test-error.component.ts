import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.css']
})
export class TestErrorComponent implements OnInit {

  constructor(private http:HttpClient) { }

  ngOnInit(): void {
  }

  get404Error()
  {
    this.http.get(environment.apiBasedUrl + "report/not-found").subscribe({
      next: response => console.log(response),
      error: errors => console.log(errors)
    })
  }

  get500Error()
  {
    this.http.get(environment.apiBasedUrl + "report/server-error").subscribe({
      next: response => console.log(response),
      error: errors => console.log(errors)
    })
  }

  get401Error()
  {
    this.http.get(environment.apiBasedUrl + "report/auth").subscribe({
      next: response => console.log(response),
      error: errors => console.log(errors)
    })
  }

}
