import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit
{
  @Output() onCancelRegister = new EventEmitter();
  model:any = {};

  constructor(private accountService:AccountService) { }

  ngOnInit(): void {
  }

  register()
  {
    this.accountService.register(this.model).subscribe({
      next: ()=>
      {
        this.cancel();
      },
      error:errors=>console.log(errors)

    });
  }

  cancel()
  {
    console.log("cancelled");
    this.onCancelRegister.emit(false);
  }

}
