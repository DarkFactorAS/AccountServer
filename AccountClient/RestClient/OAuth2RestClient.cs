

using System.Text;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using DFCommonLib.HttpApi;
using DFCommonLib.Logger;
using AccountCommon.SharedModel;
using Newtonsoft.Json;

namespace AccountClientModule.RestClient
{
    public interface IOAuth2RestClient : IDFRestClient
    {
        public Task<OAuth2AuthResponse> LoginOAuth2Client(OAuth2ClientData oauth2ClientData);
        public Task<OAuth2CodeResponse> LoginOAuth2WithCode(OAuth2CodeData codeData);
    }

    public class OAuth2RestClient : DFRestClient, IOAuth2RestClient
    {
        private const int POST_AUTH = 1;
        private const int POST_CODE = 2;

        public OAuth2RestClient(IDFLogger<DFRestClient> logger) : base(logger)
        {
        }

        override protected string GetModule()
        {
            return "ServerOAuth2";
        }

        public async Task<OAuth2AuthResponse> LoginOAuth2Client(OAuth2ClientData oauth2ClientData)
        {
            var response = await PutJsonData(POST_AUTH, "Auth", oauth2ClientData);
            var result = ConvertFromRestData<OAuth2AuthResponse>(response);
            return result;
        }

        public async Task<OAuth2CodeResponse> LoginOAuth2WithCode(OAuth2CodeData codeData)
        {
            var response = await PutJsonData(POST_CODE, "Code", codeData);
            var result = ConvertFromRestData<OAuth2CodeResponse>(response);
            return result;
        }

        private T ConvertFromRestData<T>(WebAPIData apiData) where T : WebAPIData, new()
        {
            if (apiData.errorCode == 0)
            {
                var data = JsonConvert.DeserializeObject<T>(apiData.message);
                return data;
            }
            var cls = new T();
            cls.errorCode = apiData.errorCode;
            cls.message = apiData.message;
            return cls;
        }

    }
}