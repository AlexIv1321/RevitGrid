#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace RevitGrid
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        private int _step = 0;
        private Line _line;
        private int _coordinatesByX = 0;
        private int _coordinatesByY = 0;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            View activeView = uidoc.ActiveView;
            SketchPlane sketch = activeView.SketchPlane;

            using (Transaction firstTrans = new Transaction(doc))
            {
                try
                {
                    firstTrans.Start("Start");
                    while (_step != 10)
                    {
                        CreateGridHorizontal(doc);
                        CreateLinearDimension(doc, sketch);
                        _coordinatesByY += 10;
                        _step++;
                    }
                    while (_step != 20)
                    {
                        CreateGridVertical(doc);
                        CreateLinearDimension(doc, sketch);
                        _coordinatesByX += 11;
                        _step++;
                    }
                    firstTrans.Commit();
                    return Result.Succeeded;
                }
                catch
                {
                    return Result.Failed;
                }
            }
        }

        private void CreateGridHorizontal(Document doc)
        {
            XYZ start = new XYZ(-20, _coordinatesByY, 0);
            XYZ end = new XYZ(120, _coordinatesByY, 0);
            Line geomLine = Line.CreateBound(start, end);

            Grid lineGrid = Grid.Create(doc, geomLine);

            if (null == lineGrid)
            {
                throw new Exception("Create a new straight grid failed.");
            }
            if (_coordinatesByY == 0)
            {
                lineGrid.Name = "A";
            }
        }

        private void CreateGridVertical(Document doc)
        {
            XYZ start = new XYZ(_coordinatesByX, -20, 0);
            XYZ end = new XYZ(_coordinatesByX, 120, 0);
            Line geomLine = Line.CreateBound(start, end);

            Grid lineGrid = Grid.Create(doc, geomLine);

            if (null == lineGrid)
            {
                throw new Exception("Create a new straight grid failed.");
            }
            if (_coordinatesByX == 0)
            {
                lineGrid.Name = "1";
            }
        }

        private void CreateLinearDimension(Document doc, SketchPlane sketch)
        {
            if (_step < 9)
            {
                XYZ start = new XYZ(-10, _coordinatesByY, 0);
                XYZ end = new XYZ(-10, _coordinatesByY + 10, 0);
                _line = Line.CreateBound(start, end);
            }
            else if (_step >= 10 && _step < 19)
            {
                XYZ start = new XYZ(_coordinatesByX, -10, 0);
                XYZ end = new XYZ(_coordinatesByX + 11, -10, 0);
                _line = Line.CreateBound(start, end);
            }

            ModelCurve modelcurve = doc.Create.NewModelCurve(_line, sketch);
            ReferenceArray ra = new ReferenceArray();
            ra.Append(modelcurve.GeometryCurve.GetEndPointReference(0));
            ra.Append(modelcurve.GeometryCurve.GetEndPointReference(1));
            doc.Create.NewDimension(doc.ActiveView, _line, ra);
        }
    }
}
