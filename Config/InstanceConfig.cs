namespace MessHallAPI.Config
{
    public class InstanceConfig
    {
        /// <summary>
        /// Toggles launching Multiple Instances, Default: False
        /// </summary>
        public static bool MultipleInstancesEnabled = false;
        /// <summary>
        /// Amount of extra instances you want to load at once, Default: 0
        /// </summary>
        public static int InstanceAmount = 0;

        /// <summary>
        /// this is the code for your "LAN" lobby max of 6 chars, banned characters are
        /// Z S O N Q I B G
        /// </summary>
        public static string SessionName = "TEAMMH";
    }
}
