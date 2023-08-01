import { Injectable } from '@angular/core';
import { CanDeactivate, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { MemberEditComponent } from '../components/members/member-edit/member-edit.component';
import { ConfirmService } from '../services/confirm.service';

@Injectable({
	providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<MemberEditComponent>
{

	constructor(private confirmService:ConfirmService){}

	canDeactivate(component: MemberEditComponent): Observable<boolean>
	{
		if (component.editForm?.dirty)
		{
			// return confirm("are you sure you want to continue? any changes will be lost");
			return this.confirmService.confirm();
		}
		return of(true);
	}
}
