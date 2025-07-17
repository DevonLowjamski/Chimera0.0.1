using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.UI.Core;
using System;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Economic overview HUD component showing financial status.
    /// Displays total funds, daily revenue, expenses, and profit calculations.
    /// </summary>
    public class EconomicOverviewHUD : IDataBinding
    {
        private VisualElement _rootElement;
        private Label _fundsLabel;
        private Label _revenueLabel;
        private Label _expensesLabel;
        private Label _profitLabel;

        public VisualElement CreateHUDElement()
        {
            _rootElement = new VisualElement();
            _rootElement.name = "economic-overview-hud";
            _rootElement.AddToClassList("hud-element");
            _rootElement.AddToClassList("economic-hud");

            var title = new Label("Economic Overview");
            title.AddToClassList("hud-title");
            _rootElement.Add(title);

            var content = new VisualElement();
            content.AddToClassList("hud-content");

            _fundsLabel = new Label("Funds: $0");
            _fundsLabel.AddToClassList("hud-value");
            content.Add(_fundsLabel);

            _revenueLabel = new Label("Revenue: $0/day");
            _revenueLabel.AddToClassList("hud-value");
            content.Add(_revenueLabel);

            _expensesLabel = new Label("Expenses: $0/day");
            _expensesLabel.AddToClassList("hud-value");
            content.Add(_expensesLabel);

            _profitLabel = new Label("Profit: $0/day");
            _profitLabel.AddToClassList("hud-value");
            content.Add(_profitLabel);

            _rootElement.Add(content);
            return _rootElement;
        }

        public void UpdateData(object data)
        {
            if (data is EconomicData economicData)
            {
                _fundsLabel.text = $"Funds: ${economicData.TotalFunds:F2}";
                _revenueLabel.text = $"Revenue: ${economicData.DailyRevenue:F2}/day";
                _expensesLabel.text = $"Expenses: ${economicData.DailyExpenses:F2}/day";
                var profit = economicData.DailyRevenue - economicData.DailyExpenses;
                _profitLabel.text = $"Profit: ${profit:F2}/day";
            }
        }
    }
}