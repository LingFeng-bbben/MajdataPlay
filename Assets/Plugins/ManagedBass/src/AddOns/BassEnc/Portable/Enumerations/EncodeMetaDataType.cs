namespace ManagedBass.Enc
{
    /// <summary>
    /// To be used with <see cref="BassEnc.CastSendMeta(int,EncodeMetaDataType,byte[])" /> to define the type of metadata to send.
    /// </summary>
    public enum EncodeMetaDataType
    {
        /// <summary>
        /// SHOUTcast v2 Cacheable Metadata (Content Info Metadata).
        /// </summary>
        XmlContent = 0x3000,

        /// <summary>
        /// SHOUTcast v2 Cacheable Metadata (Url Metadata).
        /// </summary>
        XmlUrl = 0x3001,

        /// <summary>
        /// SHOUTcast v2 Cacheable Metadata (Aol Radio format).
        /// </summary>
        XmlAol = 0x3901,

        /// <summary>
        /// SHOUTcast v2 Cacheable Metadata (SHOUTcast 2.0 format).
        /// </summary>
        XmlShoutcast = 0x3902,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Station logo image/jpeg).
        /// </summary>
        BinStationLogoJpg = 0x4000,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Station logo image/png).
        /// </summary>
        BinStationLogoPng = 0x4001,
        
        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Station logo image/bmp).
        /// </summary>
        BinStationLogoBmp = 0x4002,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Station logo image/gif).
        /// </summary>
        BinStationLogoGif = 0x4003,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Album art image/jpeg).
        /// </summary>
        BinAlbumArtJpg = 0x4100,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Album art image/png).
        /// </summary>
        BinAlbumArtPng = 0x4101,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Album art image/bmp).
        /// </summary>
        BinAlbumArtBmp = 0x4102,

        /// <summary>
        /// SHOUTcast v2 Cacheable Binary Metadata (Album art image/gif).
        /// </summary>
        BinAlbumArtGif = 0x4103,

        /// <summary>
        /// SHOUTcast v2 Pass-through Metadata (Time Remaining).
        /// </summary>
        XmlTimeRemaining = 0x5001
    }
}