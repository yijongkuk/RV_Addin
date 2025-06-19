using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Microsoft.VisualBasic; // for InputBox
using System;
using System.Linq;
using System.Windows.Forms;

namespace ProjectSetupAddin
{
    internal class SetupForm : Form
    {
        public TextBox LevelHeights { get; private set; } = new TextBox();
        public TextBox GridX { get; private set; } = new TextBox();
        public TextBox GridY { get; private set; } = new TextBox();
        public TextBox SpacingX { get; private set; } = new TextBox();
        public TextBox SpacingY { get; private set; } = new TextBox();

        public SetupForm()
        {
            Text = "Project Setup";
            Width = 250;
            Height = 220;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 2 };
            table.Controls.Add(new Label { Text = "Grid Count X" }, 0, 0);
            table.Controls.Add(GridX, 1, 0);
            table.Controls.Add(new Label { Text = "Grid Count Y" }, 0, 1);
            table.Controls.Add(GridY, 1, 1);
            table.Controls.Add(new Label { Text = "Spacing X (m)" }, 0, 2);
            table.Controls.Add(SpacingX, 1, 2);
            table.Controls.Add(new Label { Text = "Spacing Y (m)" }, 0, 3);
            table.Controls.Add(SpacingY, 1, 3);
            table.Controls.Add(new Label { Text = "Level heights (m)" }, 0, 4);
            table.Controls.Add(LevelHeights, 1, 4);

            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Fill };
            table.Controls.Add(ok, 0, 5);
            table.SetColumnSpan(ok, 2);

            Controls.Add(table);
            AcceptButton = ok;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class SetProjectDefaults : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Show a single dialog to gather all parameters
            using (SetupForm form = new SetupForm())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return Result.Cancelled;

                int gridX = ParseInt(form.GridX.Text, 5);
                int gridY = ParseInt(form.GridY.Text, 5);
                double spacingX = ParseDouble(form.SpacingX.Text, 8.0);
                double spacingY = ParseDouble(form.SpacingY.Text, 8.0);
                double[] levelHeights = ParseLevelHeights(form.LevelHeights.Text);

                using (Transaction tx = new Transaction(doc, "Set Project Defaults"))
                {
                    tx.Start();

                    SetProjectInformation(doc);
                    DeleteExistingLevels(doc);
                    CreateGridSystem(doc, gridX, gridY, spacingX, spacingY);
                    CreateLevels(doc, levelHeights);

                    tx.Commit();
                }

                TaskDialog.Show("Project Setup", "Project defaults have been applied.");
            }

            return Result.Succeeded;
        }

        private void SetProjectInformation(Document doc)
        {
            ProjectInfo pInfo = doc.ProjectInformation;
            if (pInfo == null)
                return;

            // Example data. In a real application, gather these from user input.
            pInfo.Name = "New Project";
            pInfo.Number = "001";
            pInfo.Address = "Site Area: 10000 sqm";
        }

        private void CreateLevels(Document doc, double[] levelHeights)
        {
            double elevation = 0;
            for (int i = 0; i < levelHeights.Length; i++)
            {
                elevation += UnitUtils.ConvertToInternalUnits(levelHeights[i], UnitTypeId.Meters);
                Level level = Level.Create(doc, elevation);
                level.Name = $"Level {i + 1}";
            }
        }

        private void CreateGridSystem(Document doc, int xCount, int yCount, double spacingXMeters, double spacingYMeters)
        {
            double sx = UnitUtils.ConvertToInternalUnits(spacingXMeters, UnitTypeId.Meters);
            double sy = UnitUtils.ConvertToInternalUnits(spacingYMeters, UnitTypeId.Meters);

            for (int i = 0; i < xCount; i++)
            {
                XYZ start = new XYZ(i * sx, 0, 0);
                XYZ end = new XYZ(i * sx, sy * (yCount - 1), 0);
                Line gridLine = Line.CreateBound(start, end);
                Grid grid = Grid.Create(doc, gridLine);
                grid.Name = $"X{i + 1}";
            }

            for (int j = 0; j < yCount; j++)
            {
                XYZ start = new XYZ(0, j * sy, 0);
                XYZ end = new XYZ(sx * (xCount - 1), j * sy, 0);
                Line gridLine = Line.CreateBound(start, end);
                Grid grid = Grid.Create(doc, gridLine);
                grid.Name = $"Y{j + 1}";
            }
        }

        private void DeleteExistingLevels(Document doc)
        {
            var levelIds = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .ToElementIds();
            if (levelIds.Count > 0)
                doc.Delete(levelIds);
        }

        private int ParseInt(string text, int defaultValue)
        {
            return int.TryParse(text, out int v) ? v : defaultValue;
        }

        private double ParseDouble(string text, double defaultValue)
        {
            return double.TryParse(text, out double v) ? v : defaultValue;
        }

        private double[] ParseLevelHeights(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new double[] { 3.0 };

            string[] parts = text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(p => ParseDouble(p, 3.0)).ToArray();
        }
    }
}
