using Okuyanlar.Core.Enums;
using Okuyanlar.Core.Extensions;

namespace Okuyanlar.Tests
{
    public class UserRoleExtensionsTests
    {
        [Theory]
        [InlineData(UserRole.SystemAdmin, "System Admin")]
        [InlineData(UserRole.Admin, "Admin")]
        [InlineData(UserRole.Librarian, "Librarian")]
        [InlineData(UserRole.EndUser, "Reader")]
        public void GetDisplayName_ReturnsExpected(UserRole role, string expected)
        {
            var name = role.GetDisplayName();
            Assert.Equal(expected, name);
        }

        [Fact]
        public void GetDisplayName_FallbacksToToString_ForUnknown()
        {
            var role = (UserRole)999;
            var name = role.GetDisplayName();
            Assert.Equal("999", name);
        }
    }
}
