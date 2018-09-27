using System;

namespace BeLazy
{
    internal class MappingHelper
    {
        internal static string DoMappingToAbstract(MapType speciality, int downlinkBTMSSystemID, string specialty)
        {
            throw new NotImplementedException();
        }
    }

    public enum MapType
    {
        Speciality, Language,
        Workflow,
        Unit
    }

}