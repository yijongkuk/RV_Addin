using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Microsoft.VisualBasic; // for InputBox
using System;

namespace ProjectSetupAddin
{
    [Transaction(TransactionMode.Manual)]
    public class SetProjectDefaults : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Gather simple input from the user. Values are in meters.
            int levelCount = AskInt("Number of levels", 3);
            double levelHeight = AskDouble("Level height (m)", 3.0);
            int gridX = AskInt("Grid count X", 5);
            int gridY = AskInt("Grid count Y", 5);
            double spacingX = AskDouble("Grid spacing X (m)", 8.0);
            double spacingY = AskDouble("Grid spacing Y (m)", 8.0);

            using (Transaction tx = new Transaction(doc, "Set Project Defaults"))
            {
                tx.Start();

                SetProjectInformation(doc);
                CreateLevels(doc, levelCount, levelHeight);
                CreateGridSystem(doc, gridX, gridY, spacingX, spacingY);

                tx.Commit();
            }

            TaskDialog.Show("Project Setup", "Project defaults have been applied.");

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

        private void CreateLevels(Document doc, int count, double heightMeters)
        {
            double h = UnitUtils.ConvertToInternalUnits(heightMeters, UnitTypeId.Meters);
            for (int i = 0; i < count; i++)
            {
                Level level = Level.Create(doc, h * i);
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
                grid.Name = (i + 1).ToString();
            }

            for (int j = 0; j < yCount; j++)
            {
                XYZ start = new XYZ(0, j * sy, 0);
                XYZ end = new XYZ(sx * (xCount - 1), j * sy, 0);
                Line gridLine = Line.CreateBound(start, end);
                Grid grid = Grid.Create(doc, gridLine);
                grid.Name = ((char)('A' + j)).ToString();
            }
        }

        private int AskInt(string prompt, int defaultValue)
        {
            string input = Interaction.InputBox(prompt, "Project Setup", defaultValue.ToString());
            return int.TryParse(input, out int v) ? v : defaultValue;
        }

        private double AskDouble(string prompt, double defaultValue)
        {
            string input = Interaction.InputBox(prompt, "Project Setup", defaultValue.ToString());
            return double.TryParse(input, out double v) ? v : defaultValue;
        }
    }
}
