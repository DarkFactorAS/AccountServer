using System;
using System.Collections.Generic;
using DFCommonLib.HttpApi;
using Newtonsoft.Json;

namespace AccountCommon.SharedModel
{
    public class ReturnData : WebAPIData
    {
        public enum ReturnCode
        {
            OK,
            UserDoesNotExist,
            UserAlreadyExist,
            ErrorInData,
            TokenLoginError,
            WrongPassword,
            InvalidUsername,
            UsernameInvalidCharacters,
            NicknameInvalidCharacters,
            PasswordInvalidCharacters,
            NicknameAlreadyExist,
            NotValidEmail,
            SessionTimedOut,
            FailWithMailServer
        }
        
        public ReturnData() : base()
        {
        }

        public ReturnData( ReturnCode code, string message ) : base( (int)code, message )
        {
        }

        public static ReturnData OKMessage(string message = "OK")
        {
            return new ReturnData(ReturnCode.OK, message);
        }

        public static ReturnData ConvertFromReturnData(WebAPIData apiData)
        {
            if ( apiData.errorCode == 0 )
            {
                var returnObject = JsonConvert.DeserializeObject<ReturnData>(apiData.message);
                if ( returnObject == null )
                {
                    return new ReturnData(ReturnCode.ErrorInData, apiData.message);
                }
                return returnObject;
            }
            else
            {
                return new ReturnData((ReturnCode)apiData.errorCode, apiData.message);
            }
        }

        public static ReturnData ErrorMessage(ReturnCode errorCode, string message)
        {
            return new ReturnData(errorCode, message);
        }

        public static ReturnData ErrorMessage( ReturnCode errorCode )
        {
            var message = GetMessageFromId(errorCode);
            return new ReturnData(errorCode, message);
        }

        public static string GetMessageFromId( ReturnCode code )
        {
            switch (code)
            {
                case ReturnCode.UserDoesNotExist: return "User does not exist";
                case ReturnCode.UserAlreadyExist: return "User already exist";
                case ReturnCode.ErrorInData: return "Error in the data";
                case ReturnCode.TokenLoginError: return "Token login error";
                case ReturnCode.WrongPassword: return "Wrong password";
                case ReturnCode.InvalidUsername: return "Wrong password";
                case ReturnCode.UsernameInvalidCharacters: return "Username contaisn valid characters";
                case ReturnCode.NicknameInvalidCharacters: return "Nickname contains valid characters";
                case ReturnCode.PasswordInvalidCharacters: return "Password contains invalid characters. User A-Z, 0-9";
                case ReturnCode.NicknameAlreadyExist: return "Nickname already exist";
                case ReturnCode.NotValidEmail: return "Not a valid email";
                case ReturnCode.SessionTimedOut: return "Code verification timed out. Please try again";
                case ReturnCode.FailWithMailServer: return "Could not send code";

                default:
                    var enumName = Enum.GetName(typeof(ReturnCode), code);
                    return $"Unknown error code {enumName} ({(int)code})";
            }
        }
    }
}
