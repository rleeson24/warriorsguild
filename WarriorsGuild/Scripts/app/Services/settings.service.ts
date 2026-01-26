namespace WarriorsGuild {
    export class SettingsService {
        crossStatusUrl: string = '/api/crossstatus';
        crossUrl: string = '/api/crosses';

        getCrossDetail = (crossId: string, success: (parm: Cross) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/${crossId}`,
                contentType: "application/json; charset=utf-8",
                success: success
            });
        }

        retrieveCrossQuestions = (crossId: string, success: (parm: CrossQuestion[]) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/${crossId}/questions`,
                contentType: "application/json; charset=utf-8",
                success: success
            });
        }

        getCrossDays = (crossId: string, success: (parm: CrossDay[]) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/${crossId}/days`,
                contentType: "application/json; charset=utf-8",
                success: success
            });
        }

        saveAnswers = (crossId: string, answers: CrossQuestionAnswer[], success: (parm: Cross) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.put({
                url: `${this.crossUrl}/${crossId}/answers`,
                data: ko.toJSON(answers),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        completeCross = (crossId: string, answers: CrossQuestionAnswer[], success: (parm: PendingCrossApproval) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${crossId}/complete`,
                data: ko.toJSON(answers),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        confirmCrossComplete = (approvalRecordId: string, success: (parm: any) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${approvalRecordId}/confirmComplete`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        confirmCrossCheckpointComplete = (approvalRecordId: string, success: (parm: any) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${approvalRecordId}/confirmComplete`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        returnCross = (crossId: string, success: (parm: CrossDay[]) => void, error: (parm: any) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.crossStatusUrl}/${crossId}/return?reason=test`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        getPendingApprovals = (crossId: string, success: (parm: PendingCrossApproval[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossStatusUrl}/pendingapprovalsbycross/${crossId}`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        getUnassignedCrosses = (success: (parm: MinimumCrossDetail[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/unassigned`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        getCompletedCrosses = (warriorId: string, success: (parm: Cross[]) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.get({
                url: `${this.crossUrl}/byuser/${warriorId}/completed`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }
	}
}