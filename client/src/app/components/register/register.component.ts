import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
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

  constructor(private accountService:AccountService, private toaster:ToastrService) { }

  ngOnInit(): void {
  }

  register()
  {
    this.accountService.register(this.model).subscribe({
      next: ()=>
      {
        this.cancel();
      },
      error:errors=> this.toaster.error(errors.error, "Error")

    });
  }

  cancel()
  {
    console.log("cancelled");
    this.onCancelRegister.emit(false);
  }

}
