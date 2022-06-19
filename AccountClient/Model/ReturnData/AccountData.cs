using System;
using System.Collections.Generic;

namespace AccountClientModule.Model
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
    }
}
