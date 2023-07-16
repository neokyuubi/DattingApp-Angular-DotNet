import { Injectable } from '@angular/core';
import { CanDeactivate, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../components/members/member-edit/member-edit.component';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<MemberEditComponent>
{
  canDeactivate(component: MemberEditComponent):boolean
  {
    if (component.editForm?.dirty)
    {
      return confirm("are you sure you want to continue? any changes will be lost");
    }
    return true;
  }
}
