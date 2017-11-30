namespace Grout.Base.Encryption
{
    public interface ITokenCryptography
    {
        string Encrypt(string password, string ip);

        string Decrypt(string encryptedToken);
    }
}