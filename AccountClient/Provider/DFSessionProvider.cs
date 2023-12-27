using Microsoft.AspNetCore.Http;

namespace AccountClientModule.Provider
{

    public class DFSessionProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _sessionKey;

        public DFSessionProvider( string sessionKey, IHttpContextAccessor httpContext )
        {
            _httpContextAccessor = httpContext;
            _sessionKey = sessionKey;
        }

        protected HttpContext GetContext()
        {
            if ( _httpContextAccessor != null )
            {
                return _httpContextAccessor.HttpContext;
            }
            return null;
        }

        protected string GetConfigString(string keyName)
        {
            var context = GetContext();
            if ( context != null )
            {
                return context.Session.GetString(_sessionKey + "." + keyName);
            }
            return null;
        }

        protected void SetConfigString(string keyName, string value)
        {
            var context = GetContext();
            if ( context != null )
            {
                if ( value != null )
                {
                    context.Session.SetString(_sessionKey + "." + keyName, value);
                } 
                else
                {
                    context.Session.Remove(_sessionKey + "." + keyName);
                }
            }
        }

        protected int? GetConfigInt(string keyName)
        {
            var context = GetContext();
            if ( context != null )
            {
                return context.Session.GetInt32(_sessionKey + "." + keyName);
            }
            return null;
        }

        protected void SetConfigInt(string keyName, int? value)
        {
            var context = GetContext();
            if ( context != null )
            {
                if ( value != null )
                {
                    context.Session.SetInt32(_sessionKey + "." + keyName, value.GetValueOrDefault());
                } 
                else
                {
                    context.Session.Remove(_sessionKey + "." + keyName);
                }
            }
        }

        protected void RemoveConfig(string keyName)
        {
            var context = GetContext();
            if ( context != null )
            {
                context.Session.Remove(_sessionKey + "." + keyName);
            }
        }
    }
}
