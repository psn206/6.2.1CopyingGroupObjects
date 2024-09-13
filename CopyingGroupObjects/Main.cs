using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;
                GroupPickFilter filter = new GroupPickFilter();

                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, filter,"Выберет группу объектов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;

                XYZ groupCenter = GetElementCenter(element);
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter;

                XYZ point = uiDoc.Selection.PickPoint("Выберте точку вставки");
                Room RoomInstallation = GetRoomByPoint(doc, point);
                XYZ RoomInstallationCenter = GetElementCenter(RoomInstallation);
                XYZ installationPoint = RoomInstallationCenter + offset;

                Transaction ts = new Transaction(doc, "Копирование группы объеетов");
                ts.Start();
                doc.Create.PlaceGroup(installationPoint, group.GroupType);
                ts.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }
        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach(Element element in collector)
            {
                Room room = element as Room;
                if(room != null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }               
            }
            return null;
        }

    }

    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
