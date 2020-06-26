using System;

namespace FMOD
{
    public enum SOUND_TYPE
    {
    }

    public enum SOUND_FORMAT
    {
    }
    public enum TIMEUNIT
    {
        MS,
    }
    public enum INITFLAGS
    {
        NORMAL
    }

    public enum MODE
    {
        OPENMEMORY,
    }
    public class System
    {
        public RESULT createSound(byte[] buf, MODE m, ref CREATESOUNDEXINFO info, out Sound s) => throw new NotImplementedException();
        public RESULT getVersion(out uint v)
        {
            v = 0;
            return RESULT.OK;
        }
        public void close() {}
        public void release() {}

        public RESULT init(int max, INITFLAGS flags, IntPtr extra) => throw new NotImplementedException();
        public RESULT playSound(Sound s, ChannelGroup chg, bool paused, out Channel ch) => throw new NotImplementedException();
    }

    public class Error
    {
        public static string String(RESULT r) => "";
    }

    public class VERSION
    {
        public const int number = 0;
    }

    public class Sound
    {
        public RESULT release() => throw new NotImplementedException();
        public RESULT getNumSubSounds(out int n) => throw new NotImplementedException();
        public RESULT getSubSound(int n, out Sound s) => throw new NotImplementedException();
        public RESULT getLength(out uint t, TIMEUNIT u) => throw new NotImplementedException();
        public RESULT getFormat(out SOUND_TYPE t, out SOUND_FORMAT f, out int nch, out int bits) => throw new NotImplementedException();
        public RESULT getDefaults(out float freq, out int prio) => throw new NotImplementedException();
    }
    
    public class ChannelGroup {}

    public class CREATESOUNDEXINFO
    {
        public uint length;
    }

    public class Channel
    {
        public RESULT getCurrentSound(out Sound s) => throw new NotImplementedException();
        public RESULT stop() => throw new NotImplementedException();
    }

    public enum RESULT
    {
        OK,
        ERR_CHANNEL_STOLEN,
    }

    public static class Factory
    {
        public static RESULT System_Create(out System system) => throw new NotImplementedException();
    }

}