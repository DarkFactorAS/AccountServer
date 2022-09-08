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
            data.message = Enum.GetName( typeof( ReturnCode ), errorCode );
            return data;
        }
    }
}
