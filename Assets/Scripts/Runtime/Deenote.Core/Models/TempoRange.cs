#nullable enable

namespace Deenote.CoreB.Models
{
    public struct TempoRange
    {
        public Tempo Tempo;
        public float EndTime;

        public readonly float Length => EndTime - Tempo.StartTime;

        public TempoRange(Tempo tempo, float endTime) 
            => (Tempo, EndTime) = (tempo, endTime);

        public TempoRange(float bpm,float startTime, float endTime) 
            => (Tempo, EndTime) = (new Tempo(bpm, startTime), endTime);
    
        public readonly void Deconstruct(out Tempo tempo,out float endTime)
            => (tempo, endTime) = (Tempo, EndTime);
    }
}