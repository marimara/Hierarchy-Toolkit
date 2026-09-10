namespace Meganeura.ProjectToolkit
{
    internal interface IProjectFeature
    {
        bool IsEnabled { get; }

        void Initialize();

        void OnProjectItemGUI(ProjectItemContext item);

        void Dispose();
    }
}
