using System;
using System.Collections.Generic;
using System.Linq;
using BuildingBlocks.Common;
using BuildingBlocks.Membership.Entities;
using BuildingBlocks.Membership.RavenDB.DomainModel;
using BuildingBlocks.Membership.RavenDB.Queries;
using BuildingBlocks.Membership.RavenDB.Queries.Criteria;
using BuildingBlocks.Query;
using BuildingBlocks.Store.RavenDB.TestHelpers.SpecFlow;
using FluentAssertions;
using Moq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace BuildingBlocks.Membership.RavenDB.Tests.Steps
{
    [Binding]
    public class UserRepositorySteps
    {
        private bool UserExistsResult
        {
            get { return ScenarioContext.Current.Get<bool>("UserExistsResult"); }
            set { ScenarioContext.Current.Set(value, "UserExistsResult"); }
        }

        private IEnumerable<User> UsersResult
        {
            get { return ScenarioContext.Current.Get<IEnumerable<User>>("UsersResult"); }
            set { ScenarioContext.Current.Set(value, "UsersResult"); }
        }

        private Page<User> UserPageResult
        {
            get { return ScenarioContext.Current.Get<Page<User>>("UserPageResult"); }
            set { ScenarioContext.Current.Set(value, "UserPageResult"); }
        }

        private int UsersCountResult
        {
            get { return ScenarioContext.Current.Get<int>("UsersCountResult"); }
            set { ScenarioContext.Current.Set(value, "UsersCountResult"); }
        }

        private IQueryBuilder QueryBuilder
        {
            get { return ScenarioContext.Current.Obtain(() => new QueryBuilder(QueryFactory), "QueryBuilder"); }
        }

        private IQueryFactory QueryFactory
        {
            get { return ScenarioContext.Current.Obtain(() => new Mock<IQueryFactory>().Object, "QueryFactory"); }
        }

        [When(@"��������� ��� ������������ � ������ ""(.*)"" ����������")]
        public void ��������������������������������������(string userName)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UserExistsResult = userRepository.HasUserWithName(MembershipSettings.ApplicationName, userName);
        }

        [When(@"��������� ��� ������������ � email ""(.*)"" ����������")]
        public void ����������������������������ByEmail����������(string email)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UserExistsResult = userRepository.HasUserWithEmail(MembershipSettings.ApplicationName, email);
        }

        [When(@"�������� ������ ������������� ���������� �����")]
        public void ����������������������������������������������(Table table)
        {
            var namesList = table.Rows.Select(r => r["���"]).ToArray();
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UsersResult = userRepository.FindUsersByNames(MembershipSettings.ApplicationName, namesList);
        }

        [When(@"���� ������������ � email ""(.*)""")]
        public void ���������������������Email(string email)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UsersResult = userRepository.FindUserByEmail(MembershipSettings.ApplicationName, email).ToEnumerableOrEmpty();
        }

        [When(@"���� ������������ � Id ""(.*)""")]
        public void ���������������������Id(string id)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UsersResult = userRepository.FindUserById(new Guid(id)).ToEnumerableOrEmpty();
        }

        [When(@"��������� (.*) �������� ������������ �� (.*) ������������ � �������� �� email ""(.*)""")]
        public void ����������������������������������������������������������Email(int pageNumber, int pageSize, string emailToMatch)
        {
            Mock.Get(QueryFactory)
                .Setup(f => f.Create<FindByEmailSubstring, Page<User>>())
                .Returns(new UsersColumnMatchedToSubstring(RavenDb.CurrentStorageSession));

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UserPageResult = userRepository.GetUsersPageByEmail(MembershipSettings.ApplicationName, emailToMatch, pageNumber - 1, pageSize);
        }

        [When(@"��������� (.*) �������� ������������ �� (.*) ������������ � �������� �� ����� ""(.*)""")]
        public void ���������������������������������������������������������������(int pageNumber, int pageSize, string usernameToMatch)
        {
            Mock.Get(QueryFactory)
                .Setup(f => f.Create<FindByUsernameSubstring, Page<User>>())
                .Returns(new UsersColumnMatchedToSubstring(RavenDb.CurrentStorageSession));

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UserPageResult = userRepository.GetUsersPageByUsername(MembershipSettings.ApplicationName, usernameToMatch, pageNumber - 1, pageSize);
        }

        [When(@"��������� (.*) �������� ������������ �� (.*) ������������")]
        public void �����������������������������������������������(int pageNumber, int pageSize)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UserPageResult = userRepository.GetUsersPage(MembershipSettings.ApplicationName, pageNumber - 1, pageSize);
        }

        [When(@"������� ������ ������������ ""(.*)""")]
        public void �����������������������������(string username)
        {
            var user = new User(Guid.NewGuid(), username, username + "@mail.ru", MembershipSettings.ApplicationName)
            {
                Password = "123"
            };
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            userRepository.AddUser(user);
        }

        [When(@"������� ������ ������������ ""(.*)"" � ������������ ������")]
        public void ������������������������������������������������(string username, Table table)
        {
            var user = new User(Guid.NewGuid(), username, username + "@mail.ru", MembershipSettings.ApplicationName)
            {
                Password = "123"
            };

            foreach (var row in table.CreateDynamicSet())
            {
                string roleName = row.����;
                user.AddRole(roleName);
            }

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            userRepository.AddUser(user);
        }

        [When(@"�������� ���������� ������������� � ��������� ����������� �� (.*)")]
        public void ����������������������������������������������������������(DateTime dateTime)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            UsersCountResult = userRepository.GetUsersCountWithLastActivityDateGreaterThen(MembershipSettings.ApplicationName, dateTime);
        }

        [When(@"��� ������������ � Id ""(.*)"" ��������� ����")]
        public void ��������������������Id�������������(Guid id, Table table)
        {
            var user = RavenDb.CurrentStorageSession.Query<UserEntity>().Single(u => u.UserId == id).ToUser();

            dynamic data = table.CreateDynamicInstance();
            user.Password                                = data.������;
            user.Username                                = data.���;
            user.ApplicationName                         = data.ApplicationName;
            user.Email                                   = data.Email;
            user.Comment                                 = data.�����������;
            user.ConfirmationToken                       = data.ConfirmationToken;
            user.CreateDate                              = data.CreateDate;
            user.IsApproved                              = data.IsApproved;
            user.IsLockedOut                             = data.IsLockedOut;
            user.LastActivityDate                        = data.LastActivityDate;
            user.LastLockoutDate                         = data.LastLockoutDate;
            user.LastLoginDate                           = data.LastLoginDate;
            user.LastPasswordChangedDate                 = data.LastPasswordChangedDate;
            user.LastPasswordFailureDate                 = data.LastPasswordFailureDate;
            user.PasswordFailuresSinceLastSuccess        = data.PasswordFailuresSinceLastSuccess;
            user.PasswordVerificationToken               = data.PasswordVerificationToken;
            user.PasswordVerificationTokenExpirationDate = data.PasswordVerificationTokenExpirationDate;

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            userRepository.SaveUser(user);
        }

        [When(@"������������ ""(.*)"" ������ ���������� �����")]
        public void �������������������������������������(string userName, Table table)
        {
            var user = RavenDb.CurrentStorageSession.Query<UserEntity>().Single(u => u.Username == userName).ToUser();

            foreach (var role in user.Roles.ToList())
            {
                user.RemoveRole(role);
            }

            foreach (var row in table.CreateDynamicSet())
            {
                string roleName = row.����;
                user.AddRole(roleName);
            }
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            userRepository.SaveUser(user);
        }

        [When(@"������� ������������ ""(.*)""")]
        public void �����������������������(string userName)
        {
            var user = RavenDb.CurrentStorageSession.Query<UserEntity>().Single(u => u.Username == userName).ToUser();

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession);
            userRepository.DeleteUser(user);
        }

        [Then(@"���������� ������������� ����� (.*)")]
        public void ������������������������������(int count)
        {
            UsersCountResult.Should().Be(count);
        }

        [Then(@"������������ �������� �������������")]
        public void �����������������������������������(dynamic page)
        {
            UserPageResult.TotalItemCount.Should().Be((int) page.�����);
            UserPageResult.Items.Should().HaveCount((int) page.����������);
        }

        [Then(@"������������ �������� � ��������������")]
        public void �������������������������������������(Table table)
        {
            UserPageResult.Items.Should().HaveCount(table.RowCount);
            for (int i = 0; i < table.RowCount; i++)
            {
                var username = table.Rows[i]["���"];
                var actualUser = UserPageResult.Items.ElementAt(i);
                actualUser.Username.Should().Be(username);
            }
        }

        [Then(@"�� ������������ �� ������ ������������")]
        public void ������������������������������������()
        {
            UsersResult.Should().BeEmpty();
        }

        [Then(@"��������� �������� �������� ��� ������������ ""(.*)""")]
        public void ������������������������������������������(bool isExists)
        {
            UserExistsResult.Should().Be(isExists);
        }

        [Then(@"������������ ��������� ������ �������������")]
        public void ������������������������������������������(Table table)
        {
            UsersResult.Should().HaveSameCount(table.Rows);
            for (var i = 0; i < table.RowCount; i++)
            {
                var expectedUserName = table.Rows[i]["���"];
                var user = UsersResult.ElementAt(i);
                user.Username.Should().Be(expectedUserName);
            }
        }
    }
}