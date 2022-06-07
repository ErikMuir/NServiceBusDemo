using System.Text.Json;
using Workflow;
using Workflow.Sagas;

namespace Api;

public static class SagaPersistenceHelper
{
    // This is such a hack, but it'll do for a demo
    public static WorkflowSagaData? GetSagaData(Guid workflowId)
    {
        var sagaDataDirectoryPath = "../Workflow/bin/Debug/net6.0/.sagas/Workflow.Sagas.WorkflowSaga";
        var sagaDataDirectoryInfo = new DirectoryInfo(sagaDataDirectoryPath);
        var sagas = sagaDataDirectoryInfo.GetFiles().Where(file => file.Extension == ".json");

        foreach (var file in sagas)
        {
            try
            {
                var sagaDataRawJson = File.ReadAllText(file.FullName);

                // workaround for weird persistence bug
                var closingBraceIndex = sagaDataRawJson.IndexOf('}', StringComparison.Ordinal);
                var sagaDataCleanJson = sagaDataRawJson.Substring(0, closingBraceIndex + 1);

                var sagaData = JsonSerializer.Deserialize<WorkflowSagaData>(sagaDataCleanJson);
                if (sagaData?.WorkflowId == workflowId)
                    return sagaData;
            }
            catch (Exception)
            {
                // Ignore.
            }
        }

        return default;
    }
}
