using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ProjectTasks
{
    public class IncrementBuildTask : Task 
    {
        [Required]
        public string SteamFileListPath { get; set; }
        
        public override bool Execute()
        {
            try
            {

            }
            catch (Exception e)
            {
                
            }

            return true;
        }
    }
}