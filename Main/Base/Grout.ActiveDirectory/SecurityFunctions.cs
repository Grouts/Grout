using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Syncfusion.Server.ActiveDirectory.Base
{
    public class SecurityFunctions
    {
        /// <summary>
        /// Used to find All objects list in active directory
        /// </summary>
        /// <param name="directoryEntry">Domain connection</param>
        /// <returns>Returns active directory object list</returns>
        public List<string> FindAllObjects(DirectoryEntry directoryEntry)
        {
            var directorySearcher = new DirectorySearcher(directoryEntry);
            return (from SearchResult result in directorySearcher.FindAll() select result.GetDirectoryEntry().Name).ToList();
        }

        /// <summary>
        /// Used to find All users list in active directory
        /// </summary>
        /// <param name="directoryEntry">Domain connection</param>
        /// <returns>Returns active directory users list</returns>
        public List<Users> FindAllUsers(DirectoryEntry directoryEntry, List<string> activeUsersList)
        {
            var directorySearcher = new DirectorySearcher(directoryEntry);
            directorySearcher.Filter = "(&(objectCategory=organizationalPerson)(objectClass=User))";
            var userList = new List<Users>();
            var users = directorySearcher.FindAll();
            foreach (SearchResult user in users)
            {
                DirectoryEntry DE = user.GetDirectoryEntry();
                try
                {
                    if (!Convert.ToBoolean((int)DE.Properties["userAccountControl"].Value & 0x0002))
                    {
                        if (!activeUsersList.Contains(DE.Properties["SamAccountName"].Value.ToString().ToLower()))
                        {
                            userList.Add(new Users
                            {
                                UserId = Guid.Parse(DE.Guid.ToString()),
                                UserName = DE.Properties["SamAccountName"].Value.ToString(),
                                FullName = DE.Properties["name"].Value.ToString(),
                                LastName =
                                    (DE.Properties["sn"].Value != null
                                        ? DE.Properties["sn"].Value.ToString()
                                        : String.Empty),
                                FirstName =
                                    (DE.Properties["givenName"].Value != null
                                        ? DE.Properties["givenName"].Value.ToString()
                                        : String.Empty),
                                EmailId =
                                    (DE.Properties["mail"].Value != null
                                        ? DE.Properties["mail"].Value.ToString()
                                        : String.Empty),
                                ContactNumber =
                                    (DE.Properties["mobile"].Value != null
                                        ? DE.Properties["mobile"].Value.ToString()
                                        : String.Empty)
                            });
                        }
                    }
                }
                catch
                {
                }
            }
            return userList;
        }


        /// <summary>
        /// Used to find All groups list in active directory
        /// </summary>
        /// <param name="directoryEntry">Domain connection</param>
        /// <returns>Returns active directory groups list</returns>
        public List<Groups> FindAllGroups(string ldapurl, Domain domainObject, List<string> activeGroupsList)
        {
            var directoryServer = new DirectoryServerDataAccess();
            var directoryEntry = directoryServer.DomainConnection(ldapurl, domainObject);
            var directorySearcher = new DirectorySearcher(directoryEntry);
            directorySearcher.Filter = "(objectClass=group)";
            var groupList = new List<Groups>();
            var groups = directorySearcher.FindAll();
            foreach (SearchResult group in groups)
            {
                DirectoryEntry DE = group.GetDirectoryEntry();
                try
                {
                    if (!activeGroupsList.Contains(DE.Properties["name"].Value.ToString().ToLower()))
                    {
                        groupList.Add(new Groups
                        {
                            GroupId = Guid.Parse(DE.Guid.ToString()),
                            GroupName = DE.Properties["name"].Value.ToString(),
                            GroupDescription =
                                DE.Properties["description"].Value != null
                                    ? DE.Properties["description"].Value.ToString()
                                    : String.Empty
                        });
                    }

                }
                catch
                {
                }
            }
            return groupList;
        }


        public List<object> FindAllGroupsDescription(DirectoryEntry directoryEntry)
        {
            var directorySearcher = new DirectorySearcher(directoryEntry);
            directorySearcher.Filter = "(ObjectCategory=group)";
            var result = directorySearcher.FindAll();
            var _list = string.Empty;
            var  reportList = new Dictionary<string, string>();
            var groupDetails = new List<object>();
            for (var i = 0; i < result.Count; i++)
            {
               
                if (result[i] != null)
                {
                    var fields = result[i].Properties;
                    foreach (String ldapField in fields.PropertyNames)
                    {
                        if (ldapField == "name" || ldapField == "description")
                        {
                            foreach (var myCollection in fields[ldapField])
                            {
                                _list = myCollection.ToString();
                            }
                            reportList.Add(ldapField, _list);
                        }
                    }
                    groupDetails.Add(reportList);
                    reportList=new Dictionary<string,string>();
                }
                else
                {
                    //Console.WriteLine("Object not found!");
                }
            }
            return groupDetails;
        }

        public Groups FindSpecificGroupDetailsbyGuid(Guid groupId, Domain domainObj, string ldapUrl)
        {
            var group = new Groups();
            try
            {
                var directoryEntry = new DirectoryEntry(ldapUrl + "/<GUID=" + groupId + ">", domainObj.UserName, domainObj.Password);
                var directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(ObjectCategory=group)";
                var path = directoryEntry.Properties.PropertyNames;
                
                try
                {
                    var result = directorySearcher.FindOne();
                    DirectoryEntry DE = result.GetDirectoryEntry();
                    group = new Groups
                    {
                        GroupId = Guid.Parse(DE.Guid.ToString()),
                        GroupName = DE.Properties["name"].Value.ToString(),
                        GroupDescription =
                            DE.Properties["description"].Value != null
                                ? DE.Properties["description"].Value.ToString()
                                : String.Empty,
                        Users = FindMembersInGroup(DE.Properties["name"].Value.ToString(), ldapUrl, domainObj)
                    };
                }
                catch
                {
                    return group;
                }
            }
            catch (Exception e)
            {
                group.GroupId = groupId;
                return group;
            }
            

            return group;
        }


        public Dictionary<string, string> FindSpecificGroupDetails(DirectoryEntry directoryEntry,string groupname)
        {
            var directorySearcher = new DirectorySearcher(directoryEntry);
            directorySearcher.Filter = "(ObjectCategory=group)";
            directorySearcher.Filter = "(name=" + groupname + ")";
            var result = directorySearcher.FindOne();
            var _list = string.Empty;
            var reportList = new Dictionary<string, string>();
                if (result!= null)
                {
                    var fields = result.Properties;
                    foreach (String ldapField in fields.PropertyNames)
                    {
                        if (ldapField == "name" || ldapField == "description")
                        {
                            foreach (var myCollection in fields[ldapField])
                            {
                                _list = myCollection.ToString();
                            }
                            reportList.Add(ldapField, _list);
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("Object not found!");
                }

                return reportList;
        }

        
        /// <summary>
        /// Used to display the reports for specific objects 
        /// </summary>
        /// <param name="directoryEntry">Domain connection</param>
        /// <param name="objects">Object properties</param>
        /// <returns>Returns specific object report</returns>

        public Dictionary<string, string> FindReportByFullName(DirectoryEntry directoryEntry, Objects objects)
        {
            var reportList = new Dictionary<string, string>();
            var directorySearcher = new DirectorySearcher(directoryEntry);

            //directorySearcher.Filter = "(cn=" + "narendrans@testlabkaruna.com" + ")";
            directorySearcher.Filter = "(cn=" + objects.ObjectName + ")";
            var result = directorySearcher.FindOne();

            var _list = string.Empty;

            if (result != null)
            {
                var fields = result.Properties;
                foreach (String ldapField in fields.PropertyNames)
                {
                    if (ldapField == "samaccountname" || ldapField == "mail" || ldapField == "cn")
                    {
                        foreach (var myCollection in fields[ldapField])
                        {
                            _list = myCollection.ToString();
                        }
                        reportList.Add(ldapField, _list);
                    }
                }
            }
            else
            {
                //Console.WriteLine("Object not found!");
            }
            return reportList;
        }
        public Dictionary<string, string> FindReport(DirectoryEntry directoryEntry, Objects objects)
        {
            var reportList = new Dictionary<string, string>();
            var directorySearcher = new DirectorySearcher(directoryEntry);

            //directorySearcher.Filter = "(cn=" + "narendrans@testlabkaruna.com" + ")";
            directorySearcher.Filter = "(samaccountname=" + objects.ObjectName + ")";
            var result = directorySearcher.FindOne();

            var _list = string.Empty;

            if (result != null)
            {
                var fields = result.Properties;
                foreach (String ldapField in fields.PropertyNames)
                {
                    foreach (var myCollection in fields[ldapField])
                    {
                        _list = myCollection.ToString();
                    }
                    reportList.Add(ldapField, _list);
                }
            }
            else
            {
                //Console.WriteLine("Object not found!");
            }
            return reportList;
        }



        /// <summary>
        /// Used to create a new user
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        public void CreateUser(DirectoryEntry dataAccess, Users userObject)
        {
            try
            {

                var newUser = dataAccess.Children.Add
                    ("CN=" + userObject.UserName, "user");
                newUser.Properties["samAccountName"].Value = userObject.UserName;
                newUser.Properties["mail"].Value = "syncfusion@gmail.com";
                newUser.Properties["sn"].Value = "hi";
                newUser.Properties["sn"].Value = userObject.LastName;
                //newUser.Properties["userprincipalname"].Value =userObject.UserName+userObject.LastName;
                newUser.Properties["userprincipalname"].Value = "abc@testlabkaruna.com";

                newUser.CommitChanges();
                newUser.Invoke("SetPassword", new object[] { userObject.Password });

                newUser.CommitChanges();
                //Console.WriteLine("User created success");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message: Password mismatch the password policy ");
            }
        }

        /// <summary>
        /// Used to create a new group
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="groupObject">Group properties</param>
        public void CreateGroup(DirectoryEntry dataAccess, Groups groupObject)
        {
            try
            {
                var newGroup = dataAccess.Children.Add("CN=" + groupObject.GroupName, "group");
                newGroup.Properties["sAmAccountName"].Value = groupObject.GroupName;
                newGroup.CommitChanges();
                //  Console.WriteLine("Group created success");
            }
            catch (Exception ex)
            {
                //  Console.WriteLine("Message:" + ex);
            }
        }
        /// <summary>
        /// User to add a specific member to specific group
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        /// <param name="objects">Object properties</param>
        /// <param name="domainObject">Domain properties</param>
        /// <param name="_distinguishedConnectionURL">Connection url for specific group</param>
        public void AddMembertoGroup(DirectoryEntry dataAccess, Users userObject, Objects objects, Domain domainObject, string _distinguishedConnectionURL)
        {

            try
            {
                var securityFunction = new SecurityFunctions();
                var dataAccessObject = new DirectoryServerDataAccess();

                var dirEntry = dataAccessObject.ObjectConnection(_distinguishedConnectionURL, domainObject);
                var userDn = securityFunction.DistinguishedName(dataAccess, objects);
                dirEntry.Properties["member"].Add(userDn);
                //Console.WriteLine("You successfully add member to group");
                dirEntry.CommitChanges();
                dirEntry.Close();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message:" + ex);
            }
        }
        /// <summary>
        /// User to remove a specific member from specific group
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        /// <param name="objects">Object properties</param>
        /// <param name="domainObject">Domain properties</param>
        /// <param name="_distinguishedConnectionURL">Connection url for specific group</param>
        public void RemoveMemberfromGroup(DirectoryEntry dataAccess, Users userObject, Objects objects, Domain domainObject, string _distinguishedConnectionURL)
        {

            try
            {

                var securityFunction = new SecurityFunctions();
                var dataAccessObject = new DirectoryServerDataAccess();

                var dirEntry = dataAccessObject.ObjectConnection(_distinguishedConnectionURL, domainObject);
                var userDn = securityFunction.DistinguishedName(dataAccess, objects);
                dirEntry.Properties["member"].Remove(userDn);
                //Console.WriteLine("You successfully add member to group");
                dirEntry.CommitChanges();
                dirEntry.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message:" + ex);
            }
        }
        /// <summary>
        /// Used to find the connection url for specific objects
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="objects">Object properties</param>
        /// <returns>Returns connection url</returns>
        public string DistinguishedConnectionURL(DirectoryEntry dataAccess, Objects objects)
        {
            var _distinguishedConnectionURL = string.Empty;
            var mySearcher = new DirectorySearcher(dataAccess)
            {
                Filter =
                    "(&(objectClass=" + objects.ObjectType + ")(|(cn=" + objects.ObjectName + ")(sAMAccountName=" +
                    objects.ObjectName + ")))"
            };

            var result = mySearcher.FindOne();
            if (result == null)
            {
                Console.WriteLine
                  ("unable to locate the distinguishedName for the object " +
                  objects.ObjectName + " in the  domain");
            }
            else
            {
                var directoryObject = result.GetDirectoryEntry();

                _distinguishedConnectionURL = "LDAP://192.168.1.14/" + directoryObject.Properties
                    ["distinguishedName"].Value;

                dataAccess.Close();
                dataAccess.Dispose();
                mySearcher.Dispose();
                return _distinguishedConnectionURL;
            }
            return _distinguishedConnectionURL;
        }
        /// <summary>
        /// Used to find the DistinguishedName for specific objects
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="objects">Object properties</param>
        /// <returns>Returns DistinguishedName</returns>
        public string DistinguishedName(DirectoryEntry dataAccess, Objects objects)
        {

            var _distinguishedName = string.Empty;
            var mySearcher = new DirectorySearcher(dataAccess)
            {
                Filter =
                    "(&(objectClass=" + objects.ObjectType + ")(|(cn=" + objects.ObjectName + ")(sAMAccountName=" +
                    objects.ObjectName + ")))"
            };

            var result = mySearcher.FindOne();
            if (result == null)
            {
                Console.WriteLine
                ("unable to locate the distinguishedName for the object " +
                objects.ObjectName + " in the  domain");

            }
            else
            {
                var directoryObject = result.GetDirectoryEntry();
                _distinguishedName = directoryObject.Properties["distinguishedName"].Value.ToString();
                dataAccess.Close();
                dataAccess.Dispose();
                mySearcher.Dispose();
            }
            return _distinguishedName;
        }

        /// <summary>
        /// Used to delete a specific objects
        /// </summary>
        /// <param name="domainObject">Domain properties</param>
        /// <param name="objects">Object properties</param>
        /// <param name="distinguishedConnectionURL">Connection url for specific object</param>
        public void DeleteObjects(Domain domainObject, Objects objects, string distinguishedConnectionURL)
        {
            try
            {

                var dataAccessObject = new DirectoryServerDataAccess();
                var _domainConnection = dataAccessObject.DomainConnection(distinguishedConnectionURL, domainObject);
                var dirEntry = dataAccessObject.ObjectConnection(distinguishedConnectionURL, domainObject);

                _domainConnection.Children.Remove(dirEntry);
                //Console.WriteLine("user deleted successfully");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message:" + ex);
            }
        }

        /// <summary>
        /// Used to update a specific object properties
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="objects">Object properties</param>
        public void UpdateObjectProperties(DirectoryEntry dataAccess, Objects objects)
        {
            try
            {
                var mySearcher = new DirectorySearcher(dataAccess);
                mySearcher.Filter = "(cn=" + objects.ObjectName + ")";

                mySearcher.PropertiesToLoad.Add("" + objects.ObjectProperties + "");
                var result = mySearcher.FindOne();
                if (result != null)
                {
                    var entryToUpdate = result.GetDirectoryEntry();
                    if (!(String.IsNullOrEmpty(objects.ObjectPropertyValue)))
                    {
                        if (result.Properties.Contains("" + objects.ObjectProperties + ""))
                        {
                            entryToUpdate.Properties["" + objects.ObjectProperties + ""].Value = objects.ObjectPropertyValue;
                            // Console.WriteLine("property add or update success");
                        }
                        else
                        {
                            entryToUpdate.Properties["" + objects.ObjectProperties + ""].Add(objects.ObjectPropertyValue);
                            //Console.WriteLine("property add or update success");
                        }
                        entryToUpdate.CommitChanges();

                    }
                }


                dataAccess.Close();
                dataAccess.Dispose();
                dataAccess.Dispose();

            }
            catch (Exception e)
            {
                // Console.WriteLine("Error: " + e.Message);
            }
        }
        /// <summary>
        /// Used to Check specific Object found or not
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="objects">Object properties</param>
        /// <returns>If object is found it returns true else returns false</returns>
        public bool CheckObjectFound(DirectoryEntry dataAccess, Objects objects)
        {
            var mySearcher = new DirectorySearcher(dataAccess);
            mySearcher.Filter = "(&((cn=" + objects.ObjectName + ")(objectClass=" + objects.ObjectType + ")))";
            var result = mySearcher.FindOne();

            return result != null;
        }
        /// <summary>
        /// Used to change user passwords
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        /// <param name="domainObject">Domain properties</param>
        /// <param name="objects">Object properties</param>
        public void PasswordChange(DirectoryEntry dataAccess, Users userObject, Domain domainObject, Objects objects)
        {
            try
            {
                var function_object = new SecurityFunctions();
                var _distinguishedConnectionURL = function_object.DistinguishedConnectionURL(dataAccess, objects);
                var dataAccessObject = new DirectoryServerDataAccess();

                var dirEntry = dataAccessObject.ObjectConnection(_distinguishedConnectionURL, domainObject);
                dirEntry.Invoke("SetPassword", new object[] { userObject.Password });
                dirEntry.Properties["LockOutTime"].Value = 0; //unlock account
                dirEntry.Close();
                //Console.WriteLine("Reset password success");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message:" + ex);
            }

        }
        /// <summary>
        /// Used to Add the specific object attributs
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="objects">Object properties</param>
        public void AddobjectsProperties(DirectoryEntry dataAccess, Objects objects)
        {
            try
            {
                var mySearcher = new DirectorySearcher(dataAccess);
                mySearcher.Filter = "(cn=" + objects.ObjectName + ")";
                //mySearcher.Filter = "(&((cn=" + objects.ObjectName + ")(objectClass=" + objects.ObjectType + ")))";

                mySearcher.PropertiesToLoad.Add("" + objects.ObjectProperties + "");
                var result = mySearcher.FindOne();
                if (result != null)
                {
                    var entryToUpdate = result.GetDirectoryEntry();
                    if (!(String.IsNullOrEmpty(objects.ObjectPropertyValue)))
                    {
                        if (result.Properties.Contains("" + objects.ObjectProperties + ""))
                        {
                            entryToUpdate.Properties["" + objects.ObjectProperties + ""].Value = objects.ObjectPropertyValue;
                            // Console.WriteLine("property add or update success");
                        }
                        else
                        {
                            entryToUpdate.Properties["" + objects.ObjectProperties + ""].Add(objects.ObjectPropertyValue);
                            //Console.WriteLine("property add or update success");
                        }
                        entryToUpdate.CommitChanges();
                    }
                }
                dataAccess.Close();
                dataAccess.Dispose();
                dataAccess.Dispose();

            }
            catch (Exception e)
            {
                //Console.WriteLine("Error: " + e.Message);
            }
        }
        /// <summary>
        /// Used to enable the specific user account
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        /// <param name="objects">Object properties</param>
        /// <param name="domainObject">Domain properties</param>
        public void EnableUserAccount(DirectoryEntry dataAccess, Users userObject, Objects objects, Domain domainObject)
        {
            try
            {
                var function_object = new SecurityFunctions();
                var _distinguishedConnectionURL = function_object.DistinguishedConnectionURL(dataAccess, objects);
                var data_access_object = new DirectoryServerDataAccess();

                var dirEntry = data_access_object.ObjectConnection(_distinguishedConnectionURL, domainObject);
                var val = (int)dirEntry.Properties["userAccountControl"].Value;
                dirEntry.Properties["userAccountControl"].Value = val & ~0x2;
                dirEntry.CommitChanges();
                dirEntry.Close();
                //Console.WriteLine("Enable success");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message:" + ex);
            }

        }
        /// <summary>
        /// Used to disable the specific user account
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        /// <param name="objects">Object properties</param>
        /// <param name="domainObject">Domain properties</param>
        public void DisableUserAccount(DirectoryEntry dataAccess, Users userObject, Objects objects, Domain domainObject)
        {
            try
            {
                var function_object = new SecurityFunctions();
                var _distinguishedConnectionURL = function_object.DistinguishedConnectionURL(dataAccess, objects);
                var data_access_object = new DirectoryServerDataAccess();

                var dirEntry = data_access_object.ObjectConnection(_distinguishedConnectionURL, domainObject);
                var val = (int)dirEntry.Properties["userAccountControl"].Value;
                dirEntry.Properties["userAccountControl"].Value = val | 0x2; ;
                dirEntry.CommitChanges();
                dirEntry.Close();
                //Console.WriteLine("Disable success");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message:" + ex);
            }

        }
        /// <summary>
        /// Used to Find the members in a specific group
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="objects">Object properties</param>
        /// <returns>Returns group members list</returns>
        public List<Users> FindMembersInGroup(string groupName, string domain, Domain domainObject)
        {
            
            var userList = new List<Users>();

            List<string> retVal = new List<string>();
            DirectoryEntry dataAccess = new DirectoryEntry(domain, domainObject.UserName, domainObject.Password);
            DirectorySearcher searcher = new DirectorySearcher("(&(objectCategory=group)(cn=" + groupName + "))");
            searcher.SearchRoot = dataAccess;
            searcher.SearchScope = SearchScope.Subtree;
            SearchResult result = searcher.FindOne();
            foreach (string member in result.Properties["member"])
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(String.Concat(domain, "/", member.ToString()), domainObject.UserName, domainObject.Password);
                if (directoryEntry.Properties["objectClass"].Contains("user") && directoryEntry.Properties["cn"].Count > 0)
                {
                    if (!Convert.ToBoolean((int)directoryEntry.Properties["userAccountControl"].Value & 0x0002))
                    {
                        userList.Add(new Users
                        {
                            UserId = Guid.Parse(directoryEntry.Guid.ToString()),
                            UserName = directoryEntry.Properties["samaccountname"][0].ToString(),
                            FullName = directoryEntry.Properties["name"][0].ToString(),
                            LastName = (directoryEntry.Properties["sn"].Value != null ? directoryEntry.Properties["sn"][0].ToString() : String.Empty),
                            FirstName = (directoryEntry.Properties["givenName"].Value != null ? directoryEntry.Properties["givenName"][0].ToString() : String.Empty),
                            EmailId = (directoryEntry.Properties["mail"].Value != null ? directoryEntry.Properties["mail"][0].ToString() : String.Empty),
                            ContactNumber = (directoryEntry.Properties["mobile"].Value != null ? directoryEntry.Properties["mobile"][0].ToString() : String.Empty)
                        });
                    }
                }
            }

            return userList;

        }
        /// <summary>
        /// Used to unlock the specific user account
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        /// <param name="objects">Object properties</param>
        /// <param name="domainObject">Domain properties</param>
        public void UnlockUserAccount(DirectoryEntry dataAccess, Users userObject, Objects objects, Domain domainObject)
        {
            try
            {
                var function_object = new SecurityFunctions();
                var _distinguishedConnectionURL = function_object.DistinguishedConnectionURL(dataAccess, objects);
                var data_access_object = new DirectoryServerDataAccess();

                var directoryEntry = data_access_object.ObjectConnection(_distinguishedConnectionURL, domainObject);
                directoryEntry.Properties["lockoutTime"].Value = 0;
                directoryEntry.CommitChanges();
                //Console.WriteLine("UnLock success");

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Message:" + ex);
            }

        }
        /// <summary>
        /// Used to login specific user and also check login creditionals
        /// </summary>
        /// <param name="dataAccess">Domain connection</param>
        /// <param name="userObject">User properties</param>
        public bool UserLogin(string path, Domain domainObject, string userName, string password)
        {
            var uri = new Uri(path);
            var host = uri != null ? uri.Host : String.Empty;

            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, host, domainObject.UserName, domainObject.Password))
            {
                // validate the credentials
                bool isValid = pc.ValidateCredentials(userName, password);
                return isValid;
            }
        }
    }

}


