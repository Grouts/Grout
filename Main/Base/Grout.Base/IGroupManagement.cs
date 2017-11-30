using System.Collections.Generic;
using Grout.Base.DataClasses;

namespace Grout.Base
{
    public interface IGroupManagement
    {      
        List<Group> GetAllActiveGroups();

        int? AddGroup(Group group);

        bool UpdateGroup(List<UpdateColumn> updateColumns, int groupId);

        DataResponse IsUserExistInGroup(int userId, int groupId);

        bool AddUserInGroup(int groupId, int userId);

        Group GetGroupByName(string groupName);

        Group GetGroupById(int groupId);

        List<User> GetAllUsersOfGroup(int groupId, List<SortCollection> sorted, int? skip, int? take, string searchKey, List<FilterCollection> filterCollection, out int usersCount);

        bool DeleteGroup(int groupId);

        bool DeleteUserFromGroup(int userId, int groupId);

        DataResponse IsActiveGroup(int groupId);

        DataResponse IsGroupAlreadyExist(string groupName);
    }
}
