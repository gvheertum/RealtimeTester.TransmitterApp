namespace MCListener.TestTool.Testers
{
    public enum PingLookupResult
    {
        Unknown,
        /// <summary>
        /// Value is not a valid ping
        /// </summary>
        Invalid,
        /// <summary>
        /// The specific ping is found in th etable
        /// </summary>
        Found,
        /// <summary>
        /// The ping was from a different session
        /// </summary>
        NotMySession,
        /// <summary>
        /// The ping is from my session
        /// </summary>
        NotInCollection
    }
}