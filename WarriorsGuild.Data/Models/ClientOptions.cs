using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;


namespace WarriorsGuild.Data.Models
{
    public static class ClientOptions
    {
        public static IEnumerable<SelectListItem> StateOptions
        {
            get
            {
                var states = StateArray.States.Select( s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.Abbreviation
                } );
                return new SelectList( states, "Value", "Text" );
            }
        }

        public static IEnumerable<SelectListItem> ShirtSizeOptions
        {
            get
            {
                var sSizes = new[]
                {
                    "XS",
                    "S",
                    "M",
                    "L",
                    "XL",
                    "XXL",
                    "XXXL"
                };
                var sizes = sSizes.Select( s => new SelectListItem()
                {
                    Text = s,
                    Value = s
                } );
                return new SelectList( sizes, "Value", "Text" );
            }
        }
    }
}