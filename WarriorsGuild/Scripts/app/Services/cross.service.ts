namespace WarriorsGuild {
    export class CrossService {
        crossStatusUrl: string = '/api/crossstatus';
        crossUrl: string = '/api/crosses';

        getCrossList = (success: (parm: Cross[]) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}`,
                success: success
            });
        }

        getPublicCross = (success: (parm: Cross) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/public`,
                success: success
            });
        }

        getCrossDetail = (crossId: string, success: (parm: Cross) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/${crossId}`,
                success: success
            });
        }

        retrieveCrossQuestions = (crossId: string, success: (parm: CrossQuestion[]) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/${crossId}/questions`,
                success: success
            });
        }

        retrieveTemplateQuestions = (success: (parm: CrossQuestion[]) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/questionsByTemplate/dayquestions`,
                success: success
            });
        }

        getCrossDays = (crossId: string, success: (parm: CrossDay[]) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/${crossId}/days`,
                success: success
            });
        }

        createCross = (crossDetail: { Name: string, Description: string }, success: (data: Cross) => void, error: (err: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: this.crossUrl,
                data: ko.toJSON(crossDetail),
                success: success,
                error: error
            });
        }

        updateCross = (crossDetail: { Id: string, Name: string, Description: string, ExplainText: string }, success: (data: Cross) => void, error: (err: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.put({
                url: `${this.crossUrl}/${crossDetail.Id}`,
                data: ko.toJSON(crossDetail),
                success: success,
                error: error
            });
        }

        updatePassages(crossId: string, daysToSave: { Passage: string, Weight: number, isCheckpoint: boolean, index: number, id: string }[], success: (data: CrossDay[]) => void, error: (err: JQueryXHR) => void) {
            WarriorsGuild.serviceBase.put({
                method: 'put',
                url: `${this.crossUrl}/${crossId}/days`,
                data: ko.toJSON(daysToSave),
                success: success,
                error: error
            });
        }

        savePassageAnswers = (crossId: string, dayId: string, answers: { CrossQuestionId: string, Answer: string }[], success: (parm: Cross) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.put({
                url: `${this.crossUrl}/${crossId}/day/${dayId}/answers`,
                data: ko.toJSON(answers),
                success: success,
                error: error
            });
        }

        saveSummaryAnswers = (crossId: string, answers: CrossQuestionAnswer[], success: (parm: Cross) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.put({
                url: `${this.crossUrl}/${crossId}/answers`,
                data: ko.toJSON(answers),
                success: success,
                error: error
            });
        }

        completeCrossDay = (crossId: string, dayId: string, answers: CrossQuestionAnswer[], success: () => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${crossId}/day/${dayId}/complete`,
                data: ko.toJSON(answers),
                success: success,
                error: error
            });
        }

        completeCross = (crossId: string, answers: CrossQuestionAnswer[], success: (parm: PendingCrossApproval) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${crossId}/complete`,
                data: ko.toJSON(answers),
                success: success,
                error: error
            });
        }

        confirmCrossDayComplete(approvalRecordId: string, success: () => void, error: (err: JQueryXHR) => void) {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${approvalRecordId}`,
                success: success,
                error: error
            });
        }

        confirmCrossComplete = (approvalRecordId: string, success: (parm: any) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${approvalRecordId}/confirmComplete`,
                success: success,
                error: error
            });
        }

        confirmCrossCheckpointComplete = (approvalRecordId: string, success: (parm: any) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${approvalRecordId}/confirmComplete`,
                success: success,
                error: error
            });
        }

        returnCross = (crossId: string, success: (parm: CrossDay[]) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${crossId}/return?reason=test`,
                success: success,
                error: error
            });
        }

        getPendingApprovals = (crossId: string, success: (parm: PendingCrossApproval[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossStatusUrl}/pendingapprovalsbycross/${crossId}`,
                success: success,
                error: error
            });
        }

        getUnassignedCrosses = (success: (parm: MinimumCrossDetail[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/unassigned`,
                success: success,
                error: error
            });
        }

        getCompletedCrossesForUser = (warriorId: string, success: (parm: Cross[]) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/byuser/${warriorId}/completed`,
                success: success,
                error: error
            });
        }

        getCompletedCrosses = (success: (parm: Cross[]) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/completed`,
                success: success,
                error: error
            });
        }

        getPinnedCrosses = (success: (parm: PinnedCross[]) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/pinned`,
                success: success,
                error: error
            });
        }

        pinCross(crossId: string, success: (ring: Cross) => void, error: (err: any) => void) {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossUrl}/pin/${crossId}`,
                success: success,
                error: error
            })
        }

        unpinCross(crossId: string, success: (ring: Cross) => void, error: (err: any) => void) {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossUrl}/unpin/${crossId}`,
                success: success,
                error: error
            })
        }

        updateCrossOrder = (crossOrder: { Id: string, Index: number }[], success: (data: Cross) => void, error: (err: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.post({
                async: false,
                timeout: 5000,
                url: `${this.crossUrl}/Order`,
                data: ko.toJSON(crossOrder),
                error: error,
                success: success
            });
        }
	}
}