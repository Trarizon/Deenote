namespace Deenote.Models
{
    // Official json file has "lines" array, which represents a interval in which all notes has same speed,
    // but the array is actually duplicated for chart, as all note has its own speed property.
    // I dont know which property DEEMO II use to parse when game playing, I use note's property, and the
    // SpeedLine is just for serialization
    public partial struct SpeedLineData
    {
        public float StartTime;
        public float Speed;
        public WarningType WarningType;

        public SpeedLineData(float startTime, float speed = 1f, WarningType warningType= WarningType.SpeedChange)
        {
            StartTime = startTime;
            Speed = speed;
            WarningType = warningType;
        }
    }
}