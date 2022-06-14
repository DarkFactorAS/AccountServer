using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AccountServer.Model;
using AccountServer.Repository;
using DFCommonLib.Utils;

namespace AccountServer.Provider
{
    public interface IAccountProvider
    {
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
        ReturnData ResetPasswordWithEmail(string emailAddress);
        ReturnData ResetPasswordWithCode(string emailAddress, string code);
        ReturnData ResetPasswordWithToken(string token, string password );
    }

    public class AccountProvider : IAccountProvider
    {
        IAccountRepository _repository;
        IAccountSessionProvider _session;

        public AccountProvider(IAccountRepository repository, IAccountSessionProvider session)
        {
            _repository = repository;
            _session = session;
        }

        public AccountData CreateAccount(CreateAccountData createAccountData)
        {
            if ( createAccountData == null )
            {
                return AccountData.Error(AccountData.ErrorCode.ErrorInData);
            }

            // Verify nickname
            var nicknameStatus = VerifyNickname(createAccountData.nickname);
            if ( nicknameStatus != AccountData.ErrorCode.OK )
            {
                return AccountData.Error(nicknameStatus);
            }

            // Verify username
            var usernameStatus = VerifyUsername(createAccountData.username);
            if ( usernameStatus != AccountData.ErrorCode.OK )
            {
                return AccountData.Error(usernameStatus);
            }

            // Verify password
            var plainPassword = DFCrypt.DecryptInput(createAccountData.password);
            var passwordStatus = VerifyPassword(plainPassword);
            if ( passwordStatus != AccountData.ErrorCode.OK )
            {
                return AccountData.Error(passwordStatus);
            }

            // Zero email if needed
            createAccountData.email = VerifyEmail(createAccountData.email);

            var salt = generateSalt();
            createAccountData.password = generateHash(plainPassword, salt);
            var internalAccount = _repository.CreateAccount(createAccountData, salt);

            // Create logintoken
            if ( internalAccount != null )
            {
                string token = CreateToken(internalAccount.id );
                return new AccountData( internalAccount.nickname, token );
            }

            return AccountData.Error(AccountData.ErrorCode.ErrorInData);
        }

        public AccountData LoginAccount(LoginData loginData)
        {
            InternalAccountData accountData = _repository.GetAccountWithUsername(loginData.username);
            if ( accountData == null )
            {
                return AccountData.Error( AccountData.ErrorCode.UserDoesNotExist);
            }

            var plainPassword = DFCrypt.DecryptInput(loginData.password);

            string encryptedPassword = generateHash(plainPassword, accountData.salt);
            if ( accountData != null && accountData.password == encryptedPassword )
            {
                string token = CreateToken(accountData.id);
                return new AccountData( accountData.nickname, token );
            }
            return AccountData.Error( AccountData.ErrorCode.WrongPassword);
        }

        public AccountData LoginToken(LoginTokenData loginTokenData)
        {
            if ( loginTokenData!= null && !string.IsNullOrEmpty(loginTokenData.token) )
            {
                var accountData = _repository.GetAccountWithToken(loginTokenData.token);
                if ( accountData != null )
                {
                    string token = CreateToken(accountData.id);
                    return new AccountData( accountData.nickname, token);
                }
            }
            return AccountData.Error( AccountData.ErrorCode.UserDoesNotExist);
        }

        public ReturnData ResetPasswordWithEmail(string emailAddress)
        {
            var vEmail = VerifyEmail(emailAddress);
            if ( !string.IsNullOrEmpty(vEmail) )
            {
                var accountData = _repository.GetAccountWithEmail(emailAddress);
                if ( accountData != null )
                {
                    var twoFactorCode = GenerateCode();

                    _session.SetAccountId(accountData.id);
                    _session.SetAccountCode(twoFactorCode);

                    // TODO: Set this in reset with 2FA
                    _session.SetAccountToken("1234");

                    // TODO: Send 2FA+email to email server
                    return ReturnData.OKMessage("The code was sent to " + emailAddress);
                }
                return ReturnData.OKMessage("Unknown user with email" + emailAddress);
            }

            return ReturnData.ErrorMessage( ReturnData.ReturnCode.NotValidEmail );
        }

