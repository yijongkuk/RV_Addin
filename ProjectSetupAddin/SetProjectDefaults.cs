using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace ProjectSetupAddin
{
    [Transaction(TransactionMode.Manual)]
    public class SetProjectDefaults : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            using (Transaction tx = new Transaction(doc, "Set Project Defaults"))
            {
                tx.Start();

                // Example default settings - replace with real logic.
                SetProjectInformation(doc);
                CreateGrids(doc);

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

        private void CreateGrids(Document doc)
        {
            // Example grid creation. Real logic would use site data to determine count and spacing.
            XYZ start = new XYZ(0, 0, 0);
            XYZ end = new XYZ(100, 0, 0);
            Line gridLine = Line.CreateBound(start, end);
            Grid grid = Grid.Create(doc, gridLine);
            grid.Name = "1";
        }
    }
}
