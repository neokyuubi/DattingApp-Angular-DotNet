import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/models/member';
import { Photo } from 'src/app/models/photo';
import { MessageService } from '../../../services/message.service';
import { Message } from '../../../models/message';
import { PresenceService } from '../../../services/presence.service';
import { AccountService } from '../../../services/account.service';
import { User } from '../../../models/user';
import { take } from 'rxjs';
import { MembersService } from '../../../services/members.service';
import { ToastrService } from 'ngx-toastr';

@Component({
	selector: 'app-member-detail',
	templateUrl: './member-detail.component.html',
	styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy
{

	@ViewChild('memberTabs', {static:true}) memberTabs? : TabsetComponent;

	member: Member = {} as Member;
	galleryOptions: NgxGalleryOptions[] = [];
	galleryImages: NgxGalleryImage[] = [];
	activeTab?: TabDirective;
	messages:Message[] = [];
	user?:User;

	constructor(private memberService:MembersService, private toaster:ToastrService, private accountService: AccountService,
		private activatedRoute: ActivatedRoute,
		private messageService:MessageService, public presenceService:PresenceService,
		private router:Router)
	{
		this.accountService.currentUser$.pipe(take(1)).subscribe((user) =>
		{
			if(user) this.user = user;
		});
		this.router.routeReuseStrategy.shouldReuseRoute = () => false;
	}

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

	addLike(member:Member)
	{
		this.memberService.addLike(member.userName).subscribe(()=>
		{
			this.toaster.success("You have liked " + member.knownAs);
		});
	}

	onTabActivated(data:TabDirective)
	{
		this.activeTab = data;
		if (this.activeTab.heading == "Messages" && this.user)
		{
			this.messageService.createHubConnection(this.user, this.member.userName);
		}
		else
		{

			this.messageService.stopHubConnection();
		}
	}

	ngOnDestroy(): void
	{
		this.messageService.stopHubConnection();
	}

}
