namespace DHI.Services.Jobs.Test
{
    using AutoFixture.Xunit2;
    using Workflows;
    using Xunit;

    public class CodeWorkflowTest
    {
        [Theory, AutoData]
        public void ToDefinitionIsOk(CodeWorkflow codeWorkflow)
        {
            var definition = codeWorkflow.ToDefinition();
            Assert.Equal(codeWorkflow.AssemblyName, definition.AssemblyName);
            Assert.Equal(codeWorkflow.Id, definition.TypeName);
        }
    }
}
