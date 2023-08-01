import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit
{

	registerMode = false;

	constructor(private accountService: AccountService, private router: Router)
	{
		accountService.currentUser$.subscribe((user) =>
		{
			if (user)
			{
				router.navigateByUrl("/members")
			}
		});
	}

	ngOnInit(): void
	{
	}

	registerToggle()
	{
		this.registerMode = !this.registerMode;
	}



	cancelRegisterMode(event: boolean)
	{
		this.registerMode = event;
	}

}
