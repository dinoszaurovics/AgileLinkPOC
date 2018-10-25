using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BeLazy
{
    internal static class ScriptingHelper
    {   
        internal static string EvalutaScriptResult(string script, AbstractProject project)
        {

            Regex placeholderMatcher = new Regex(@"(?<!\\)@[\p{L}\d]+?(?<!\\)@");
            MatchCollection placeholders = placeholderMatcher.Matches(script);
            string evaluatedScript = script;
            PropertyInfo[] projectProps = project.GetType().GetProperties();
            
            foreach (Match placeholder in placeholders)
            {
                string propName = placeholder.Value.Trim('@');

                try
                {
                    PropertyInfo projectProp = projectProps.Where(x => x.Name == propName).First();
                    string replaceValue = projectProp.GetValue(project).ToString();
                    evaluatedScript = evaluatedScript.Replace("@" + propName +"@", replaceValue);
                }
                catch (Exception ex)
                {
                    Log.AddLog("Evaluation of " + propName + " failed: " + ex.Message, ErrorLevels.Error);
                }
            }

            return evaluatedScript;
        }
    }
}