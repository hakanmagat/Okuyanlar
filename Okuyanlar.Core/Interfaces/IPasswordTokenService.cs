namespace Okuyanlar.Web.Services
{
    public interface IPasswordTokenService
    {
        string CreateResetToken(string email);
        bool ValidateResetToken(string email, string token);
        void ConsumeResetToken(string email, string token);

    }

}
