using System;

namespace BeLazy
{
    internal class MappingHelper
    {
        internal static int DoMappingToAbstract(MapType mapType, int TMSSystemID, string itemName)
        {
            string idToReturn, table, searchField;
            switch (mapType)
            {
                case MapType.Language:
                    idToReturn = "LanguageID";
                    table = "tLanguagesMapping";
                    searchField = "LanguageName";
                    break;
                case MapType.Speciality:
                    idToReturn = "SpecialityID";
                    table = "tSpecialitiesMapping";
                    searchField = "SpecialityName";
                    break;
                case MapType.Unit:
                    idToReturn = "UnitID";
                    table = "tUnitsMapping";
                    searchField = "UnitName";
                    break;
                case MapType.Workflow:
                    idToReturn = "WorkflowID";
                    table = "tWorkflowsMapping";
                    searchField = "WorkflowName";
                    break;
                default:
                    throw new Exception("Unknown MapType.");
            }

            return DatabaseInterface.GetMappingToAbstractValue(idToReturn, table, TMSSystemID, searchField, itemName);

        }
    }

    public enum MapType
    {
        Language,
        Speciality,
        Workflow,
        Unit
    }

}