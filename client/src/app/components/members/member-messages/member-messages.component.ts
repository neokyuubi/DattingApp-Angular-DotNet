import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from '../../../models/message';
import { MessageService } from '../../../services/message.service';
import { NgForm } from '@angular/forms';

@Component({
	changeDetection: ChangeDetectionStrategy.OnPush,
	selector: 'app-member-messages',
	templateUrl: './member-messages.component.html',
	styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit
{
	@ViewChild('messageForm') messageForm?: NgForm;
	@Input() username? : string;
	messageContent = '';

	constructor(public messageService:MessageService) { }

	ngOnInit(): void
	{
	}

	sendMessage()
	{
		if(!this.username) return;
		this.messageService.sendMessage(this.username, this.messageContent).then((result)=>
		{
			this.messageForm?.reset();
		})
	}


}
