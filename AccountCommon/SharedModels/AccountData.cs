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
            PasswordInvalidLength,
            PasswordInvalidCharacters,
            NicknameAlreadyExist,
            NotValidEmail,
            NicknameAndUsernameEqual
        }

        public uint id { get; set; }
        public string? nickname { get; set; }
        public string? token { get; set; }
        public uint? flags { get; set; }

        public ErrorCode errorCode { get; set; }
        public string? errorMessage { get; set; }

        public AccountData()
        {
        }

        public AccountData(uint id, string nickname, string token, uint flags)
        {
            this.id = id;
            this.nickname = nickname;
            this.token = token;
            this.flags = flags;
        }

        public static AccountData Error(ErrorCode errorCode)
        {
            var data = new AccountData();
            data.errorCode = errorCode;
            data.errorMessage = GetErrorMessage(errorCode);
            return data;
        }

        private static string GetErrorMessage(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.UserDoesNotExist:
                    return "User does not exist";
                case ErrorCode.UserAlreadyExist:
                    return "User already exist";
                case ErrorCode.ErrorInData:
                    return "Error in data";
                case ErrorCode.TokenLoginError:
                    return "Token Loginerror";
                case ErrorCode.WrongPassword:
                    return "Wrong username/password";
                case ErrorCode.InvalidUsername:
                    return "Invalid Username";
                case ErrorCode.UsernameInvalidCharacters:
                    return "Username has invalid characters";
                case ErrorCode.NicknameInvalidCharacters:
                    return "Nickname has invalid characters";
                case ErrorCode.PasswordInvalidCharacters:
                    return "Password contains invalid characters. Valid characters are A-Z, 0-9, and special characters +$%*()";
                case ErrorCode.NicknameAlreadyExist:
                    return "Nickname already exist";
                case ErrorCode.NotValidEmail:
                    return "Not valid email";
                case ErrorCode.NicknameAndUsernameEqual:
                    return "Nickname and username cannot be equal";
            }

            return "Unknown error";
        }
    }
}
