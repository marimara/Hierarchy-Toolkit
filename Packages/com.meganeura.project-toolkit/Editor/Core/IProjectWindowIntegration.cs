using System;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal interface IProjectWindowIntegration : IDisposable
    {
        event Action<string, Rect> ItemGUI;

        event Action ProjectChanged;
    }
}
