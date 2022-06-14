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
            SessionTimedOut
        }

        public ReturnCode code { get; set; }
        public string message { get; set; }

        public ReturnData()
        {
        }

        public static ReturnData OKMessage( string message = "" )
        {
            var data = new ReturnData();
            data.code = ReturnCode.OK;
            data.message = message;
            return data;
        }

        public static ReturnData ErrorMessage( ReturnCode errorCode )
        {
            var data = new ReturnData();
            data.code = errorCode;
            data.message = Enum.GetName( typeof( ReturnCode ), errorCode );
/*
            switch(errorCode)
            {
                case ReturnCode.UserDoesNotExist:
                    data.message = "UserDoesNotExist";
                    break;
                case ReturnCode.UserAlreadyExist:
                    data.message = "UserAlreadyExist";
                    break;
                case ReturnCode.ErrorInData:
                    data.message = "ErrorInData";
                    break;
                case ReturnCode.TokenLoginError:
                    data.message = "TokenLoginError";
                    break;
                case ReturnCode.WrongPassword:
                    data.message = "WrongPassword";
                    break;
                case ReturnCode.InvalidUsername:
                    data.message = "InvalidUsername";
                    break;
                case ReturnCode.UsernameInvalidCharacters:
                    data.message = "UsernameInvalidCharacters";
                    break;
                case ReturnCode.NicknameInvalidCharacters:
                    data.message = "NicknameInvalidCharacters";
                    break;
                case ReturnCode.PasswordInvalidCharacters:
                    data.message = "PasswordInvalidCharacters";
                    break;
                case ReturnCode.NicknameAlreadyExist:
                    data.message = "NicknameAlreadyExist";
                    break;
                case ReturnCode.NotValidEmail:
                    data.message = "NotValidEmail";
                    break;
                case ReturnCode.SessionTimedOut:
                    data.message = "SessionTimedOut";
                    break;
            }
*/
            return data;
        }
    }
}
