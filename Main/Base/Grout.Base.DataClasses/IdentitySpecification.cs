namespace Grout.Base.DataClasses
{
    public class IdentitySpecification
    {
        private int _identityIncrement = 1;
        private int _identitySeed = 1;

        /// <summary>
        ///     Gets or sets the Identity start count
        /// </summary>
        public int IdentitySeed
        {
            get { return _identitySeed; }
            set { _identitySeed = value; }
        }

        /// <summary>
        ///     Gets or sets the Identity Increment count
        /// </summary>
        public int IdentityIncrement
        {
            get { return _identityIncrement; }
            set { _identityIncrement = value; }
        }
    }
}