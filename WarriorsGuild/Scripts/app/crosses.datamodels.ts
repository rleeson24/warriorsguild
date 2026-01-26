var crossSummaryQuestionsWeight: number = 10;

namespace WarriorsGuild {
	export class Cross {
		id: string;
		name: string;
		description: string;
		explainText: string;
		imageUploaded: Date | null;
		questions: CrossQuestion[];
		completedAt: Date | null;
		approvedAt: Date | null;
		days: CrossDay[];
		imageExtension: string;
	}

	export class PinnedCross {
		id: string;
		name: string;
		percentComplete: number;
		cross: Cross;
		crossId: string;
	}

	export class ObservableCross {
		id: KnockoutObservable<string> = ko.observable( '' );
		name: KnockoutObservable<string> = ko.observable( '' );
		description: KnockoutObservable<string> = ko.observable('');
		explainText: KnockoutObservable<string> = ko.observable( '');
		imageUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		imageExtension: KnockoutObservable<string> = ko.observable('');
		questions: KnockoutObservableArray<ObservableCrossQuestion> = ko.observableArray([]);
		days: KnockoutObservableArray<ObservableCrossDay> = ko.observableArray([]);
		guideUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		isPinned: KnockoutObservable<boolean> = ko.observable(false);
		isCompleted: KnockoutObservable<boolean> = ko.observable(false);

		pin: Function;
		unpin: Function;
		explainTextPreview: KnockoutComputed<string> = ko.pureComputed<string>(() => {
			if (this.questions().length > 0) {
				for (let i = 0; i < this.questions().length; i++) {
					var qText = this.questions()[i].text;
					if (qText.indexOf('{explain}') > -1) {
						return qText.replace('{explain}', this.explainText() ?? '[nothing entered]');
					}
				}
			}
			else {
				return '[nothing entered]';
			}
			return `oops`;
		}, this);
		hasImage: KnockoutComputed<boolean> = ko.pureComputed(() => {
			return this.imageUploaded() != null;
		}, this );

		addQuestion( data: ObservableCross ): void {
			data.questions.push( new ObservableCrossQuestion() );
		};

		addDay = (): void => {
			let oDay = new ObservableCrossDay();
			this.days.push(oDay);
		}

		createObservableCrossQuestion(arg0: string): ObservableCrossQuestion {
			let q = new ObservableCrossQuestion();
			q.text = arg0;
			return q;
		}

		imgSrcAttr = ko.pureComputed<string>(() => { return this.hasImage() ? '/images/crosses/' + this.id() + this.imageExtension() + '?' + this.imageUploaded().getTime() : '/images/logo/Warriors-Guild-icon-sm-wide.png' }, this);
		getDetailLink = ko.pureComputed<string>(() => { return `/crosses#detail?id=${this.id()}`; });

		markAsComplete: Function;
		completedAt: KnockoutObservable<Date | null> = ko.observable<Date | null>( null );
		approvedAt: KnockoutObservable<Date | null> = ko.observable<Date | null>( null );
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
			return this.formatDateTime( this.completedAt );
		} );
		formattedGuardianReviewedTs = ko.computed(() => {
			return this.formatDateTime( this.approvedAt );
		} );
		warriorCompleted = ko.computed(() => {
			return this.completedAt ? this.completedAt() !== null : false;
		} );
		guardianReviewed = ko.computed(() => {
			return this.approvedAt ? this.approvedAt() !== null : false;
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
		hasGuide: KnockoutComputed<boolean> = ko.pureComputed( () => {
			return this.guideUploaded() !== null;
		}, this);

		guideUploaderSettings: KnockoutComputed<{ key: string, id: string, postUrl: string, imgUrl: string, fileClass: string, successCallBack: Function }> = ko.pureComputed(() => {
			return {
				key: 'uploadRankDocumentation',
				id: this.id(),
				postUrl: crossUrls.uploadGuideUrl + '/' + this.id(),
				imgUrl: crossUrls.downloadGuideUrl + '/' + this.id(),
				fileClass: 'pdf',
				successCallBack: () => { this.guideUploaded(new Date()); }
			};
		}, this);
		downloadGuideUrl: KnockoutComputed<string> = ko.pureComputed(() => { return crossUrls.downloadGuideUrl + '/' + this.id(); }, this);
	}

	export class CrossQuestion {
		id: string;
		crossId: string;
		text: string;
		answer: string;
	}

	export class ObservableCrossQuestion {
		id: string = '';
		crossId: string = '';
		showEntryField: boolean = true;
		text: string = '';
		answer: KnockoutObservable<string> = ko.observable( '' );
	}

	export class CrossDay {
		id: string;
		crossId: string;
		passage: string;
		weight: number;
		index: number;
		isCheckpoint: boolean;
		questions: CrossQuestion[];
		completedAt: string;
		approvedAt: string;
	}

	export class ObservableCrossDay {
		id: string = null;
		passage: KnockoutObservable<string> = ko.observable<string>('');
		weight: KnockoutObservable<number> = ko.observable<number>(0);
		index: KnockoutObservable<number> = ko.observable<number>(0);
		isCheckpoint: KnockoutObservable<boolean> = ko.observable<boolean>(false);
		questions: KnockoutObservableArray<ObservableCrossQuestion> = ko.observableArray([]);
		completedAt: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		approvedAt: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		warriorCompleted = ko.computed(() => {
			return this.completedAt ? this.completedAt() !== null : false;
		});
		guardianReviewed = ko.computed(() => {
			return this.approvedAt ? this.approvedAt() !== null : false;
		});
		editing: KnockoutObservable<boolean> = ko.observable<boolean>(false);
	}

	export class PendingCrossApproval {
		approvalRecordId: string = '';
		crossId: string = '';
		crossName: string = '';
		percentComplete: number = 0;
		crossImageUploaded: KnockoutObservable<Date | null> = ko.observable<Date | null>(null);
		//unconfirmedRequirements: RankRequirement[] = new RankRequirement[0];
		imageExtension: string = '';
		hasImage: boolean;
		imgSrcAttr: string;
        dayId: string;
	}

	export class CrossQuestionAnswer {
		CrossId: string;
		CrossQuestionId: string;
		Answer: string;
	}
}