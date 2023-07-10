import { Component, OnInit } from '@angular/core';
import { BsDropdownConfig } from 'ngx-bootstrap/dropdown';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
  providers: [{ provide: BsDropdownConfig, useValue: { isAnimated: false, autoClose: true } }]
})
export class NavComponent implements OnInit
{

  model:any = {};
  loggedIn = false;

  constructor(private account:AccountService) { }

  ngOnInit(): void {
  }

  login()
  {
    this.account.login(this.model).subscribe(
    {
      next:response =>
      {
        console.log(response);
        this.loggedIn = true;
      },
      error: errors => console.log(errors)
    });
  }

  logout()
  {
    this.loggedIn = false;
  }

}
