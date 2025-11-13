namespace IPA.Bcfier.Ipc
{
    public enum IpcMessageCommand
    {
        AppClosed = 0,

        CreateViewpoint = 1,

        ViewpointCreated = 2,

        ShowViewpoint = 3,

        ViewpointShown = 4,

        CreateNavisworksClashDetectionIssues = 5,

        NavisworksClashDetectionIssuesCreated = 6,

        GetNavisworksAvailableClashes = 7,

        NavisworksAvailableClashes = 8,

        PluginErrorEncountered = 9,

        CadClosing = 10,

        NavisworksClashIssuesTotalCount = 11,

        NavisworksClashIssuesCurrentCount = 12,

        NavisworksClashIssuesCancellation = 13,

        RevitProjectChanged = 15,

        RefreshProjectData = 16,

        GetElementNamesList = 17,

        ReturnElementNamesList = 18,

        SelectElement = 19,

        SelectElementResult = 20
    }
}
