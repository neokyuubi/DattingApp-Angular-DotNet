import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/models/member';
import { Photo } from 'src/app/models/photo';
import { MembersService } from 'src/app/services/members.service';
import { MessageService } from '../../../services/message.service';
import { Message } from '../../../models/message';
import { PresenceService } from '../../../services/presence.service';

@Component({
	selector: 'app-member-detail',
	templateUrl: './member-detail.component.html',
	styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit
{

	@ViewChild('memberTabs', {static:true}) memberTabs? : TabsetComponent;

	member: Member = {} as Member;
	galleryOptions: NgxGalleryOptions[] = [];
	galleryImages: NgxGalleryImage[] = [];
	activeTab?: TabDirective;
	messages:Message[] = [];

	constructor(private memberService: MembersService, private activatedRoute: ActivatedRoute,
		private messageService:MessageService, public presenceService:PresenceService) { }

	ngOnInit(): void
	{
		this.activatedRoute.data.subscribe((data) =>
		{
			this.member = data['member'];
		});

		this.activatedRoute.queryParams.subscribe((params)=>
		{
			params['tab'] && this.selectTab(params['tab']);
		});

		this.galleryOptions = [
			{
				width: "500px",
				height: "500px",
				imagePercent: 100,
				thumbnailsColumns: 4,
				imageAnimation: NgxGalleryAnimation.Slide,
				preview: false
			}
		];
		this.galleryImages = this.getImages();
	}



	getImages()
	{
		if (!this.member) return [];
		const imageUrls: NgxGalleryImage[] = [];
		this.member.photos.forEach((photo: Photo) =>
		{
			imageUrls.push({
				small: photo.url,
				medium: photo.url,
				big: photo.url
			});
		});
		return imageUrls;
	}


	selectTab(heading:string)
	{
		if (this.memberTabs)
		{
			this.memberTabs.tabs.find(tab => tab.heading == heading)!.active = true;
		}
	}


	loadMessages()
	{
		if (this.member)
		{
			this.messageService.getMessageThread(this.member.userName).subscribe((messages)=>
			{
				this.messages = messages;
			})
		}
	}

	onTabActivated(data:TabDirective)
	{
		this.activeTab = data;
		if (this.activeTab.heading == "Messages")
		{
			this.loadMessages();
		}
	}

}
