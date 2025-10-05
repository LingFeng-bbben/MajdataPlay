namespace ManagedBass
{
    /// <summary>
    /// EAX environment constants to be used with <see cref="Bass.SetEAXParameters" /> (Windows only).
    /// </summary>
    public enum EAXEnvironment
    {
        /// <summary>
        /// -1 = leave current EAX environment as is
        /// </summary>
        LeaveCurrent = -1,

        /// <summary>
        /// Generic
        /// </summary>
        Generic,

        /// <summary>
        /// Padded Cell
        /// </summary>
        PaddedCell,

        /// <summary>
        /// Room
        /// </summary>
        Room,

        /// <summary>
        /// Bathroom
        /// </summary>
        Bathroom,

        /// <summary>
        /// Livingroom
        /// </summary>
        Livingroom,

        /// <summary>
        /// Stoneroom
        /// </summary>
        Stoneroom,

        /// <summary>
        /// Auditorium
        /// </summary>
        Auditorium,

        /// <summary>
        /// Concert Hall
        /// </summary>
        ConcertHall,

        /// <summary>
        /// Cave
        /// </summary>
        Cave,

        /// <summary>
        /// Arena
        /// </summary>
        Arena,

        /// <summary>
        /// Hangar
        /// </summary>
        Hangar,

        /// <summary>
        /// Carpeted Hallway
        /// </summary>
        CarpetedHallway,

        /// <summary>
        /// Hallway
        /// </summary>
        Hallway,

        /// <summary>
        /// Stone Corridor
        /// </summary>
        StoneCorridor,

        /// <summary>
        /// Alley
        /// </summary>
        Alley,

        /// <summary>
        /// Forest
        /// </summary>
        Forest,

        /// <summary>
        /// City
        /// </summary>
        City,

        /// <summary>
        /// Mountains
        /// </summary>
        Mountains,

        /// <summary>
        /// Quarry
        /// </summary>
        Quarry,

        /// <summary>
        /// Plain
        /// </summary>
        Plain,

        /// <summary>
        /// Parkinglot
        /// </summary>
        ParkingLot,

        /// <summary>
        /// Sewer Pipe
        /// </summary>
        SewerPipe,

        /// <summary>
        /// Underwater
        /// </summary>
        Underwater,

        /// <summary>
        /// Drugged
        /// </summary>
        Drugged,

        /// <summary>
        /// Dizzy
        /// </summary>
        Dizzy,

        /// <summary>
        /// Psychotic
        /// </summary>
        Psychotic,

        /// <summary>
        /// Total number of environments
        /// </summary>
        Count
    }
}