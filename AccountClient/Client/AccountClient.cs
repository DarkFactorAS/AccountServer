
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DFCommonLib.HttpApi;
using Newtonsoft.Json;

using AccountCommon.SharedModel;
using AccountClientModule.RestClient;
using AccountClientModule.Provider;
using DFCommonLib.HttpApi.OAuth2;

namespace AccountClientModule.Client
{
    public interface IAccountClient : IDFClient
    {
        void SetAuthCredentials(string clientId, string clientSecret, string scope);
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData LoginGameCenter(LoginGameCenterData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
        ReturnData ResetPasswordWithEmail(string emailAddress);
        ReturnData ResetPasswordWithCode(string code, string verifyEmailAddress);
        ReturnData ResetPasswordWithToken(string token, string password );

        // Admin functions. Only work for OAuth2 clients listed in the AccountServer's AdminClients.
        AdminUserListData AdminSearchUsers(string query);
        AdminUserData AdminGetUser(uint accountId);
        AdminUserData AdminUpdateUser(AdminUpdateUserData data);
        /// <summary>Password must be encrypted with DFCrypt.EncryptInput.</summary>
        AdminUserData AdminSetPassword(uint accountId, string encryptedPassword);
    }

    public class AccountClient : DFClient, IAccountClient
    {
        IAccountRestClient _restClient;
        IAccountSessionProvider _sessionProvider;

        public AccountClient(IAccountRestClient restClient, IAccountSessionProvider sessionProvider) : base(restClient)
        {
            _restClient = restClient;
            _sessionProvider = sessionProvider;
        }

        public void SetAuthCredentials(string clientId, string clientSecret, string scope)
        {
            _restClient.SetAuthClient( new OAuth2ClientData(){ ClientId = clientId, ClientSecret = clientSecret, Scope = scope } );
        }

        public AccountData LoginAccount(LoginData loginData)
        {
            var result = Task.Run(async () => await _restClient.LoginAccount(loginData)).Result;
            return ConvertFromRestData(result);
        }
    
        public AccountData LoginToken(LoginTokenData loginData)
        {
            var result = Task.Run(async() => await _restClient.LoginToken(loginData)).Result;
            return ConvertFromRestData( result );
        }

        public AccountData LoginGameCenter(LoginGameCenterData loginData)
        {
            if (loginData == null)
            {
                return new AccountData
                {
                    errorCode = AccountData.ErrorCode.ErrorInData,
                    errorMessage = "Login data cannot be null."
                };
            }

            if (string.IsNullOrWhiteSpace(loginData.nickname) || string.IsNullOrWhiteSpace(loginData.username) || string.IsNullOrWhiteSpace(loginData.password))
            {
                return new AccountData
                {
                    errorCode = AccountData.ErrorCode.ErrorInData,
                    errorMessage = "Nickname, username and password are required."
                };
            }

            var result = Task.Run(async() => await _restClient.LoginGameCenter(loginData)).Result;
            return ConvertFromRestData( result );
        }

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            var result = Task.Run(async() => await _restClient.CreateAccount(createAccountData)).Result;
            return ConvertFromRestData( result );
        }

        public ReturnData ResetPasswordWithEmail(string emailAddress)
        {
            var result = Task.Run(async() => await _restClient.ResetPasswordWithEmail(emailAddress)).Result;
            var returnData = ReturnData.ConvertFromReturnData( result );
            return returnData;
        }

        public ReturnData ResetPasswordWithCode(string code, string emailAddress)
        {
            var result = Task.Run(async() => await _restClient.ResetPasswordWithCode(code, emailAddress)).Result;
            var returnData = ReturnData.ConvertFromReturnData( result );
            return returnData;
        }

        public ReturnData ResetPasswordWithToken(string token, string password)
        {
            var result = Task.Run(async() => await _restClient.ResetPasswordWithToken(token,password)).Result;
            return ReturnData.ConvertFromReturnData( result );
        }

        public AdminUserListData AdminSearchUsers(string query)
        {
            var data = new AdminSearchUsersData { query = query ?? string.Empty };
            var result = Task.Run(async () => await _restClient.AdminSearchUsers(data)).Result;
            return ConvertAdmin(result, (code, message) => new AdminUserListData { errorCode = code, errorMessage = message });
        }

        public AdminUserData AdminGetUser(uint accountId)
        {
            var data = new AdminGetUserData { id = accountId };
            var result = Task.Run(async () => await _restClient.AdminGetUser(data)).Result;
            return ConvertAdmin(result, (code, message) => new AdminUserData { errorCode = code, errorMessage = message });
        }

        public AdminUserData AdminUpdateUser(AdminUpdateUserData data)
        {
            var result = Task.Run(async () => await _restClient.AdminUpdateUser(data)).Result;
            return ConvertAdmin(result, (code, message) => new AdminUserData { errorCode = code, errorMessage = message });
        }

        public AdminUserData AdminSetPassword(uint accountId, string encryptedPassword)
        {
            var data = new AdminSetPasswordData { id = accountId, password = encryptedPassword };
            var result = Task.Run(async () => await _restClient.AdminSetPassword(data)).Result;
            return ConvertAdmin(result, (code, message) => new AdminUserData { errorCode = code, errorMessage = message });
        }

        private static T ConvertAdmin<T>(WebAPIData apiData, System.Func<AccountData.ErrorCode, string, T> error) where T : class
        {
            if (apiData.errorCode == 0)
            {
                return JsonConvert.DeserializeObject<T>(apiData.message)
                    ?? error(AccountData.ErrorCode.ErrorInData, "Empty response from the account server");
            }
            return error((AccountData.ErrorCode)apiData.errorCode, apiData.message);
        }

        private AccountData ConvertFromRestData(WebAPIData apiData)
        {
            if ( apiData.errorCode == 0 )
            {
                var accountData = JsonConvert.DeserializeObject<AccountData>(apiData.message);
                return accountData;
            }
            else
            {
                var accountData = new AccountData();
                accountData.errorCode = (AccountData.ErrorCode)apiData.errorCode;
                accountData.errorMessage = apiData.message;
                return accountData;
            }
        }

        public static void SetupService( IServiceCollection services )
        {
            services.AddTransient<IAccountRestClient, AccountRestClient>();
            services.AddTransient<IAccountClient, AccountClient>();
            services.AddTransient<IAccountSessionProvider, AccountSessionProvider>();
        }
    }
}