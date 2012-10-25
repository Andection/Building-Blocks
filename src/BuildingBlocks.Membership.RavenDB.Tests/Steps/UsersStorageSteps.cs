using System;
using System.Linq;
using BuildingBlocks.Membership.RavenDB.DomainModel;
using BuildingBlocks.Store.RavenDB;
using BuildingBlocks.Store.RavenDB.TestHelpers.SpecFlow;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace BuildingBlocks.Membership.RavenDB.Tests.Steps
{
    [Binding]
    public class UsersStorageSteps
    {
        [Given(@"���������� ������������")]
        public void ����������������������(Table table)
        {
            foreach (var row in table.Rows)
            {
                var username = row["���"];
                var email = row.ContainsKey("email") ? row["email"] : username + "@mail.com";
                var user = new UserEntity
                    {
                        ApplicationName = MembershipSettings.ApplicationName,
                        UserId = Guid.NewGuid(),
                        Username = username,
                        Email = email, 
                        Password = "123"
                    };
                RavenDb.CurrentSession.Store(user);
            }
        }

        [Given(@"������ ���������� �������������")]
        public void �������������������������������������(Table table)
        {
            var session = RavenDb.CurrentStorageSession;
            foreach (var row in table.CreateDynamicSet())
            {
                string username = row.���;
                var user = session.Query<UserEntity>().Single(u => u.Username == username);
                user.LastActivityDate = (DateTime)row.����������;
                session.Save(user);
            }
        }

        [Given(@"������������ ""(.*)"" ����� Id ""(.*)""")]
        public void �������������������������Id(string userName, string id)
        {
            var user = RavenDb.CurrentStorageSession.Query<UserEntity>().Single(u => u.Username == userName);
            user.UserId = new Guid(id);
            RavenDb.CurrentStorageSession.Save(user);
        }

        [Then(@"������������ � Id ""(.*)"" ����� ��������� ����")]
        public void ���������������Id������������������(Guid id, Table table)
        {
            dynamic data = table.CreateDynamicInstance();

            RavenDb.CurrentStorageSession.Query<UserEntity>().Should().Contain(u => u.UserId == id);
            var user = RavenDb.CurrentStorageSession.Query<UserEntity>().Single(u => u.UserId == id);

            user.Password                                .Should().Be((string)  data.������);
            user.Username                                .Should().Be((string)  data.���);
            user.Email                                   .Should().Be((string)  data.Email);
            user.ApplicationName                         .Should().Be((string)  data.ApplicationName);
            user.Comment                                 .Should().Be((string)  data.�����������);
            user.ConfirmationToken                       .Should().Be((string)  data.ConfirmationToken);
            user.CreateDate                              .Should().Be((DateTime)data.CreateDate);
            user.IsApproved                              .Should().Be((bool)    data.IsApproved);
            user.IsLockedOut                             .Should().Be((bool)    data.IsLockedOut);
            user.LastActivityDate                        .Should().Be((DateTime)data.LastActivityDate);
            user.LastLockoutDate                         .Should().Be((DateTime)data.LastLockoutDate);
            user.LastLoginDate                           .Should().Be((DateTime)data.LastLoginDate);
            user.LastPasswordChangedDate                 .Should().Be((DateTime)data.LastPasswordChangedDate);
            user.LastPasswordFailureDate                 .Should().Be((DateTime)data.LastPasswordFailureDate);
            user.PasswordFailuresSinceLastSuccess        .Should().Be((int)     data.PasswordFailuresSinceLastSuccess);
            user.PasswordVerificationToken               .Should().Be((string)  data.PasswordVerificationToken);
            user.PasswordVerificationTokenExpirationDate .Should().Be((DateTime)data.PasswordVerificationTokenExpirationDate);
        }

        [Then(@"���������� (.*) ������������")]
        public void ������������������������(int count)
        {
            var actualCount = RavenDb.CurrentStorageSession.Query<UserEntity>().WaitForNonStaleResultsAsOfLastWrite().Count();
            actualCount.Should().Be(count);
        }

        [Then(@"���������� ������������ ""(.*)"" �� ������� �����")]
        public void ��������������������������������������(string username, Table table)
        {
            var expectedRoles = table.Rows.Select(r => r["����"]).ToList();

            var allUsers = RavenDb.CurrentSession.Query<UserEntity>().WaitForNonStaleResultsAsOfLastWrite().ToList();
            var allRoles = RavenDb.CurrentSession.Query<RoleEntity>().WaitForNonStaleResultsAsOfLastWrite().ToList();

            allUsers.Where(u => u.Username == username).Should().HaveCount(1);

            var user = allUsers.Single(u => u.Username == username);
            user.Roles.Should().HaveCount(table.RowCount);

            foreach (var roleReference in user.Roles)
            {
                var role = allRoles.Single(r => r.Id == roleReference.Id);
                role.RoleName.Should().Be(roleReference.Name);
                expectedRoles.Should().Contain(role.RoleName);
            }
        }
    }
}