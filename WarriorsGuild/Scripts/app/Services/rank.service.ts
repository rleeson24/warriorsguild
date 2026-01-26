namespace WarriorsGuild {
    export class RankService {
        rankUrl: string = '/api/ranks';
        rankStatusUrl: string = '/api/rankstatus';

        getRanks = (success: (parm: Rank[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: this.rankUrl,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        getPublicRank = (success: (parm: Rank) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.get({
                url: rankUrls.publicRankUrl,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        getRankDetail = (rankId: string, success: (parm: Rank) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.get({
                url: this.rankUrl + '/' + rankId,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }        

        retrieveRankRequirements = (rankId: string, success: (parm: RankRequirement[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.rankUrl}/${rankId}/requirements`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        getPendingApprovals = (rankId: string, success: (parm: PendingRankApproval[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.rankStatusUrl}/approvalsForRank/${rankId}`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        markRequirementComplete = (rankId: string, rankRequirementId: string, success: (rank: Rank) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.post({
                url: `${this.rankStatusUrl}/RecordCompletion`,
                data: ko.toJSON({ RankRequirementId: rankRequirementId, RankId: rankId }),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        markRequirementCompleteByGuardian = (rankId: string, rankRequirementId: string, success: (rank: Rank) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.post({
                url: `${this.rankStatusUrl}/RecordCompletionAsGuardian`,
                data: ko.toJSON({ RankRequirementId: rankRequirementId, RankId: rankId }),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        create = (model: { Name: string, Description: string }, success: (parm: Rank) => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.post({
                url: this.rankUrl,
                data: ko.toJSON(model),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        update = (model: {
                Id: string, Name: string, Description: string, Requirements: {
                    RankId: string,
                    ActionToComplete: string,
                    Weight: number,
                    ImageUploaded: Date,
                    Index: number,
                    CrossesToComplete: { id: string }[]
                }[]
            }, success: () => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.put({
                url: `${this.rankUrl}/${model.Id}`,
                data: ko.toJSON(model),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        saveNewRankOrder = (rankOrder: { Id: string, Index: number }[], success: () => void, error: (parm: JQueryXHR) => void) => {
            WarriorsGuild.serviceBase.post({
                async: false,
                timeout: 5000,
                url: `${rankUrls.ranksUrl}/Order`,
                data: ko.toJSON(rankOrder),
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }


        returnToWarrior = (approvalRecordId: string, reasonForReturn: string, success: () => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.rankStatusUrl}/${approvalRecordId}/return?reason=${reasonForReturn}`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }

        confirmRankCompletion = (approvalRecordId: string, success: () => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.post({
                url: `${this.rankStatusUrl}/${approvalRecordId}/ApproveProgress`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }
    }
}