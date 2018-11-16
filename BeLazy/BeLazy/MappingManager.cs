using System;

namespace BeLazy
{
    internal class MappingManager
    {
        internal static int DoMappingToAbstract(MapType mapType, int TMSSystemTypeID, string itemName)
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

            return DatabaseInterface.GetMappingForGeneralValues(idToReturn, table, TMSSystemTypeID, searchField, itemName);

        }

        internal static int DoMappingToUplinkGeneral(MapType mapType, int TMSSystemTypeID, string itemName)
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

            return DatabaseInterface.GetMappingForGeneralValues(idToReturn, table, TMSSystemTypeID, searchField, itemName);

        }

        internal static string GetScriptedValue(Link link, MapType mapType, AbstractProject project)
        {
            string script = DatabaseInterface.GetMappingScript(link.linkID, mapType);
            string result = ScriptingHelper.EvalutaScriptResult(script, project);
            return result;
        }

        
        internal static string DoMappingToUplinkCustomValues(MapType mapType, Link link, string projectValue)
        {
            string result = DatabaseInterface.GetMappingToUplinkValue(mapType, link, projectValue);
            if(String.IsNullOrEmpty(result))
            {
                throw new Exception("Mapped value is empty: " + mapType + " - " + projectValue);
            }
            return result;
        }

    }

    public enum MapType
    {
        Language,
        Speciality,
        Workflow,
        Unit,
        ProjectName,
        Contact
    }

}