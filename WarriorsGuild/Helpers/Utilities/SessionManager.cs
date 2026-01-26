using Newtonsoft.Json;
using WarriorsGuild.Models;

namespace WarriorsGuild.Helpers.Utilities
{
    public class SessionManager : ISessionManager
    {
        private readonly ISession _session;
        private readonly IHttpContextAccessor httpContextAccessor;
        private const String ID_KEY = "_ID";
        private const String LOGIN_KEY = "_LoginName";
        private const String USERID_FOR_STATUS = "userIdForStatuses";
        private const String WARRIORS = "Warriors";
        private const String PREVIEW_MODE = "PreviewMode";
        private const String LAST_RETRIEVED_WARRIORS = "LastRetrievedWarriors";
        public SessionManager( IHttpContextAccessor httpContextAccessor )
        {
            _session = httpContextAccessor.HttpContext!.Session;
            this.httpContextAccessor = httpContextAccessor;
        }

        public int ID
        {
            get
            {
                var v = _session.GetInt32( ID_KEY );
                if ( v.HasValue )
                    return v.Value;
                else
                    return 0;
            }
            set
            {
                _session.SetInt32( ID_KEY, value );
            }
        }
        public string? LoginName
        {
            get
            {
                return _session.GetString( LOGIN_KEY );
            }
            set
            {
                _session.SetString( LOGIN_KEY, value! );
            }
        }

        public Boolean IsLoggedIn
        {
            get
            {
                if ( ID > 0 )
                    return true;
                else
                    return false;
            }
        }

        public string? UserIdForStatuses
        {
            get
            {
                var value = _session.GetString( USERID_FOR_STATUS );
                return value == String.Empty ? null : value;
            }
            set
            {
                _session.SetString( USERID_FOR_STATUS, value ?? String.Empty );
            }
        }

        public DateTime? LastRetrievedWarriors
        {
            get
            {
                return _session.GetObject<DateTime?>( LAST_RETRIEVED_WARRIORS );
            }
            set
            {
                _session.SetValue( LAST_RETRIEVED_WARRIORS, value );
            }
        }

        public IEnumerable<WarriorDropDownItem>? Warriors
        {
            get => _session.GetObject<IEnumerable<WarriorDropDownItem>>( WARRIORS );
            set
            {
                _session.SetValue( WARRIORS, value );
            }
        }

        public Boolean PreviewMode
        {
            get
            {
                var previewMode = _session.Keys.Any( k => k == PREVIEW_MODE ) ? _session.GetObject<Boolean>( PREVIEW_MODE ) : Boolean.Parse( httpContextAccessor.HttpContext!.Request.Cookies[ PREVIEW_MODE ] ?? "true" );

                if ( httpContextAccessor.HttpContext!.Request.Cookies[ "PreviewMode" ] == null )
                {
                    httpContextAccessor.HttpContext.Response.Cookies.Append( PREVIEW_MODE, previewMode.ToString() );
                }
                return previewMode;
            }
            set
            {
                _session.SetValue( PREVIEW_MODE, value );
                httpContextAccessor.HttpContext!.Response.Cookies.Append( PREVIEW_MODE, value.ToString() );
            }
        }

        public void Reset()
        {
            _session.Clear();
        }
    }

    public interface ISessionManager
    {
        int ID { get; set; }
        string? LoginName { get; set; }
        Boolean IsLoggedIn { get; }
        string? UserIdForStatuses { get; set; }
        DateTime? LastRetrievedWarriors { get; set; }
        IEnumerable<WarriorDropDownItem>? Warriors { get; set; }
        Boolean PreviewMode { get; set; }
        void Reset();
    }

    public static class SessionExtensions
    {
        public static void SetValue<T>( this ISession session, string key, T value )
        {
            session.SetString( key, JsonConvert.SerializeObject( value ) );
        }

        public static T? GetObject<T>( this ISession session, string key )
        {
            var value = session.GetString( key );
            return value == null ? default( T ) : JsonConvert.DeserializeObject<T>( value );
        }
    }
}
