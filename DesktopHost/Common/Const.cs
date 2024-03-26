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

        public const byte STREAMING_OP_TYPE_LEFT_CLICK = 1;
        public const byte STREAMING_OP_TYPE_RIGHT_CLICK = 2;
        public const byte STREAMING_OP_TYPE_LEFT_DOWN = 3;
        public const byte STREAMING_OP_TYPE_RIGHT_DOWN = 4;
        public const byte STREAMING_OP_TYPE_WHEEL = 5;
        public const byte STREAMING_OP_TYPE_MOVE = 6;
        public const byte STREAMING_OP_TYPE_DOUBLE_CLICK = 7;

    }
}
