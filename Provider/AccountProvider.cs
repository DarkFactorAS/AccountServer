using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AccountServer.Model;

namespace AccountServer.Repository
{
    public interface IAccountProvider
    {
        AccountData LoginAccount(LoginData accountData);
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
            if ( string.IsNullOrEmpty( createAccountData.nickname ) 
                || string.IsNullOrEmpty( createAccountData.username )  
                || string.IsNullOrEmpty( createAccountData.password ) ) 
            {
                return AccountData.Error(AccountData.ErrorCode.ErrorInData);
            }

            InternalAccountData accountData = _repository.GetAccount(createAccountData.username);
            if ( accountData != null )
            {
                return AccountData.Error(AccountData.ErrorCode.UserAlreadyExist);
            }

            var salt = generateSalt();
            createAccountData.password = generateHash(createAccountData.password, salt);

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

            string encryptedPassword = generateHash(loginData.password, accountData.salt);
            if ( accountData != null && accountData.password == encryptedPassword )
            {
                string token = CreateToken(accountData.id);
                return new AccountData( accountData.nickname, token );
            }
            return AccountData.Error( AccountData.ErrorCode.UserDoesNotExist);
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