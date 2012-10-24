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

        private IEnumerable<Role> RolesResult
        {
            get { return ScenarioContext.Current.Get<IEnumerable<Role>>("RolesResult"); }
            set { ScenarioContext.Current.Set(value, "RolesResult"); }
        }

        [When(@"��������� ��� ���� ""(.*)"" ����������")]
        public void ������������������������������(string roleName)
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.CurrentStorageSession);
            RoleExistsResult = roleRepository.IsRoleExists(roleName);
        }

        [When(@"�������� ������ �����")]
        public void �����������������������()
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.CurrentStorageSession);
            RolesResult = roleRepository.GetAll();
        }

        [When(@"�������� ������ ����� ���������� �����")]
        public void ��������������������������������������(Table table)
        {
            var namesList = table.Rows.Select(r => r["���"]).ToArray();
            var roleRepository = new RoleRepositoryImpl(RavenDb.CurrentStorageSession);
            RolesResult = roleRepository.FindRolesByNames(namesList);
        }

        [When(@"������� ���� ""(.*)""")]
        public void ���������������(string roleName)
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.CurrentStorageSession);
            roleRepository.CreateRole(new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = roleName,
                    Description = roleName,
                });
        }

        [When(@"������� ���� ""(.*)"" �� ������� �������������")]
        public void �������������������������������������(string roleName, Table table)
        {
            var roleRepository = new RoleRepositoryImpl(RavenDb.CurrentStorageSession);
            roleRepository.CreateRole(new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName,
                Description = roleName,
                Users = table.Rows.Select(r => r["���"]).ToList()
            });
        }

        [When(@"������� ���� ""(.*)""")]
        public void ���������������(string roleName)
        {
            var role = RavenDb.CurrentStorageSession.Query<RoleEntity>().Single(r => r.RoleName == roleName).ToRole();
            var roleRepository = new RoleRepositoryImpl(RavenDb.CurrentStorageSession);
            roleRepository.DeleteRole(role);
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
                actualRole.RoleName.Should().Be(expectedRoleName);
            }
        }

        [Then(@"���������� (.*) ����")]
        [Then(@"���������� (.*) �����")]
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

            var role = allRoles.First(r => r.RoleName == roleName);
            role.Users.Should().HaveCount(table.RowCount);
            foreach (var userName in table.Rows.Select(r => r["���"]))
            {
                role.Users.Should().Contain(u => u.Name == userName);
                var userReference = role.Users.Single(u => u.Name == userName);

                allUsers.Should().Contain(u => u.Id == userReference.Id);
                var user = allUsers.Single(u => u.Id == userReference.Id);
                user.Username.Should().Be(userName);
            }
        }
    }
}