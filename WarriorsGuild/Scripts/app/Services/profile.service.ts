namespace WarriorsGuild {
    export class ProfileService {
        profileUrl: string = '/api/profile';

        retrieveProfileData = (userId: string, success: (parm: Profile) => void): void => {
            WarriorsGuild.serviceBase.get({
                url: `${this.profileUrl}/${userId}`,
                success: success
            });
        }

		setWarriorProfile = (warriorId: string): void => {
			WarriorsGuild.serviceBase.post({
				url: '/api/Profile/SetActiveWarrior',
				data: `"${warriorId}"`,
			});
		}

		togglePreviewMode = (success: () => void, error: (err: JQueryXHR) => void): void => {
			WarriorsGuild.serviceBase.post({
				url: '/api/Profile/TogglePreviewMode',
				success: success,
				error: error
			});
		}
    }
}