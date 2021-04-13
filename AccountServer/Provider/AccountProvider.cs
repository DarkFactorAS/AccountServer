using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AccountServer.Model;
using DFCommonLib.Utils;

namespace AccountServer.Repository
{
    public interface IAccountProvider
    {
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
    }

    public class AccountProvider : IAccountProvider
    {
        IAccountRepository _repository;

        public AccountProvider(IAccountRepository repository)
        {
            _repository = repository;
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
            InternalAccountData accountData = _repository.GetAccount(loginData.username);
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
            InternalAccountData accountData = _repository.GetAccount(username);
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

        private string CreateToken(uint userId)
        {
            string token = Guid.NewGuid().ToString();
            return _repository.SaveToken( userId, token );
        }
    }
}