using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.UI.Core;
using System;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Construction progress HUD component showing construction status.
    /// Displays active projects, completed projects, total value, workers, and efficiency.
    /// </summary>
    public class ConstructionProgressHUD : IDataBinding
    {
        private VisualElement _rootElement;
        private Label _activeProjectsLabel;
        private Label _completedProjectsLabel;
        private Label _totalValueLabel;
        private Label _activeWorkersLabel;
        private Label _efficiencyLabel;

        public VisualElement CreateHUDElement()
        {
            _rootElement = new VisualElement();
            _rootElement.name = "construction-progress-hud";
            _rootElement.AddToClassList("hud-element");
            _rootElement.AddToClassList("construction-hud");

            var title = new Label("Construction Progress");
            title.AddToClassList("hud-title");
            _rootElement.Add(title);

            var content = new VisualElement();
            content.AddToClassList("hud-content");

            _activeProjectsLabel = new Label("Active: 0");
            _activeProjectsLabel.AddToClassList("hud-value");
            content.Add(_activeProjectsLabel);

            _completedProjectsLabel = new Label("Completed: 0");
            _completedProjectsLabel.AddToClassList("hud-value");
            content.Add(_completedProjectsLabel);

            _totalValueLabel = new Label("Value: $0");
            _totalValueLabel.AddToClassList("hud-value");
            content.Add(_totalValueLabel);

            _activeWorkersLabel = new Label("Workers: 0");
            _activeWorkersLabel.AddToClassList("hud-value");
            content.Add(_activeWorkersLabel);

            _efficiencyLabel = new Label("Efficiency: 0%");
            _efficiencyLabel.AddToClassList("hud-value");
            content.Add(_efficiencyLabel);

            _rootElement.Add(content);
            return _rootElement;
        }

        public void UpdateData(object data)
        {
            if (data is ConstructionData constructionData)
            {
                _activeProjectsLabel.text = $"Active: {constructionData.ActiveProjects}";
                _completedProjectsLabel.text = $"Completed: {constructionData.CompletedProjects}";
                _totalValueLabel.text = $"Value: ${constructionData.TotalValue:F0}";
                _activeWorkersLabel.text = $"Workers: {constructionData.ActiveWorkers}";
                _efficiencyLabel.text = $"Efficiency: {constructionData.Efficiency:F1}%";
            }
        }
    }
}