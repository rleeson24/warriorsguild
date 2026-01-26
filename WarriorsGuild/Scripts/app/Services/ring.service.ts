namespace WarriorsGuild {
    export class RingService {
        ringUrl: string = '/api/rings';
        ringStatusUrl: string = '/api/ringstatus';

        getCompletedRings = (warriorId: string, success: (parm: Ring[]) => void, error: (parm: JQueryXHR) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.ringUrl}/byuser/${warriorId}/completed`,
                contentType: "application/json; charset=utf-8",
                success: success,
                error: error
            });
        }
    }
}