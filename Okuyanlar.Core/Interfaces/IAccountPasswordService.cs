namespace Okuyanlar.Core.Interfaces
{
    public interface IAccountPasswordService
    {
        // email ile kullanýcý var mý? (username lazým)
        (bool exists, string username) FindUserByEmail(string email);

        // þifreyi güncelle (hash dahil) -> sende nasýl yapýlýyorsa
        void UpdatePasswordByEmail(string email, string newPassword);
    }
}
