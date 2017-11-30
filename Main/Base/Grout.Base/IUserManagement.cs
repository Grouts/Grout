using System;
using System.Collections.Generic;
using Grout.Base.DataClasses;

namespace Grout.Base
{
    public interface IUserManagement
    {
        Result AddUser(User user, out DateTime activationExpirationDate, out string activationCode);

        bool AddUserToken(int userId, string ipAddress, string token);        

        bool AddUserLog(int logType, int currentUser, int? groupId, int userId,
            int oldValue = 0, int newValue = 0);

        bool AddSystemUserLog(int logType, int currentUser, int? userId);

        List<User> SearchUsers(int? skip, int? take = 10, string searchkey = "");

        Result UserLogView(List<ConditionColumn> whereConditionColumns);

        Result SystemLogView(List<ConditionColumn> whereConditionColumns);

        List<string> ValidateEmailList(List<string> emailList);

        List<string> ValidateUserNameList(List<string> userNameList);

        User FindUserByEmail(string email);

        User FindUserByUserName(string username);
        
        User FindUserByUserId(int userId);

        User FindUserByActivationCode(string activationCode);

        DataResponse IsExistingUser(string username);
        
        string GenerateRandomCode(int length);

        bool DeleteUser(int userId);

        bool UpdateUserProfileDetails(List<UpdateColumn> updateColumns, int userId);

        List<User> GetUsers(List<ConditionColumn> whereConditionColumns);

        List<User> GetAllUsers();

        List<User> GetAllActiveInactiveUsers();

        bool IsValidEmail(string emailAddress);

        string GetUserPreferentSort(int userId);

        string GetUserPreferenceFilters(int userId);

        bool UpdateUserPreferenceDetails(List<UpdateColumn> updateColumns, int userId);

        string GetUserPreferTimeZone(int userId);

        bool ActivateUser(int userId, string password);

        int GetRecordSizeFromUserPreference(int userId);

        int GetUserId(string userName);

        string GetUserName(int userId);

        DataResponse IsActiveUser(int userId);

        DataResponse IsDeletedUser(int userId);

        bool UpdateLoginTime(int userId, DateTime loginTime);        

        List<UserGroup> GetAllGroupsOfUser(int userId);

        string GetUserPassword(int userId);

        string GetUserPassword(string userName);

        bool UpdateUserSortPreference(string sortXml, int userId);

        DataResponse SetResetPasswordForUser(int userId);



    }
}
