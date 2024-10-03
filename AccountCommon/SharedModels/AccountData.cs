using System;
using System.Collections.Generic;

namespace AccountCommon.SharedModel
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
            NicknameAlreadyExist,
            NotValidEmail,
        }

        public uint id { get; set; }
        public string? nickname { get; set; }
        public string? token { get; set; }
        public ErrorCode errorCode { get; set; }
        public string? errorMessage { get; set; }

        public AccountData()
        {
        }

        public AccountData(uint id, string nickname, string token)
        {
            this.id = id;
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
                    data.errorMessage = "User does not exist";
                    break;
                case ErrorCode.UserAlreadyExist:
                    data.errorMessage = "User already exist";
                    break;
                case ErrorCode.ErrorInData:
                    data.errorMessage = "Error in data";
                    break;
                case ErrorCode.TokenLoginError:
                    data.errorMessage = "Token Loginerror";
                    break;
                case ErrorCode.WrongPassword:
                    data.errorMessage = "Wrong username/password";
                    break;
                case ErrorCode.InvalidUsername:
                    data.errorMessage = "Invalid Username";
                    break;
                case ErrorCode.UsernameInvalidCharacters:
                    data.errorMessage = "Username has invalid characters";
                    break;                                                                      
                case ErrorCode.NicknameInvalidCharacters:
                    data.errorMessage = "Nickname has invalid characters";
                    break;
                case ErrorCode.PasswordInvalidCharacters:
                    data.errorMessage = "Password has invalid characters";
                    break;
                case ErrorCode.NicknameAlreadyExist:
                    data.errorMessage = "Nickname already exist";
                    break;
                case ErrorCode.NotValidEmail:
                    data.errorMessage = "Not valid email";
                    break;
            }

            return data;
        }
    }
}
