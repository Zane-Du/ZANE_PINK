using Xunit;
using ZANE.Models.Auth;
using ZANE.Services;

namespace ZANE.Tests;

public class AuthModelTests
{
    [Fact]
    public void RoleModel_DefaultLevel_IsOne()
    {
        var role = new RoleModel();
        Assert.Equal((ulong)1, role.Level);
    }

    [Fact]
    public void RoleModel_Constructor_SetsLevelAndName()
    {
        var role = new RoleModel(1 << 5, "测试角色");
        Assert.Equal((ulong)1 << 5, role.Level);
        Assert.Equal("测试角色", role.Name);
    }

    [Fact]
    public void PermissionItem_HasPermission_ReturnsTrueWhenBitSet()
    {
        var role = new RoleModel(1 << 3, "设备");
        var item = new PermissionItem
        {
            PropertyName = "TestProp",
            EditLevel = (ulong)1 << 3,
        };
        Assert.True(item.HasPermission(role));
    }

    [Fact]
    public void PermissionItem_HasPermission_ReturnsFalseWhenBitNotSet()
    {
        var role = new RoleModel(1 << 2, "工艺");
        var item = new PermissionItem
        {
            PropertyName = "TestProp",
            EditLevel = (ulong)1 << 3,
        };
        Assert.False(item.HasPermission(role));
    }

    [Fact]
    public void PermissionItem_SetPermission_GrantsAndRevokes()
    {
        var role = new RoleModel(1 << 2, "工艺");
        var item = new PermissionItem { PropertyName = "Test" };

        // Grant
        item.SetPermission(role, true);
        Assert.True(item.HasPermission(role));

        // Revoke
        item.SetPermission(role, false);
        Assert.False(item.HasPermission(role));
    }

    [Fact]
    public void UserModel_DefaultValues()
    {
        var user = new UserModel();
        Assert.Empty(user.Account);
        Assert.Empty(user.Name);
        Assert.Empty(user.Password);
    }

    [Fact]
    public void RoleConfigModel_Default_HasFourRoles()
    {
        var config = new RoleConfigModel();
        Assert.Equal(4, config.Roles.Count);
        Assert.Contains(config.Roles, r => r.Name == "管理员");
        Assert.Contains(config.Roles, r => r.Name == "工艺");
    }
}

public class AuthServiceTests
{
    [Fact]
    public void AuthService_LoadUsers_ReturnsDefaultUsers()
    {
        var service = new AuthService();
        var users = service.LoadUsers();
        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Account == "admin");
    }

    [Fact]
    public void AuthService_Login_ValidCredentials_Succeeds()
    {
        var service = new AuthService();
        var result = service.Login("admin", "admin");
        Assert.True(result);
        Assert.True(service.IsLoggedIn);
        Assert.NotNull(service.CurrentUser);
        Assert.Equal("管理员", service.CurrentUser.Name);
    }

    [Fact]
    public void AuthService_Login_InvalidCredentials_Fails()
    {
        var service = new AuthService();
        var result = service.Login("admin", "wrongpass");
        Assert.False(result);
        Assert.False(service.IsLoggedIn);
    }

    [Fact]
    public void AuthService_Logout_ClearsUser()
    {
        var service = new AuthService();
        service.Login("admin", "admin");
        Assert.True(service.IsLoggedIn);

        service.Logout();
        Assert.False(service.IsLoggedIn);
        Assert.Null(service.CurrentUser);
    }

    [Fact]
    public void AuthService_Login_SetsCurrentRole()
    {
        var service = new AuthService();
        service.Login("admin", "admin");
        Assert.NotNull(service.CurrentRole);
        Assert.Equal("管理员", service.CurrentRole.Name);
    }

    [Fact]
    public void AuthService_AddAndDeleteUser()
    {
        var service = new AuthService();
        var user = new UserModel
        {
            Account = "testuser_" + Guid.NewGuid().ToString("N")[..6],
            Name = "测试用户",
            Password = "pass123",
            RoleName = "工艺",
        };

        service.AddUser(user);
        var users = service.LoadUsers();
        Assert.Contains(users, u => u.Account == user.Account);

        service.DeleteUser(user.Account);
        users = service.LoadUsers();
        Assert.DoesNotContain(users, u => u.Account == user.Account);
    }
}

public class RoleConfigServiceTests
{
    [Fact]
    public void RoleConfigService_Load_ReturnsDefaultConfig()
    {
        var service = new RoleConfigService();
        var config = service.Load();
        Assert.NotNull(config);
        Assert.NotEmpty(config.Roles);
        Assert.NotEmpty(config.PermissionItems);
    }

    [Fact]
    public void RoleConfigService_DefaultPermissions_HaveCategories()
    {
        var service = new RoleConfigService();
        var config = service.Load();

        Assert.Contains(config.PermissionItems, p => p.Category == "Device");
        Assert.Contains(config.PermissionItems, p => p.Category == "Run");
        Assert.Contains(config.PermissionItems, p => p.Category == "Function");
        Assert.Contains(config.PermissionItems, p => p.Category == "Advanced");
    }
}

public class UserModelExtendedTests
{
    [Fact]
    public void UserModel_HasNewFields()
    {
        var user = new UserModel
        {
            Account = "test",
            Name = "测试",
            Password = "123",
            RoleName = "工艺",
            Tel = "13800138000",
        };
        Assert.Equal("13800138000", user.Tel);
        Assert.NotEqual(default, user.RegisterTime);
    }

    [Fact]
    public void UserModel_UpdateTime_IsSetOnCreation()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var user = new UserModel { Account = "u1", Name = "User1", Password = "p1" };
        Assert.True(user.RegisterTime >= before);
    }
}

