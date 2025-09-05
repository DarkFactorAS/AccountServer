using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using DFCommonLib.Utils;
using DFCommonLib.Logger;
using DFCommonLib.Config;
using DFCommonLib.HttpApi;

using DarkFactor.MailClient;

using AccountCommon.SharedModel;
using AccountServer.Model;
using AccountServer.Repository;

namespace AccountServer.Provider
{
    public interface IAccountProvider
    {
        string PingServer();
        AccountData LoginAccount(LoginData accountData);
        AccountData LoginToken(LoginTokenData accountData);
        AccountData LoginGameCenter(LoginGameCenterData accountData);
        AccountData CreateAccount(CreateAccountData createAccountData);
        ReturnData ResetPasswordWithEmail(string emailAddress);
        ReturnData ResetPasswordWithCode(string emailAddress, string code);
        ReturnData ResetPasswordWithToken(string token, string password );
    }

    public class AccountProvider : IAccountProvider
    {
        IAccountRepository _repository;
        IAccountSessionProvider _session;
        IMailClient _mailClient;
        IDFLogger<AccountProvider> _logger;
        AccountConfig _accountConfig;

        public AccountProvider(IAccountRepository repository,
                                IAccountSessionProvider session,
                                IMailClient mailClient,
                                IConfigurationHelper configurationHelper,
                                IDFLogger<AccountProvider> logger)
        {
            _repository = repository;
            _session = session;
            _mailClient = mailClient;
            _logger = logger;

            _accountConfig = configurationHelper.Settings as AccountConfig;
            if (_accountConfig != null && _accountConfig.mailServer != null)
            {
                _mailClient.SetEndpoint(_accountConfig.mailServer.ServerAddress);
            }
        }

        public string PingServer()
        {
            _logger.LogInfo("PING-PONG");
            return "PONG";
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

            // Nickname and username cannot be equal
            if (createAccountData.nickname == createAccountData.username)
            {
                return AccountData.Error(AccountData.ErrorCode.NicknameAndUsernameEqual);
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
            var internalAccount = _repository.CreateAccount(createAccountData, salt, AccountLoginType.DarkFactor);

            // Create logintoken
            if ( internalAccount != null )
            {
                string token = CreateToken(internalAccount.id );
                return new AccountData( internalAccount.id, internalAccount.nickname, token, internalAccount.flags );
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
                _repository.UpdateLastLogin(accountData.id);
                string token = CreateToken(accountData.id);
                return new AccountData(accountData.id, accountData.nickname, token, accountData.flags);
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
                    _repository.UpdateLastLogin(accountData.id);
                    string token = CreateToken(accountData.id);
                    return new AccountData( accountData.id, accountData.nickname, token, accountData.flags);
                }
            }
            return AccountData.Error( AccountData.ErrorCode.UserDoesNotExist);
        }

        public AccountData LoginGameCenter(LoginGameCenterData loginData)
        {
            if (loginData == null || string.IsNullOrWhiteSpace(loginData.username) || string.IsNullOrWhiteSpace(loginData.nickname) || string.IsNullOrWhiteSpace(loginData.password))
            {
                return AccountData.Error(AccountData.ErrorCode.ErrorInData);
            }

            // If the user already exists, we can log them in
            InternalAccountData accountData = _repository.GetAccountWithUsername(loginData.username);
            if (accountData != null)
            {
                _repository.UpdateLastLogin(accountData.id);
                string token = CreateToken(accountData.id);
                return new AccountData(accountData.id, accountData.nickname, token, accountData.flags);
            }

            // If the user does not exist, we create a new account
            var createAccountData = new CreateAccountData
            {
                nickname = loginData.nickname,
                username = loginData.username,
                password = DFCrypt.DecryptInput(loginData.password),
                email = "",
            };

            var salt = generateSalt();
            createAccountData.password = generateHash(createAccountData.password, salt);
            var internalAccount = _repository.CreateAccount(createAccountData, salt, AccountLoginType.GameCenter);

            // Create logintoken
            if ( internalAccount != null )
            {
                string token = CreateToken(internalAccount.id );
                return new AccountData( internalAccount.id, internalAccount.nickname, token, internalAccount.flags );
            }

            return AccountData.Error(AccountData.ErrorCode.ErrorInData);
        }

        public ReturnData ResetPasswordWithEmail(string emailAddress)
        {
            var vEmail = VerifyEmail(emailAddress);
            if (!string.IsNullOrEmpty(vEmail))
            {
                InternalAccountData accountData = _repository.GetAccountWithEmail(emailAddress);
                if (accountData != null)
                {
                    var twoFactorCode = GenerateCode();
                    var mailServerConfig = _accountConfig.mailServer;

                    _session.SetAccountId(accountData.id);
                    _session.SetAccountCode(twoFactorCode);

                    var content = String.Format(mailServerConfig.EmailBody, twoFactorCode, accountData.username);

                    _logger.LogInfo(content);

                    EmailMessage message = new EmailMessage()
                    {
                        Subject = mailServerConfig.EmailSubject,
                        Content = content
                    };

                    message.AddSender(mailServerConfig.SenderName, mailServerConfig.SenderEmail);
                    message.AddReceiver(accountData.nickname, accountData.email);

                    // THOR: Disabled mail server until we get the mail sending to work
                    //       This means I can only change password by finding code in the logs
                    //var webAPIData = _mailClient.SendEmail(message).Result;
                    //if ( webAPIData.errorCode == WebAPIData.CODE_OK )
                    //{
                        return ReturnData.OKMessage("Blabla");
                    //}
                    //var msg = String.Format("{0}:{1}", webAPIData.errorCode, webAPIData.message);
                    //return new ReturnData(ReturnData.ReturnCode.FailWithMailServer, msg);
                }
                return ReturnData.OKMessage("Unknown user with email" + emailAddress);
            }

            return ReturnData.ErrorMessage(ReturnData.ReturnCode.NotValidEmail);
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
           //Random rnd = new Random();
            //int randomNumber  = rnd.Next(111111, 999999);
            ///return "" + randomNumber;

            // THOR: Disabled 2FA on mail. Just hardcode code for now
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
            if ( !Regex.IsMatch(username, @"^[a-zA-Z0-9]") )
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

            if ( password.Length < 8 )
            {
                return AccountData.ErrorCode.PasswordInvalidLength;
            }

            if ( password.Length > 20 )
            {
                return AccountData.ErrorCode.PasswordInvalidLength;
            }

            // Only A-Z, 0-9, and %*()
            if ( !Regex.IsMatch(password, @"^[a-zA-Z0-9%*()]+$") )
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
            _repository.PurgeOldTokens(userId, _accountConfig.LoginTokenHistoryExpirationDays);
            var token = GenerateToken();
            return _repository.SaveToken( userId, token );
        }
    }
}