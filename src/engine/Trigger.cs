using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Magic3D
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Trigger
    {
        [FieldOffset(0)]
        public MagicEventType Type;
        [FieldOffset(4)]
        public GamePhases Phase;
        [FieldOffset(8)]
        public CardInstance Card;
    }
}
