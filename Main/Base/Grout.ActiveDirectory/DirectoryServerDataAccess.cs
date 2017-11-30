using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace Syncfusion.Server.ActiveDirectory.Base
{
    public class DirectoryServerDataAccess
    {
        /// <summary>
        ///     Used to Connect the active directory domain
        /// </summary>
        /// <param name="domainObject">Domain properties</param>
        /// <returns>Returns Domain connection</returns>
        public DirectoryEntry DomainConnection(string distinguishedConnectionURL, Domain domainObject)
        {
            var directoryEntry = new DirectoryEntry(distinguishedConnectionURL)
            {
                Username = @domainObject.UserName,
                Password = domainObject.Password
            };

            return directoryEntry;
        }

        /// <summary>
        ///     Used to Connect the active directory objects
        /// </summary>
        /// <param name="distinguishedConnectionURL">Connection path of objects</param>
        /// <param name="domainObject">Returns Object connection</param>
        /// <returns></returns>
        public DirectoryEntry ObjectConnection(string distinguishedConnectionURL, Domain domainObject)
        {
            var directoryEntry = new DirectoryEntry(distinguishedConnectionURL);
            directoryEntry.Username = @domainObject.UserName;
            directoryEntry.Password = domainObject.Password;
            return directoryEntry;
        }

        /// <summary>
        ///     Used to validate the authenticate  connection
        /// </summary>
        /// <param name="directoryEntry">Connection</param>
        /// <returns>If connections is authenticate it returns true else false</returns>
        public bool ValidateCreditionals(DirectoryEntry directoryEntry)
        {
            using (directoryEntry)
            {
                try
                {
                    var tmp = directoryEntry.NativeObject;
                    return true;
                }
                catch (COMException ex)
                {
                    return false;
                }
            }
        }
    }
}