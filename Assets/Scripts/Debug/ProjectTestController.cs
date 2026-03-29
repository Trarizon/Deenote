#nullable enable

using Deenote;
using Deenote.Contexts;
using Deenote.ProjectManagement;
using UnityEngine;

#if UNITY_EDITOR

public sealed class ProjectTestController : MonoBehaviour
{
    private ProjectContext _context;
    private ProjectManagerB _projectManager;

    private void Awake()
    {
        _context = MainSystem.Contexts.Project;
        _projectManager = MainSystem.ProjectManagerB;
    }

    private async void Start()
    {
        var proj = await DebugUtils.GetTestProject();
        Debug.Log($"Loaded project: fake");
        _context.CurrentProject = proj;
    }
}

#endif
