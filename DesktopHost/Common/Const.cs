using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Think.Viewer.Common
{
    public class Const
    {
        public const int BUFFER_SIZE = 1024 * 4;

        public const byte STREAMING_OP_DOWN = 1;
        public const byte STREAMING_OP_UP = 2;
        public const byte STREAMING_OP_CLICK = 3;
        public const byte STREAMING_OP_DOUBLE_CLICK = 4;
        public const byte STREAMING_OP_MOVE = 5;

    }
}
