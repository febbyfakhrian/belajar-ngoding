namespace PLCCommunication
{
    public static class WritePLCAddress
    {
        public static readonly byte[] READ = System.Text.Encoding.ASCII.GetBytes("%01#RCSR0401**\r");

        public static readonly byte[] NEXT = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04001**\r");
        public static readonly byte[] POST_NEXT = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04000**\r");

        public static readonly byte[] PASS = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04021**\r");
        public static readonly byte[] POST_PASS = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04020**\r");

        public static readonly byte[] FAIL = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04031**\r");
        public static readonly byte[] POST_FAIL = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04030**\r");
    }

    public static class WritePLCAddressV2
    {
        // Camera 0
        public static readonly byte[] READ0 = System.Text.Encoding.ASCII.GetBytes("%01#RCSR0401**\r");

        public static readonly byte[] NEXT00 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04001**\r");
        public static readonly byte[] POST_NEXT00 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04000**\r");
        public static readonly byte[] NEXT01 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR040A1**\r");
        public static readonly byte[] POST_NEXT01 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR040A0**\r");
        public static readonly byte[] NEXT02 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR040B1**\r");
        public static readonly byte[] POST_NEXT02 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR040B0**\r");
        public static readonly byte[] NEXT03 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR040C1**\r");
        public static readonly byte[] POST_NEXT03 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR040C0**\r");

        public static readonly byte[] PASS0 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04021**\r");
        public static readonly byte[] POST_PASS0 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04020**\r");
        public static readonly byte[] FAIL0 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04031**\r");
        public static readonly byte[] POST_FAIL0 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04030**\r");

        // Camera 1
        public static readonly byte[] READ1 = System.Text.Encoding.ASCII.GetBytes("%01#RCSR0411**\r");
        public static readonly byte[] NEXT10 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04101**\r");
        public static readonly byte[] POST_NEXT10 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04100**\r");
        public static readonly byte[] NEXT11 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR041A1**\r");
        public static readonly byte[] POST_NEXT11 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR041A0**\r");

        public static readonly byte[] PASS1 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04121**\r");
        public static readonly byte[] POST_PASS1 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04120**\r");
        public static readonly byte[] FAIL1 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04131**\r");
        public static readonly byte[] POST_FAIL1 = System.Text.Encoding.ASCII.GetBytes("%01#WCSR04130**\r");
    }

    public static class PLCResponseMessage
    {
        public static readonly byte[] TRUE = System.Text.Encoding.ASCII.GetBytes("%01$RC120\r");
        public static readonly byte[] FALSE = System.Text.Encoding.ASCII.GetBytes("%01$RC021\r");
        public static readonly byte[] SUCCESS = System.Text.Encoding.ASCII.GetBytes("%01$WC14\r");
    }
}
