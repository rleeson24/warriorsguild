namespace WarriorsGuild {
    export class DateConversion {
        static convertStringToNullableDate(dateString: string): Date | null {
            if (!dateString) return null;
            var d = new Date(dateString);
            var date = Date.UTC(d.getFullYear(), d.getMonth(), d.getDate(),
                d.getHours(), d.getMinutes(), d.getSeconds());
            return new Date(date);
        }
    }
}