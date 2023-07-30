import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/models/member';
import { Photo } from 'src/app/models/photo';
import { MembersService } from 'src/app/services/members.service';
import { MessageService } from '../../../services/message.service';
import { Message } from '../../../models/message';

@Component({
	selector: 'app-member-detail',
	templateUrl: './member-detail.component.html',
	styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit
{

	@ViewChild('memberTabs') memberTabs? : TabsetComponent;

	member: Member | undefined;
	galleryOptions: NgxGalleryOptions[] = [];
	galleryImages: NgxGalleryImage[] = [];
	activeTab?: TabDirective;
	messages:Message[] = [];

	constructor(private memberService: MembersService, private activatedRoute: ActivatedRoute, private messageService:MessageService) { }

	ngOnInit(): void
	{
		this.loadMember();

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

	loadMember()
	{
		const username = this.activatedRoute.snapshot.paramMap.get('username');
		if (!username) return;
		this.memberService.getMember(username).subscribe((member: Member) =>
		{
			this.member = member;
			this.galleryImages = this.getImages();
		});
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
