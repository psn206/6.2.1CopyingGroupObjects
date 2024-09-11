using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyingGroupObjects
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, "Выберет группу объектов");
            Element element = doc.GetElement(reference);
            Group group = element as Group;
            XYZ point = uiDoc.Selection.PickPoint("Выберте точку вставки");

            Transaction ts = new Transaction(doc,"Копирование группы объеетов");
            ts.Start();
            doc.Create.PlaceGroup(point,group.GroupType);
            ts.Commit();

            return Result.Succeeded;
        }
    }
}
