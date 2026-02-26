using Infrastructure.Repository;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Travel.Tests.Shared.Helper;

public class InfrastructureHelper
{
    private const string AgentTemplateFolder = "Templates";

    public static IAgentTemplateRepository Create()
    {
        var fileStorageSettings = Options.Create(new FileStorageSettings
        {
            AgentTemplateFolder = AgentTemplateFolder,
            AgentThreadFolder = "",
            CheckpointFolder = "",
            SessionFolder = ""

        });

        var mockFileLogger = new Mock<ILogger<FileRepository>>();
        var fileRepository = new FileRepository(mockFileLogger.Object);

        var templateRepository = new AgentTemplateRepository(fileRepository, fileStorageSettings);

        return templateRepository;
    }
}