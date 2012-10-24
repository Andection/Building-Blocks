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
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UserExistsResult = userRepository.HasUserWithName(userName);
        }

        [When(@"��������� ��� ������������ � email ""(.*)"" ����������")]
        public void ����������������������������ByEmail����������(string email)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UserExistsResult = userRepository.HasUserWithEmail(email);
        }

        [When(@"�������� ������ ������������� ���������� �����")]
        public void ����������������������������������������������(Table table)
        {
            var namesList = table.Rows.Select(r => r["���"]).ToArray();
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UsersResult = userRepository.FindUsersByNames(namesList);
        }

        [When(@"���� ������������ � email ""(.*)""")]
        public void ���������������������Email(string email)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UsersResult = userRepository.FindUserByEmail(email).ToEnumerableOrEmpty();
        }

        [Given(@"������������ ""(.*)"" ����� Id ""(.*)""")]
        public void �������������������������Id(string userName, string id)
        {
            var user = RavenDb.CurrentStorageSession.Query<UserEntity>().Single(u => u.Username == userName);
            user.UserId = new Guid(id);
            RavenDb.CurrentStorageSession.Save(user);
        }

        [When(@"���� ������������ � Id ""(.*)""")]
        public void ���������������������Id(string id)
        {
            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UsersResult = userRepository.FindUserById(new Guid(id)).ToEnumerableOrEmpty();
        }

        [When(@"��������� (.*) �������� ������������ �� (.*) ������������ � �������� �� email ""(.*)""")]
        public void ����������������������������������������������������������EmailKreml_Uc(int pageNumber, int pageSize, string emailToMatch)
        {
            Mock.Get(QueryFactory)
                .Setup(f => f.Create<FindByEmailSubstring, Page<User>>())
                .Returns(new UserWithMatchedEmails(RavenDb.CurrentStorageSession));

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UserPageResult = userRepository.GetUsersPageByEmail(emailToMatch, pageNumber - 1, pageSize);
        }

        [When(@"��������� (.*) �������� ������������ �� (.*) ������������ � �������� �� ����� ""(.*)""")]
        public void ���������������������������������������������������������������(int p0, int p1, string p2)
        {
            Mock.Get(QueryFactory)
                .Setup(f => f.Create<FindByUsernameSubstring, Page<User>>())
                .Returns(new UserWithMatchedUsernames(RavenDb.CurrentStorageSession));

            var userRepository = new UserRepositoryImpl(RavenDb.CurrentStorageSession, QueryBuilder);
            UserPageResult = userRepository.GetUsersPageByEmail(emailToMatch, pageNumber - 1, pageSize);
        }

        [Then(@"������������ �������� �������������")]
        public void �����������������������������������(dynamic page)
        {
            UserPageResult.TotalItemCount.Should().Be((int) page.�����);
            UserPageResult.Items.Should().HaveCount((int) page.����������);
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