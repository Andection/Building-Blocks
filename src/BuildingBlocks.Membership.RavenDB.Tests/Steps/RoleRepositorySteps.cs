using System;
using System.Collections.Generic;
using System.Linq;
using BuildingBlocks.Membership.Entities;
using BuildingBlocks.Membership.RavenDB.DomainModel;
using BuildingBlocks.Store.RavenDB;
using BuildingBlocks.Store.RavenDB.TestHelpers.SpecFlow;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace BuildingBlocks.Membership.RavenDB.Tests.Steps
{
    [Binding]
    public class RoleRepositorySteps
    {
        private bool RoleExistsResult
        {
            get { return ScenarioContext.Current.Get<bool>("RoleExistsResult"); }
            set { ScenarioContext.Current.Set(value, "RoleExistsResult"); }
        }

        private IEnumerable<string> RolesResult
        {
            get { return ScenarioContext.Current.Get<IEnumerable<string>>("RolesResult"); }
            set { ScenarioContext.Current.Set(value, "RolesResult"); }
        }

        [When(@"��������� ��� ���� ""(.*)"" ����������")]
        public void ������������������������������(string roleName)
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.Storage);
            RoleExistsResult = roleRepository.IsRoleExists(MembershipSettings.DefaultApplicationName, roleName);
        }

        [When(@"�������� ������ �����")]
        public void �����������������������()
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.Storage);
            RolesResult = roleRepository.GetAll(MembershipSettings.DefaultApplicationName);
        }

        [When(@"�������� ������ ����� ���������� �����")]
        public void ��������������������������������������(Table table)
        {
            var namesList = table.Rows.Select(r => r["���"]).ToArray();
            var roleRepository = new RoleRepositoryImpl(RavenDb.Storage);
            RolesResult = roleRepository.FindRolesByNames(MembershipSettings.DefaultApplicationName, namesList);
        }

        [When(@"������� ���� ""(.*)""")]
        public void ���������������(string roleName)
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.Storage);
            roleRepository.CreateRole(Guid.NewGuid(), MembershipSettings.DefaultApplicationName, roleName);
        }

        [When(@"������� ���� ""(.*)""")]
        public void ���������������(string roleName)
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.Storage);
            roleRepository.DeleteRole(MembershipSettings.DefaultApplicationName, roleName);
        }

        [Then(@"��������� �������� �������� ��� ���� ""(.*)""")]
        public void ��������������������������������������������(bool expectedValue)
        {
            RoleExistsResult.Should().Be(expectedValue);
        }

        [Then(@"������������ ��������� ������ �����")]
        public void ����������������������������������(Table table)
        {
            RolesResult.Should().HaveSameCount(table.Rows);
            for (var i = 0; i < table.RowCount; i++)
            {
                var expectedRoleName = table.Rows[i]["���"];
                var actualRole = RolesResult.ElementAt(i);
                actualRole.Should().Be(expectedRoleName);
            }
        }

        [Then(@"���������� ""(.*)"" ����")]
        [Then(@"���������� ""(.*)"" �����")]
        public void ����������������(int count)
        {
            RavenDb.CurrentStorageSession.Query<RoleEntity>().WaitForNonStaleResultsAsOfLastWrite().Count().Should().Be(count);
        }

        [Then(@"���������� ���� ""(.*)"" �� ������� �������������")]
        public void ��������������������������������������(string roleName, Table table)
        {
            var allUsers = RavenDb.CurrentSession.Query<UserEntity>().WaitForNonStaleResultsAsOfLastWrite().ToList();
            var allRoles = RavenDb.CurrentSession.Query<RoleEntity>().WaitForNonStaleResultsAsOfLastWrite().ToList();
            allRoles.Should().Contain(r => r.RoleName == roleName);

            foreach (var username in table.Rows.Select(r => r["���"]))
            {
                var user = allUsers.Single(u => u.Username == username);
                user.Roles.Should().Contain(roleName);
            }
        }
    }
}