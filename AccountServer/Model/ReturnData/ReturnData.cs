using System;
using System.Collections.Generic;

namespace AccountServer.Model
{
    public class ReturnData
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

        public ReturnCode code { get; set; }
        public string message { get; set; }

        public ReturnData()
        {
        }

        public ReturnData(ReturnCode code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public static ReturnData OKMessage( string message = "OK" )
        {
            return new ReturnData(ReturnCode.OK, message);
        }

        public static ReturnData ErrorMessage( ReturnCode errorCode, string message )
        {
            return new ReturnData(errorCode, message);
        }

        public static ReturnData ErrorMessage( ReturnCode errorCode )
        {
            var data = new ReturnData();
            data.code = errorCode;
            data.message = GetMessageFromId(errorCode);
            return data;
        }

        public static String GetMessageFromId( ReturnCode code )
        {
            switch(code)
            {
                case UserDoesNotExist: return "User does not exist";
                case UserAlreadyExist: return "User already exist";
                case ErrorInData: return "Error in the data";
                case TokenLoginError: return "Token login error";
                case WrongPassword: return "Wrong password";
                case InvalidUsername: return "Wrong password";
                case UsernameInvalidCharacters: return "Username contaisn valid characters";
                case NicknameInvalidCharacters: return "Nickname contains valid characters";
                case PasswordInvalidCharacters: return "Password contains invalid characters. User A-Z, 0-9";
                case NicknameAlreadyExist: return "Nickname already exist";
                case NotValidEmail: return "Not a valid email";
                case SessionTimedOut: return "Code verification timed out. Please try again";
                case FailWithMailServer: return "Could not send code";

                default:
                    return Enum.GetName( typeof( ReturnCode ), code );
            }
        }
    }
}
