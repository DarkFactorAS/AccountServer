

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

        public OAuth2RestClient() : base()
        {
        }

        override protected string GetModule()
        {
            return "ServerOAuth2";
        }

        public async Task<OAuth2AuthResponse> LoginOAuth2Client(OAuth2ClientData oauth2ClientData)
        {
            return await PutData<OAuth2AuthResponse>(POST_AUTH, "Auth", oauth2ClientData);
        }

        public async Task<OAuth2CodeResponse> LoginOAuth2WithCode(OAuth2CodeData codeData)
        {
            return await PutData<OAuth2CodeResponse>(POST_CODE, "Code", codeData);
        }
    }
}