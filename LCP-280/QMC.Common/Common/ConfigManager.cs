using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    public class ConfigManager
    {
        private static string g_strEquipmentName;
        private static readonly string g_strTeachingDataPath = "TeachingData";
        private static readonly string g_strRecipeDataPath = "RecipeData";
        private static readonly string g_strConfigPath = "Config";
        private static readonly string g_strRecipePath = "Recipe";
        private static readonly string g_strBackupPath = "Backup";

        private static readonly string g_strMotionFile = "Motion";
        private static readonly string g_strMotionGantryFile = "Gantry";
        private static readonly string g_strIOFile = "IO";
        private static readonly string g_strModuleListFile = "ModuleList";
        private static readonly string g_strInitializeSequenceFile = "InitializeSequence";
        private static readonly string g_strRecipeFile = "Recipe";
        private static readonly string g_strConfigFile = "Config";
        private static readonly string g_strExtension = "dat";
        private static readonly string g_strProductDataFile = "ProductData";

        private static readonly string g_strMotFile = "LCP-280.mot";    //   "Motor.mot";

        public static void SetEquipmentName(string strEquipmentName)
        {
            g_strEquipmentName = strEquipmentName;
        }

        public static string GetAjinMotorParameterFile()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("D:\\LCP-280");
            builder.AppendFormat("\\{0}", g_strEquipmentName); 
            builder.AppendFormat("\\{0}", g_strConfigPath);
            builder.AppendFormat("\\{0}", g_strMotFile);

            return builder.ToString();
        }

        public static string GetConfigPath()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("D:\\LCP-280");
            //builder.Append("D:\\Test");
            builder.AppendFormat("\\{0}", g_strEquipmentName);
            builder.AppendFormat("\\{0}", g_strConfigPath);

            return builder.ToString();
        }

        public static string GetTeachingDataPath()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("D:\\LCP-280");
            //builder.Append("D:\\Test");
            builder.AppendFormat("\\{0}", g_strEquipmentName);
            builder.AppendFormat("\\{0}", g_strConfigPath);
            builder.AppendFormat("\\{0}", g_strTeachingDataPath);

            return builder.ToString();
        }

        public static string GetRecipeDataPath()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("D:\\LCP-280");
            //builder.Append("D:\\Test");
            builder.AppendFormat("\\{0}", g_strEquipmentName);
            builder.AppendFormat("\\{0}", g_strRecipePath);
            builder.AppendFormat("\\{0}", g_strRecipeDataPath);

            return builder.ToString();
        }

        public static string GetRecipePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("D:\\LCP-280");
            //builder.Append("D:\\Test");
            builder.AppendFormat("\\{0}", g_strEquipmentName);
            builder.AppendFormat("\\{0}", g_strRecipePath);

            return builder.ToString();
        }

        public static string GetPatternImagePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("D:\\LCP-280");
            //builder.Append("D:\\Test");
            builder.AppendFormat("\\{0}", g_strEquipmentName);
            builder.AppendFormat("\\{0}", g_strRecipePath);
            builder.AppendFormat("\\{0}", "PatternImage");

            return builder.ToString();
        }

        public static string GetMotionFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strMotionFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetMotionGantryFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strMotionGantryFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetIOFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strIOFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetModuleFilePath(string strModuleName)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}.dat", strModuleName);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetModuleListFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strModuleListFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetInitializeSequenceFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strInitializeSequenceFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetRecipeFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetRecipePath());
            builder.AppendFormat("\\{0}", g_strRecipeFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetRecipeFilePath(string strRecipeName)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetRecipePath());
            builder.AppendFormat("\\{0}", strRecipeName);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetConfigFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strConfigFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetConfigFilePath(string strRecipeName)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strConfigFile);
            builder.AppendFormat("_{0}", strRecipeName);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetConfigBackupFilePath()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetConfigPath());
            builder.AppendFormat("\\{0}", g_strBackupPath);
            builder.AppendFormat("\\{0}", g_strConfigFile);
            builder.AppendFormat("_{0}", DateTime.Now.ToString("yyyyMMddhhmmssfff"));
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetRecipeBackupFilePath(string strBakupPath)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetRecipePath());
            builder.AppendFormat("\\{0}", g_strBackupPath);
            builder.AppendFormat("\\{0}", strBakupPath);
            builder.AppendFormat("\\{0}", g_strRecipeFile);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }

        public static string GetRecipeBackupFilePath(string strRecipeName, string strBakupPath)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetRecipePath());
            builder.AppendFormat("\\{0}", g_strBackupPath);
            builder.AppendFormat("\\{0}", strBakupPath);
            builder.AppendFormat("\\{0}", strRecipeName);
            builder.AppendFormat(".{0}", g_strExtension);

            return builder.ToString();
        }
    }
}
