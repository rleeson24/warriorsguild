namespace WarriorsGuild.Models
{
    public class CompletedItemListPopupModel
    {
        public string? PopupId { get; set; }
        public string? ViewModelPath { get; }
        public string? WarriorIdFromContext { get; }
        public string? ViewModelForShowPopup { get; set; }

        public CompletedItemListPopupModel( string? popupId, string? viewModelPath, string? warriorIdFromContext, string? vmForShowPopup = "" )
        {
            PopupId = popupId;
            ViewModelPath = viewModelPath;
            WarriorIdFromContext = warriorIdFromContext;
            ViewModelForShowPopup = vmForShowPopup;
        }
    }
}
