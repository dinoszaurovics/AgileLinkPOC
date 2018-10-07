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
                    idToReturn = "AbstractLanguageID";
                    table = "tLanguagesMapping";
                    searchField = "LanguageName";
                    break;
                case MapType.Speciality:
                    idToReturn = "AbstractSpecialityID";
                    table = "tSpecialitiesMapping";
                    searchField = "SpecialityName";
                    break;
                case MapType.Unit:
                    idToReturn = "AbstractUnitID";
                    table = "tUnitsMapping";
                    searchField = "UnitName";
                    break;
                default:
                    throw new Exception("Unknown MapType.");
            }

            return DatabaseInterface.GetMappingForGeneralValues(idToReturn, table, TMSSystemID, searchField, itemName);

        }

        internal static object DoMappingToUplinkGeneral(MapType mapType, int TMSSystemID, string itemName)
        {
            string idToReturn, table, searchField;
            switch (mapType)
            {
                case MapType.Language:
                    idToReturn = "LanguageName";
                    table = "tLanguagesMapping";
                    searchField = "AbstractLanguageID";
                    break;
                case MapType.Speciality:
                    idToReturn = "SpecialityName";
                    table = "tSpecialitiesMapping";
                    searchField = "AbstractSpecialityID";
                    break;
                case MapType.Unit:
                    idToReturn = "UnitName";
                    table = "tUnitsMapping";
                    searchField = "AbstractUnitID";
                    break;
                default:
                    throw new Exception("Unknown MapType.");
            }

            return DatabaseInterface.GetMappingForGeneralValues(idToReturn, table, TMSSystemID, searchField, itemName);

        }

        internal static string DoMappingToUplinkCustomValues(MapType mapType, Link link, string projectValue)
        {
            string result = DatabaseInterface.GetMappingToUplinkValue(mapType, link, projectValue);
            if(String.IsNullOrEmpty(result))
            {
                // Do something to register new value
            }
            return result;
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