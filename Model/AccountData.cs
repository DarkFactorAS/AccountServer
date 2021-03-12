using System;
using System.Collections.Generic;

namespace AccountServer.Model
{
    public class AccountData
    {
        public enum ErrorCode
        {
            OK,
            UserDoesNotExist,
            UserAlreadyExist,
            ErrorInData,
            TokenLoginError,
        }

        public string nickname { get; set; }
        public string token { get; set; }
        public ErrorCode errorCode { get; set; }
        public string errorMessage { get; set; }

        public AccountData()
        {
        }

        public AccountData(string nickname, string token)
        {
            this.nickname = nickname;
            this.token = token;
        }

        public static AccountData Error( ErrorCode errorCode )
        {
            var data = new AccountData();
            data.errorCode = errorCode;

            switch(errorCode)
            {
                case ErrorCode.UserDoesNotExist:
                    data.errorMessage = "UserDoesNotExist";
                    break;
                case ErrorCode.UserAlreadyExist:
                    data.errorMessage = "UserAlreadyExist";
                    break;
                case ErrorCode.ErrorInData:
                    data.errorMessage = "ErrorInData";
                    break;
                case ErrorCode.TokenLoginError:
                    data.errorMessage = "TokenLoginError";
                    break;
            }

            return data;
        }
    }
}
