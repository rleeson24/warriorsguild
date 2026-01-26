namespace WarriorsGuild.Models
{
    public class WorldCountry
    {
        public WorldCountry( string name, string alpha2Code, string alpha3Code, string numericCode, bool enabled )
        {
            Name = name;
            Alpha2Code = alpha2Code;
            Alpha3Code = alpha3Code;
            NumericCode = numericCode;
            Enabled = enabled;
        }

        public string Name { get; set; } = default!;
        public string Alpha2Code { get; set; } = default!;
        public string Alpha3Code { get; set; } = default!;
        public string NumericCode { get; set; } = default!;
        public bool Enabled { get; set; }

        public override string ToString()
        {
            //Returns "USA - United States"
            return string.Format( "{0} - {1}", Alpha3Code, Name );
        }
    }

    public class CountryArray
    {
        public List<WorldCountry> countries;
        public CountryArray()
        {
            countries = new List<WorldCountry>( 50 )
            {
                new WorldCountry( "Afghanistan", "AF", "AFG", "004", false ),
                new WorldCountry( "Aland Islands", "AX", "ALA", "248", false ),
                new WorldCountry( "Albania", "AL", "ALB", "008", false ),
                new WorldCountry( "Algeria", "DZ", "DZA", "012", false ),
                new WorldCountry( "American Samoa", "AS", "ASM", "016", false ),
                new WorldCountry( "Andorra", "AD", "AND", "020", false ),
                new WorldCountry( "Angola", "AO", "AGO", "024", false ),
                new WorldCountry( "Anguilla", "AI", "AIA", "660", false ),
                new WorldCountry( "Antarctica", "AQ", "ATA", "010", false ),
                new WorldCountry( "Antigua and Barbuda", "AG", "ATG", "028", false ),
                new WorldCountry( "Argentina", "AR", "ARG", "032", false ),
                new WorldCountry( "Armenia", "AM", "ARM", "051", false ),
                new WorldCountry( "Aruba", "AW", "ABW", "533", false ),
                new WorldCountry( "Australia", "AU", "AUS", "036", false ),
                new WorldCountry( "Austria", "AT", "AUT", "040", false ),
                new WorldCountry( "Azerbaijan", "AZ", "AZE", "031", false ),
                new WorldCountry( "Bahamas", "BS", "BHS", "044", false ),
                new WorldCountry( "Bahrain", "BH", "BHR", "048", false ),
                new WorldCountry( "Bangladesh", "BD", "BGD", "050", false ),
                new WorldCountry( "Barbados", "BB", "BRB", "052", false ),
                new WorldCountry( "Belarus", "BY", "BLR", "112", false ),
                new WorldCountry( "Belgium", "BE", "BEL", "056", false ),
                new WorldCountry( "Belize", "BZ", "BLZ", "084", false ),
                new WorldCountry( "Benin", "BJ", "BEN", "204", false ),
                new WorldCountry( "Bermuda", "BM", "BMU", "060", false ),
                new WorldCountry( "Bhutan", "BT", "BTN", "064", false ),
                new WorldCountry( "Bolivia, Plurinational State of", "BO", "BOL", "068", false ),
                new WorldCountry( "Bonaire, Sint Eustatius and Saba", "BQ", "BES", "535", false ),
                new WorldCountry( "Bosnia and Herzegovina", "BA", "BIH", "070", false ),
                new WorldCountry( "Botswana", "BW", "BWA", "072", false ),
                new WorldCountry( "Bouvet Island", "BV", "BVT", "074", false ),
                new WorldCountry( "Brazil", "BR", "BRA", "076", false ),
                new WorldCountry( "British Indian Ocean Territory", "IO", "IOT", "086", false ),
                new WorldCountry( "Brunei Darussalam", "BN", "BRN", "096", false ),
                new WorldCountry( "Bulgaria", "BG", "BGR", "100", false ),
                new WorldCountry( "Burkina Faso", "BF", "BFA", "854", false ),
                new WorldCountry( "Burundi", "BI", "BDI", "108", false ),
                new WorldCountry( "Cambodia", "KH", "KHM", "116", false ),
                new WorldCountry( "Cameroon", "CM", "CMR", "120", false ),
                new WorldCountry( "Canada", "CA", "CAN", "124", true ),
                new WorldCountry( "Cape Verde", "CV", "CPV", "132", false ),
                new WorldCountry( "Cayman Islands", "KY", "CYM", "136", false ),
                new WorldCountry( "Central African Republic", "CF", "CAF", "140", false ),
                new WorldCountry( "Chad", "TD", "TCD", "148", false ),
                new WorldCountry( "Chile", "CL", "CHL", "152", false ),
                new WorldCountry( "China", "CN", "CHN", "156", false ),
                new WorldCountry( "Christmas Island", "CX", "CXR", "162", false ),
                new WorldCountry( "Cocos (Keeling) Islands", "CC", "CCK", "166", false ),
                new WorldCountry( "Colombia", "CO", "COL", "170", false ),
                new WorldCountry( "Comoros", "KM", "COM", "174", false ),
                new WorldCountry( "Congo", "CG", "COG", "178", false ),
                new WorldCountry( "Congo, the Democratic Republic of the", "CD", "COD", "180", false ),
                new WorldCountry( "Cook Islands", "CK", "COK", "184", false ),
                new WorldCountry( "Costa Rica", "CR", "CRI", "188", false ),
                new WorldCountry( "Cote d'Ivoire", "CI", "CIV", "384", false ),
                new WorldCountry( "Croatia", "HR", "HRV", "191", false ),
                new WorldCountry( "Cuba", "CU", "CUB", "192", false ),
                new WorldCountry( "Curacao", "CW", "CUW", "531", false ),
                new WorldCountry( "Cyprus", "CY", "CYP", "196", false ),
                new WorldCountry( "Czech Republic", "CZ", "CZE", "203", false ),
                new WorldCountry( "Denmark", "DK", "DNK", "208", false ),
                new WorldCountry( "Djibouti", "DJ", "DJI", "262", false ),
                new WorldCountry( "Dominica", "DM", "DMA", "212", false ),
                new WorldCountry( "Dominican Republic", "DO", "DOM", "214", false ),
                new WorldCountry( "Ecuador", "EC", "ECU", "218", false ),
                new WorldCountry( "Egypt", "EG", "EGY", "818", false ),
                new WorldCountry( "El Salvador", "SV", "SLV", "222", false ),
                new WorldCountry( "Equatorial Guinea", "GQ", "GNQ", "226", false ),
                new WorldCountry( "Eritrea", "ER", "ERI", "232", false ),
                new WorldCountry( "Estonia", "EE", "EST", "233", false ),
                new WorldCountry( "Ethiopia", "ET", "ETH", "231", false ),
                new WorldCountry( "Falkland Islands (Malvinas)", "FK", "FLK", "238", false ),
                new WorldCountry( "Faroe Islands", "FO", "FRO", "234", false ),
                new WorldCountry( "Fiji", "FJ", "FJI", "242", false ),
                new WorldCountry( "Finland", "FI", "FIN", "246", false ),
                new WorldCountry( "France", "FR", "FRA", "250", false ),
                new WorldCountry( "French Guiana", "GF", "GUF", "254", false ),
                new WorldCountry( "French Polynesia", "PF", "PYF", "258", false ),
                new WorldCountry( "French Southern Territories", "TF", "ATF", "260", false ),
                new WorldCountry( "Gabon", "GA", "GAB", "266", false ),
                new WorldCountry( "Gambia", "GM", "GMB", "270", false ),
                new WorldCountry( "Georgia", "GE", "GEO", "268", false ),
                new WorldCountry( "Germany", "DE", "DEU", "276", false ),
                new WorldCountry( "Ghana", "GH", "GHA", "288", false ),
                new WorldCountry( "Gibraltar", "GI", "GIB", "292", false ),
                new WorldCountry( "Greece", "GR", "GRC", "300", false ),
                new WorldCountry( "Greenland", "GL", "GRL", "304", false ),
                new WorldCountry( "Grenada", "GD", "GRD", "308", false ),
                new WorldCountry( "Guadeloupe", "GP", "GLP", "312", false ),
                new WorldCountry( "Guam", "GU", "GUM", "316", false ),
                new WorldCountry( "Guatemala", "GT", "GTM", "320", false ),
                new WorldCountry( "Guernsey", "GG", "GGY", "831", false ),
                new WorldCountry( "Guinea", "GN", "GIN", "324", false ),
                new WorldCountry( "Guinea-Bissau", "GW", "GNB", "624", false ),
                new WorldCountry( "Guyana", "GY", "GUY", "328", false ),
                new WorldCountry( "Haiti", "HT", "HTI", "332", false ),
                new WorldCountry( "Heard Island and McDonald Islands", "HM", "HMD", "334", false ),
                new WorldCountry( "Holy See (Vatican City State)", "VA", "VAT", "336", false ),
                new WorldCountry( "Honduras", "HN", "HND", "340", false ),
                new WorldCountry( "Hong Kong", "HK", "HKG", "344", false ),
                new WorldCountry( "Hungary", "HU", "HUN", "348", false ),
                new WorldCountry( "Iceland", "IS", "ISL", "352", false ),
                new WorldCountry( "India", "IN", "IND", "356", false ),
                new WorldCountry( "Indonesia", "ID", "IDN", "360", false ),
                new WorldCountry( "Iran, Islamic Republic of", "IR", "IRN", "364", false ),
                new WorldCountry( "Iraq", "IQ", "IRQ", "368", false ),
                new WorldCountry( "Ireland", "IE", "IRL", "372", false ),
                new WorldCountry( "Isle of Man", "IM", "IMN", "833", false ),
                new WorldCountry( "Israel", "IL", "ISR", "376", false ),
                new WorldCountry( "Italy", "IT", "ITA", "380", false ),
                new WorldCountry( "Jamaica", "JM", "JAM", "388", false ),
                new WorldCountry( "Japan", "JP", "JPN", "392", false ),
                new WorldCountry( "Jersey", "JE", "JEY", "832", false ),
                new WorldCountry( "Jordan", "JO", "JOR", "400", false ),
                new WorldCountry( "Kazakhstan", "KZ", "KAZ", "398", false ),
                new WorldCountry( "Kenya", "KE", "KEN", "404", false ),
                new WorldCountry( "Kiribati", "KI", "KIR", "296", false ),
                new WorldCountry( "Korea, Democratic People's Republic of", "KP", "PRK", "408", false ),
                new WorldCountry( "Korea, Republic of", "KR", "KOR", "410", false ),
                new WorldCountry( "Kuwait", "KW", "KWT", "414", false ),
                new WorldCountry( "Kyrgyzstan", "KG", "KGZ", "417", false ),
                new WorldCountry( "Lao People's Democratic Republic", "LA", "LAO", "418", false ),
                new WorldCountry( "Latvia", "LV", "LVA", "428", false ),
                new WorldCountry( "Lebanon", "LB", "LBN", "422", false ),
                new WorldCountry( "Lesotho", "LS", "LSO", "426", false ),
                new WorldCountry( "Liberia", "LR", "LBR", "430", false ),
                new WorldCountry( "Libya", "LY", "LBY", "434", false ),
                new WorldCountry( "Liechtenstein", "LI", "LIE", "438", false ),
                new WorldCountry( "Lithuania", "LT", "LTU", "440", false ),
                new WorldCountry( "Luxembourg", "LU", "LUX", "442", false ),
                new WorldCountry( "Macao", "MO", "MAC", "446", false ),
                new WorldCountry( "Macedonia, the former Yugoslav Republic of", "MK", "MKD", "807", false ),
                new WorldCountry( "Madagascar", "MG", "MDG", "450", false ),
                new WorldCountry( "Malawi", "MW", "MWI", "454", false ),
                new WorldCountry( "Malaysia", "MY", "MYS", "458", false ),
                new WorldCountry( "Maldives", "MV", "MDV", "462", false ),
                new WorldCountry( "Mali", "ML", "MLI", "466", false ),
                new WorldCountry( "Malta", "MT", "MLT", "470", false ),
                new WorldCountry( "Marshall Islands", "MH", "MHL", "584", false ),
                new WorldCountry( "Martinique", "MQ", "MTQ", "474", false ),
                new WorldCountry( "Mauritania", "MR", "MRT", "478", false ),
                new WorldCountry( "Mauritius", "MU", "MUS", "480", false ),
                new WorldCountry( "Mayotte", "YT", "MYT", "175", false ),
                new WorldCountry( "Mexico", "MX", "MEX", "484", false ),
                new WorldCountry( "Micronesia, Federated States of", "FM", "FSM", "583", false ),
                new WorldCountry( "Moldova, Republic of", "MD", "MDA", "498", false ),
                new WorldCountry( "Monaco", "MC", "MCO", "492", false ),
                new WorldCountry( "Mongolia", "MN", "MNG", "496", false ),
                new WorldCountry( "Montenegro", "ME", "MNE", "499", false ),
                new WorldCountry( "Montserrat", "MS", "MSR", "500", false ),
                new WorldCountry( "Morocco", "MA", "MAR", "504", false ),
                new WorldCountry( "Mozambique", "MZ", "MOZ", "508", false ),
                new WorldCountry( "Myanmar", "MM", "MMR", "104", false ),
                new WorldCountry( "Namibia", "NA", "NAM", "516", false ),
                new WorldCountry( "Nauru", "NR", "NRU", "520", false ),
                new WorldCountry( "Nepal", "NP", "NPL", "524", false ),
                new WorldCountry( "Netherlands", "NL", "NLD", "528", false ),
                new WorldCountry( "New Caledonia", "NC", "NCL", "540", false ),
                new WorldCountry( "New Zealand", "NZ", "NZL", "554", false ),
                new WorldCountry( "Nicaragua", "NI", "NIC", "558", false ),
                new WorldCountry( "Niger", "NE", "NER", "562", false ),
                new WorldCountry( "Nigeria", "NG", "NGA", "566", false ),
                new WorldCountry( "Niue", "NU", "NIU", "570", false ),
                new WorldCountry( "Norfolk Island", "NF", "NFK", "574", false ),
                new WorldCountry( "Northern Mariana Islands", "MP", "MNP", "580", false ),
                new WorldCountry( "Norway", "NO", "NOR", "578", false ),
                new WorldCountry( "Oman", "OM", "OMN", "512", false ),
                new WorldCountry( "Pakistan", "PK", "PAK", "586", false ),
                new WorldCountry( "Palau", "PW", "PLW", "585", false ),
                new WorldCountry( "Palestine, State of", "PS", "PSE", "275", false ),
                new WorldCountry( "Panama", "PA", "PAN", "591", false ),
                new WorldCountry( "Papua New Guinea", "PG", "PNG", "598", false ),
                new WorldCountry( "Paraguay", "PY", "PRY", "600", false ),
                new WorldCountry( "Peru", "PE", "PER", "604", false ),
                new WorldCountry( "Philippines", "PH", "PHL", "608", false ),
                new WorldCountry( "Pitcairn", "PN", "PCN", "612", false ),
                new WorldCountry( "Poland", "PL", "POL", "616", false ),
                new WorldCountry( "Portugal", "PT", "PRT", "620", false ),
                new WorldCountry( "Puerto Rico", "PR", "PRI", "630", false ),
                new WorldCountry( "Qatar", "QA", "QAT", "634", false ),
                new WorldCountry( "Reunion", "RE", "REU", "638", false ),
                new WorldCountry( "Romania", "RO", "ROU", "642", false ),
                new WorldCountry( "Russian Federation", "RU", "RUS", "643", false ),
                new WorldCountry( "Rwanda", "RW", "RWA", "646", false ),
                new WorldCountry( "Saint BarthÃ©lemy", "BL", "BLM", "652", false ),
                new WorldCountry( "Saint Helena, Ascension and Tristan da Cunha", "SH", "SHN", "654", false ),
                new WorldCountry( "Saint Kitts and Nevis", "KN", "KNA", "659", false ),
                new WorldCountry( "Saint Lucia", "LC", "LCA", "662", false ),
                new WorldCountry( "Saint Martin (French part)", "MF", "MAF", "663", false ),
                new WorldCountry( "Saint Pierre and Miquelon", "PM", "SPM", "666", false ),
                new WorldCountry( "Saint Vincent and the Grenadines", "VC", "VCT", "670", false ),
                new WorldCountry( "Samoa", "WS", "WSM", "882", false ),
                new WorldCountry( "San Marino", "SM", "SMR", "674", false ),
                new WorldCountry( "Sao Tome and Principe", "ST", "STP", "678", false ),
                new WorldCountry( "Saudi Arabia", "SA", "SAU", "682", false ),
                new WorldCountry( "Senegal", "SN", "SEN", "686", false ),
                new WorldCountry( "Serbia", "RS", "SRB", "688", false ),
                new WorldCountry( "Seychelles", "SC", "SYC", "690", false ),
                new WorldCountry( "Sierra Leone", "SL", "SLE", "694", false ),
                new WorldCountry( "Singapore", "SG", "SGP", "702", false ),
                new WorldCountry( "Sint Maarten (Dutch part)", "SX", "SXM", "534", false ),
                new WorldCountry( "Slovakia", "SK", "SVK", "703", false ),
                new WorldCountry( "Slovenia", "SI", "SVN", "705", false ),
                new WorldCountry( "Solomon Islands", "SB", "SLB", "090", false ),
                new WorldCountry( "Somalia", "SO", "SOM", "706", false ),
                new WorldCountry( "South Africa", "ZA", "ZAF", "710", false ),
                new WorldCountry( "South Georgia and the South Sandwich Islands", "GS", "SGS", "239", false ),
                new WorldCountry( "South Sudan", "SS", "SSD", "728", false ),
                new WorldCountry( "Spain", "ES", "ESP", "724", false ),
                new WorldCountry( "Sri Lanka", "LK", "LKA", "144", false ),
                new WorldCountry( "Sudan", "SD", "SDN", "729", false ),
                new WorldCountry( "Suriname", "SR", "SUR", "740", false ),
                new WorldCountry( "Svalbard and Jan Mayen", "SJ", "SJM", "744", false ),
                new WorldCountry( "Swaziland", "SZ", "SWZ", "748", false ),
                new WorldCountry( "Sweden", "SE", "SWE", "752", false ),
                new WorldCountry( "Switzerland", "CH", "CHE", "756", false ),
                new WorldCountry( "Syrian Arab Republic", "SY", "SYR", "760", false ),
                new WorldCountry( "Taiwan, Province of China", "TW", "TWN", "158", false ),
                new WorldCountry( "Tajikistan", "TJ", "TJK", "762", false ),
                new WorldCountry( "Tanzania, United Republic of", "TZ", "TZA", "834", false ),
                new WorldCountry( "Thailand", "TH", "THA", "764", false ),
                new WorldCountry( "Timor-Leste", "TL", "TLS", "626", false ),
                new WorldCountry( "Togo", "TG", "TGO", "768", false ),
                new WorldCountry( "Tokelau", "TK", "TKL", "772", false ),
                new WorldCountry( "Tonga", "TO", "TON", "776", false ),
                new WorldCountry( "Trinidad and Tobago", "TT", "TTO", "780", false ),
                new WorldCountry( "Tunisia", "TN", "TUN", "788", false ),
                new WorldCountry( "Turkey", "TR", "TUR", "792", false ),
                new WorldCountry( "Turkmenistan", "TM", "TKM", "795", false ),
                new WorldCountry( "Turks and Caicos Islands", "TC", "TCA", "796", false ),
                new WorldCountry( "Tuvalu", "TV", "TUV", "798", false ),
                new WorldCountry( "Uganda", "UG", "UGA", "800", false ),
                new WorldCountry( "Ukraine", "UA", "UKR", "804", false ),
                new WorldCountry( "United Arab Emirates", "AE", "ARE", "784", false ),
                new WorldCountry( "United Kingdom", "GB", "GBR", "826", false ),
                new WorldCountry( "United States", "US", "USA", "840", true ),
                new WorldCountry( "United States Minor Outlying Islands", "UM", "UMI", "581", false ),
                new WorldCountry( "Uruguay", "UY", "URY", "858", false ),
                new WorldCountry( "Uzbekistan", "UZ", "UZB", "860", false ),
                new WorldCountry( "Vanuatu", "VU", "VUT", "548", false ),
                new WorldCountry( "Venezuela, Bolivarian Republic of", "VE", "VEN", "862", false ),
                new WorldCountry( "Viet Nam", "VN", "VNM", "704", false ),
                new WorldCountry( "Virgin Islands, British", "VG", "VGB", "092", false ),
                new WorldCountry( "Virgin Islands, U.S.", "VI", "VIR", "850", false ),
                new WorldCountry( "Wallis and Futuna", "WF", "WLF", "876", false ),
                new WorldCountry( "Western Sahara", "EH", "ESH", "732", false ),
                new WorldCountry( "Yemen", "YE", "YEM", "887", false ),
                new WorldCountry( "Zambia", "ZM", "ZMB", "894", false ),
                new WorldCountry( "Zimbabwe", "ZW", "ZWE", "716", false )
            };
        }

        /// <summary>
        /// List of 3 digit abbreviated country codes
        /// </summary>
        /// <returns></returns>
        public string[] Alpha3Codes()
        {
            List<string> abbrevList = new List<string>( countries.Count );
            foreach ( var country in countries )
            {
                if ( country.Enabled )
                    abbrevList.Add( country.Alpha3Code );
            }
            return abbrevList.ToArray();
        }

        /// <summary>
        /// List of 2 digit abbreviated country codes
        /// </summary>
        /// <returns></returns>
        public string[] Alpha2Codes()
        {
            List<string> abbrevList = new List<string>( countries.Count );
            foreach ( var country in countries )
            {
                if ( country.Enabled )
                    abbrevList.Add( country.Alpha2Code );
            }
            return abbrevList.ToArray();
        }

        /// <summary>
        /// List of Country names
        /// </summary>
        /// <returns></returns>
        public string[] Names()
        {
            List<string> nameList = new List<string>( countries.Count );
            foreach ( var country in countries )
            {
                if ( country.Enabled )
                    nameList.Add( country.Name );
            }
            return nameList.ToArray();
        }

        /// <summary>
        /// List of Countries
        /// </summary>
        /// <returns></returns>
        public WorldCountry[] Countries()
        {
            return countries.Where( c => c.Enabled == true ).ToArray();
        }
    }
}