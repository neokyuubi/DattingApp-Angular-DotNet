import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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
  registerForm:FormGroup = new FormGroup({});
  maxDate:Date = new Date();
  validationErrors:string[] | undefined;

  constructor(private accountService:AccountService,
    private toaster:ToastrService, private fb:FormBuilder, private router:Router) { }

  ngOnInit(): void
  {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm()
  {
    this.registerForm = this.fb.group(
    {
      gender: ["male"],
      knownAs: ["", Validators.required],
      dateOfBirth: ["", Validators.required],
      city: ["", Validators.required],
      country: ["", Validators.required],
      username: ["", Validators.required],
      password: ["", [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ["", [Validators.required, this.matchValues('password')]]
    });

    this.registerForm.controls['password'].valueChanges.subscribe(()=>
    {
      this.registerForm.controls['confirmPassword'].updateValueAndValidity();
    });
  }

  matchValues(matchTo:string): ValidatorFn
  {
    return (control:AbstractControl) =>
    {
      return control.value == control.parent?.get(matchTo)?.value ? null : {notMatching:true}
    };
  }

  register()
  {
    const dateOfBirth = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    const values = {...this.registerForm.value, dateOfBirth:dateOfBirth};

    this.accountService.register(values).subscribe({
      next: ()=>
      {
        this.router.navigateByUrl('/members');
      },
      error:errors=>{
        this.validationErrors = errors
      }
    });
  }

  cancel()
  {
    this.onCancelRegister.emit(false);
  }

  private getDateOnly(dateOfBirth: string | undefined)
  {
    if (!dateOfBirth) return;
    let theDateOfBirth = new Date(dateOfBirth);
    return new Date(theDateOfBirth.setMinutes(theDateOfBirth.getMinutes() - theDateOfBirth.getTimezoneOffset())).toISOString().slice(0, 10);
  }

}
