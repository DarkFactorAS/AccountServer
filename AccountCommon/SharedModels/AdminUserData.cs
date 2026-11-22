using System;
using System.Collections.Generic;

namespace AccountCommon.SharedModel
{
    /// <summary>One account as seen by admin tools (AccountWeb). Never contains password or salt.</summary>
    public class AdminUserData
    {
        public uint id { get; set; }
        public string username { get; set; } = string.Empty;
        public string nickname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public uint flags { get; set; }
        public int logintype { get; set; }
        public DateTime created { get; set; }
        public DateTime lastlogin { get; set; }
        public int numlogins { get; set; }

        public AccountData.ErrorCode errorCode { get; set; }
        public string? errorMessage { get; set; }

        public static AdminUserData Error(AccountData.ErrorCode errorCode) =>
            new AdminUserData { errorCode = errorCode, errorMessage = AccountData.Error(errorCode).errorMessage };
    }

    public class AdminUserListData
    {
        public List<AdminUserData> users { get; set; } = new List<AdminUserData>();

        public AccountData.ErrorCode errorCode { get; set; }
        public string? errorMessage { get; set; }

        public static AdminUserListData Error(AccountData.ErrorCode errorCode) =>
            new AdminUserListData { errorCode = errorCode, errorMessage = AccountData.Error(errorCode).errorMessage };
    }

    public class AdminSearchUsersData
    {
        /// <summary>Matches username, nickname or email (substring). Empty lists the newest accounts.</summary>
        public string query { get; set; } = string.Empty;
    }

    public class AdminGetUserData
    {
        public uint id { get; set; }
    }

    public class AdminUpdateUserData
    {
        public uint id { get; set; }
        public string nickname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class AdminSetPasswordData
    {
        public uint id { get; set; }

        /// <summary>New password, encrypted with DFCrypt.EncryptInput (same as CreateAccount).</summary>
        public string password { get; set; } = string.Empty;
    }
}
