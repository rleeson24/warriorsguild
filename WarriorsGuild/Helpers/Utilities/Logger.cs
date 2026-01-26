using log4net;
using Newtonsoft.Json;
using WarriorsGuild.Email;

namespace WarriorsGuild.Helpers.Utilities
{
    public class Logger //: ILogger
    {
        private IEmailProvider _emailProvider;

        public bool IsDebugEnabled => Log4NetLogger.IsDebugEnabled;

        private ILog _log4NetLogger = default!;
        private ILog Log4NetLogger => _log4NetLogger ?? (_log4NetLogger = log4net.LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType ));

        public Logger( IEmailProvider emailProvider )
        {
            _emailProvider = emailProvider;
        }

        public async void Debug( string message, params object[] formatArgs )
        {
            var dataString = String.Join( Environment.NewLine, formatArgs.Select( o => o is Exception ? o.ToString() : JsonConvert.SerializeObject( o ) ) );
            await _emailProvider.SendAsync( "Warrior's Guild - Debug", message + (dataString.Length > 0 ? Environment.NewLine + Environment.NewLine + dataString : String.Empty), EmailView.Generic );
        }

        public void Error( Exception exception, string message, params object[] formatArgs )
        {
            Log4NetLogger.Error( message, exception );
        }

        public async void Error( string message, params object[] formatArgs )
        {
            var dataString = String.Join( Environment.NewLine, formatArgs.Select( o => o is Exception ? o.ToString() : JsonConvert.SerializeObject( o ) ) );
            await _emailProvider.SendAsync( "Warrior's Guild - Error", message + (dataString.Length > 0 ? Environment.NewLine + Environment.NewLine + dataString : String.Empty), EmailView.Generic );
        }

        public async void Info( string message, params object[] formatArgs )
        {
            var dataString = String.Join( Environment.NewLine, formatArgs.Select( o => o is Exception ? o.ToString() : JsonConvert.SerializeObject( o ) ) );
            await _emailProvider.SendAsync( "Warrior's Guild - Information", message + (dataString.Length > 0 ? Environment.NewLine + Environment.NewLine + dataString : String.Empty), EmailView.Generic );
        }

        public async Task LogMessage( string subject, string message, params object[] objectsToLog )
        {
            var dataString = String.Join( Environment.NewLine, objectsToLog.Select( o => o is Exception ? o.ToString() : JsonConvert.SerializeObject( o ) ) );
            await _emailProvider.SendAsync( "Warrior's Guild - " + subject, message + (dataString.Length > 0 ? Environment.NewLine + Environment.NewLine + dataString : String.Empty), EmailView.Generic );
        }

        public async void Warning( string message, params object[] formatArgs )
        {
            var dataString = String.Join( Environment.NewLine, formatArgs.Select( o => o is Exception ? o.ToString() : JsonConvert.SerializeObject( o ) ) );
            await _emailProvider.SendAsync( "Warrior's Guild - Warning", message + (dataString.Length > 0 ? Environment.NewLine + Environment.NewLine + dataString : String.Empty), EmailView.Generic );
        }
    }
}