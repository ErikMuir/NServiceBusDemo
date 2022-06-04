using System.Text.Json;
using Demo.Workflow;
using Demo.Workflow.Sagas;

namespace Demo.Api;

public static class SagaPersistenceHelper
{
    public static string GetSagaData(Guid workflowId)
    {
        try
        {
            var sagaDataDirectoryPath = "../Demo.Workflow/bin/Debug/net6.0/.sagas/Demo.Workflow.Sagas.WorkflowSaga";
            var sagaDataDirectoryInfo = new DirectoryInfo(sagaDataDirectoryPath);
            var sagaDataFileInfo = sagaDataDirectoryInfo.GetFiles().First(file => file.Extension == ".json");
            var sagaDataRawJson = File.ReadAllText(sagaDataFileInfo.FullName);

            // workaround for weird persistence bug
            var sagaDataCleanJson = sagaDataRawJson.Substring(0, sagaDataRawJson.IndexOf('}', StringComparison.Ordinal) + 1);

            var sagaData = JsonSerializer.Deserialize<WorkflowSagaData>(sagaDataCleanJson);
            return sagaData?.Status.ToFriendlyString() ?? "Could not retrieve workflow status.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
