namespace WarriorsGuild {
	export class Ring {
		id: string;
		name: string;
		description: string;
		type: string;
		imageUploaded: Date | null;
		requirements: [{
			id: string;
			actionToComplete: string;
		}];
		statuses: RingStatus[];
		imageExtension: string;
	}

	export class PinnedRing {
		id: string;
		name: string;
		imageUploaded: Date | null;
		percentComplete: number;
		ring: Ring;
        ringId: string;
	}

	export class RingStatus {
		ringId: string;
		ringRequirementId: string;
		warriorCompleted: string | null;
		guardianCompleted: string | null;
	}

	export class ObservableRing {
		constructor() {
			var self = this;
			this.requestPromotionEnabled.subscribe(newValue => {
				if (newValue && !self.pendingApproval()) {
					$('#requestPromotionPopup').modal('show');
				}
			});
			this.pendingApproval.subscribe(newValue => {
				if (newValue) {
					$('#requestPromotionPopup').modal('hide');
				}
			});
		}
		id: KnockoutObservable<string> = ko.observable( '' );
		name: KnockoutObservable<string> = ko.observable( '' );
		description: KnockoutObservable<string> = ko.observable( '' );
		type: KnockoutObservable<string> = ko.observable( '' );
		imageUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		imageExtension: KnockoutObservable<string> = ko.observable('');
		requirements: KnockoutObservableArray<ObservableRingRequirement> = ko.observableArray( [] );
		approvalRecordId: KnockoutObservable<string> = ko.observable( null );
		guardianApprovedTs: KnockoutObservable<Date | null> = ko.observable<Date | null>( null );
		isPinned: KnockoutObservable<boolean> = ko.observable( false );
		isCompleted: KnockoutObservable<boolean> = ko.observable( false );
		warriorCompletedTs: KnockoutObservable<Date | null> = ko.observable<Date | null>( null );
		guardianReviewedTs: KnockoutObservable<Date | null> = ko.observable<Date | null>( null );
		guideUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>( null );

		pin: Function;
		unpin: Function;
		addRequirement( data: ObservableRing ): void {
			data.requirements.push( new ObservableRingRequirement() );
		};
		warriorCompleted = ko.computed( () => {
			return this.warriorCompletedTs ? this.warriorCompletedTs() !== null : false;
		} );
		guardianReviewed = ko.computed( () => {
			return this.guardianReviewedTs ? this.guardianReviewedTs() !== null : false;
		} );
		hasImage: KnockoutComputed<boolean> = ko.pureComputed(() => {
			return this.imageUploaded() != null;
		}, this );
		pendingApproval = ko.pureComputed( (): boolean => {
			return this.approvalRecordId() !== null && !this.guardianReviewed();
		}, this );
		completedPercent = ko.pureComputed(() => {
			var total = 0
			for ( var i = 0; i < this.requirements().length; i++ ) {
				if ( this.requirements()[i].warriorCompleted() ) {
					total += this.requirements()[i].weight();
				}
			}
			return total;
		}, this);
		requestPromotionEnabled = ko.pureComputed((): boolean => {
			return this.completedPercent() === 100 && !this.warriorCompleted();
		}, this);
		hasGuide: KnockoutComputed<boolean> = ko.pureComputed( () => {
			return this.guideUploaded() !== null;
		}, this );
		imgSrcAttr = ko.pureComputed<string>(() => { return this.hasImage() ? '/images/rings/' + this.id() + this.imageExtension() + '?' + this.imageUploaded().getTime() : '/images/logo/Warriors-Guild-icon-sm-wide.png' }, this);
		getDetailLink = ko.pureComputed<string>(() => { return `/rings#detail?id=${this.id()}`; });
	}

	export class RingRequirement extends BaseRequirement {
		ringId: string;
	}

	export class ObservableRingRequirement {
		constructor() {
            if ( this.requireAttachment() ) {
                this.warriorCompletedTs.subscribe( d => {
                    if ( d !== null ) {
                        return false;
                    }
                } )
            }
		}
		id: string = '';
		ringId: string = '';
		weight: KnockoutObservable<number> = ko.observable(0);
		actionToComplete: KnockoutObservable<string> = ko.observable('');
		markAsComplete: Function;
		revertCompletion: Function;
		warriorCompletedTs: KnockoutObservable<Date> = ko.observable(null);
		guardianReviewedTs: KnockoutObservable<Date> = ko.observable(null);
        requireAttachment: KnockoutObservable<boolean> = ko.observable<boolean>( false );
		attachments: KnockoutObservableArray<{ id: KnockoutObservable<string> }> = ko.observableArray<{ id: KnockoutObservable<string> }>([]);
		seeHowLink: KnockoutObservable<string> = ko.observable<string>(null);

		seeHowLinkInvalid: KnockoutComputed<boolean> = ko.pureComputed<boolean>(() => { return this.seeHowLink() !== null && this.seeHowLink().length !== 0 && !WarriorsGuild.isValidUrl(this.seeHowLink()) }, this);
		actionToCompleteLinked: KnockoutComputed<string> = ko.pureComputed<string>(() => { return this.actionToComplete()?.replace(/\n/g, '<br />').replace(/\[link ([a-zA-Z ]+)\]/g, `<a href='${this.seeHowLink()}' target='_blank'>$1</a>`); });

		formatDateTime = ( date ) => {
			var ts = date;
			var result = '';
			if ( ts != null ) {
				if ( ts() !== null ) {
					var hours = ( "0" + ts().getHours() );
					hours = hours.substr( hours.length - 2, 2 );
					var minutes = ( "0" + ts().getMinutes() );
					minutes = minutes.substr( minutes.length - 2, 2 );
					result = ts().toLocaleDateString() + " " + hours + ":" + minutes;
				}
			}
			return result;
		};
		formattedWarriorCompletedTs = ko.computed(() => {
			return this.formatDateTime( this.warriorCompletedTs );
		} );
		formattedGuardianReviewedTs = ko.computed(() => {
			return this.formatDateTime( this.guardianReviewedTs );
		} );
		warriorCompleted = ko.computed(() => {
			return this.warriorCompletedTs ? this.warriorCompletedTs() !== null : false;
		} );
		guardianReviewed = ko.computed(() => {
			return this.guardianReviewedTs ? this.guardianReviewedTs() !== null : false;
		} );
		completionSummaryText = ko.computed(() => {
			let result = '';
			if ( this.warriorCompleted() ) {
				result += 'Completed at ' + this.formattedWarriorCompletedTs();
				if ( this.guardianReviewed() )
					result += ';  Confirmed at ' + this.formattedGuardianReviewedTs();
			}
			return result;
        }, this );
        addAttachment = ( attIds: string[] ) => {
            $.each( attIds, ( index: number, att: string ) => {
                this.attachments.push( { id: ko.observable<string>( att ) } );
            } );
        };
	}

	export class PendingRingApproval {
		approvalRecordId: string = '';
		ringId: string = '';
		ringName: string = '';
		percentComplete: number = 0;
		ringImageUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		warriorCompleted: Date | null;
		guardianConfirmed: Date | null;
		//unconfirmedRequirements: RingRequirement[] = new RingRequirement[0];
		hasImage: boolean;
		imageExtension: string;
		imgSrcAttr: string;
	}
}