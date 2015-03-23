using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Magic3D
{    
    public struct Trigger
    {        
        public MagicEventType Type;
        public GamePhases Phase;
        public CardInstance Card;
    }
}
