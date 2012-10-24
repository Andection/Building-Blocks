using System;
using TechTalk.SpecFlow;

namespace BuildingBlocks.Membership.RavenDB.Tests.Steps
{
    [Binding]
    public class StringToBoolTransform
    {
        [StepArgumentTransformation]
        public bool StringToBool(string value)
        {
            switch (value)
            {
                case "����������":
                    return true;
                case "�� ����������":
                    return false;
            }
            throw new ArgumentException("Unexpected string value", "value");
        }
    }
}