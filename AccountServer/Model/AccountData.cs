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
            WrongPassword,
            InvalidUsername,
            UsernameInvalidCharacters,
            NicknameInvalidCharacters,
            PasswordInvalidCharacters,
            NicknameAlreadyExist
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
                case ErrorCode.WrongPassword:
                    data.errorMessage = "WrongPassword";
                    break;
                case ErrorCode.InvalidUsername:
                    data.errorMessage = "InvalidUsername";
                    break;
                case ErrorCode.UsernameInvalidCharacters:
                    data.errorMessage = "UsernameInvalidCharacters";
                    break;
                case ErrorCode.NicknameInvalidCharacters:
                    data.errorMessage = "NicknameInvalidCharacters";
                    break;
                case ErrorCode.PasswordInvalidCharacters:
                    data.errorMessage = "PasswordInvalidCharacters";
                    break;
                case ErrorCode.NicknameAlreadyExist:
                    data.errorMessage = "NicknameAlreadyExist";
                    break;
            }

            return data;
        }
    }
}