        public ReturnData ResetPasswordWithCode(string code, string emailAddress)
        {
            var sessionCode = _session.GetAccountCode();
            if ( sessionCode != code || sessionCode == null )
            {
                return ReturnData.ErrorMessage( ReturnData.ReturnCode.SessionTimedOut);
            }

            var token = GenerateToken();
            _session.SetAccountToken(token);
            return ReturnData.OKMessage(token);
        }

        public ReturnData ResetPasswordWithToken(string token, string password)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(password))
            {
                return ReturnData.ErrorMessage( ReturnData.ReturnCode.UserDoesNotExist);
            }

            var accountId = _session.GetAccountId();
            if ( accountId == 0 )
            {
                return ReturnData.ErrorMessage( ReturnData.ReturnCode.SessionTimedOut);
            }

            var sessionToken = _session.GetAccountToken();
            if ( sessionToken != token )
            {
                return ReturnData.ErrorMessage( ReturnData.ReturnCode.SessionTimedOut);
            }

            // Verify passwordrules
            var passwordCode = VerifyPassword(password);
            if ( passwordCode != AccountData.ErrorCode.OK )
            {
                return ReturnData.ErrorMessage( ReturnData.ReturnCode.UserDoesNotExist );
            }

            InternalAccountData accountData = _repository.GetAccountWithId(accountId);
            if ( accountData == null )
            {
                return ReturnData.ErrorMessage( ReturnData.ReturnCode.SessionTimedOut);
            }

            var hashedPassword = generateHash(password, accountData.salt);

            // Set new password
            _repository.SetNewPassword(accountId, hashedPassword);

            return ReturnData.OKMessage();
        }

        private string GenerateCode()
        {
            return "1234";
        }

        private AccountData.ErrorCode VerifyNickname( string nickname )
        {
            if ( string.IsNullOrEmpty( nickname ) )
            {
                return AccountData.ErrorCode.ErrorInData;
            }

            // Only A-Z and 0-9
            if ( !Regex.IsMatch(nickname, @"^[a-zA-Z0-9]+$") )
            {
                return AccountData.ErrorCode.NicknameInvalidCharacters;
            }

            // Nickname already exist ?
            InternalAccountData accountData = _repository.GetAccountWithNickname(nickname);
            if ( accountData != null )
            {
                return AccountData.ErrorCode.NicknameAlreadyExist;
            }

            return AccountData.ErrorCode.OK;
        }

        private AccountData.ErrorCode VerifyUsername( string username )
        {
            if ( string.IsNullOrEmpty( username ) )
            {
                return AccountData.ErrorCode.ErrorInData;
            }

            // Only A-Z and 0-9
            if ( !Regex.IsMatch(username, @"^[a-zA-Z0-9]+$") )
            {
                return AccountData.ErrorCode.UsernameInvalidCharacters;
            }

            // Username already exist ?
            InternalAccountData accountData = _repository.GetAccountWithUsername(username);
            if ( accountData != null )
            {
                return AccountData.ErrorCode.UserAlreadyExist;
            }

            return AccountData.ErrorCode.OK;
        }

        private AccountData.ErrorCode VerifyPassword( string password )
        {
            if ( string.IsNullOrEmpty( password ) )
            {
                return AccountData.ErrorCode.ErrorInData;
            }

            // Only A-Z and 0-9
            if ( !Regex.IsMatch(password, @"^[a-zA-Z0-9]+$") )
            {
                return AccountData.ErrorCode.PasswordInvalidCharacters;
            }

            return AccountData.ErrorCode.OK;
        }

        private string VerifyEmail(string email)
        {
            if ( email == null )
            {
                return "";
            }

            // TODO : Verify email string

            return email;
        }

        private string generateHash(string password, byte[] salt) 
        {
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

        private byte[] generateSalt() 
        {
            // generate a 128-bit (16*8) salt using a secure PRNG
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) 
            {
                rng.GetBytes(salt);
            }
            return salt; 
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        private string CreateToken(uint userId)
        {
            var token = GenerateToken();
            return _repository.SaveToken( userId, token );
        }
    }
}